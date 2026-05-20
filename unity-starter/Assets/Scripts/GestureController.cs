using UnityEngine;

/// <summary>
/// Watches HandManager landmarks each frame, debounces gesture transitions,
/// and drives the player abilities (bomb, guard, focus, dash, dragon).
/// </summary>
public class GestureController : MonoBehaviour
{
    private bool prevPinch, prevFist, prevPeace, prevThumbBent, prevThumbsUp;
    private float lastTipX = 0.5f, lastTipY = 0.5f;
    private float lastDashAt = -10;
    private float lastSpeed;            // fingertip speed, for debug overlay
    private SpriteRenderer playerSr;    // for guard tint

    /// <summary>On-screen live gesture readout. Set false for the final build.</summary>
    public static bool ShowDebug = true;
    public static float DashSpeed = 0.065f;   // fingertip speed to trigger DASH

    [Header("References (auto-wired in GameBootstrap)")]
    public PlayerHealth health;
    public PlayerShooter shooter;
    public DragonBeam dragon;

    void Start()
    {
        if (health) playerSr = health.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var hm = HandManager.Instance;
        if (hm == null || !hm.HandSeen)
        {
            // Lose all sticky states on hand loss
            if (health) health.SetGuarding(false);
            if (shooter) shooter.Suppressed = false;
            prevPinch = prevFist = prevPeace = prevThumbBent = prevThumbsUp = false;
            return;
        }
        var lm = hm.Landmarks;

        bool pinch    = GestureClassifier.IsPinch(lm);
        bool fist     = GestureClassifier.IsFist(lm);
        bool peace    = GestureClassifier.IsPeace(lm);
        bool bent     = GestureClassifier.IsThumbBent(lm);
        bool thumbsUp = GestureClassifier.IsThumbsUp(lm);

        // BOMB: pinch rising edge
        if (pinch && !prevPinch) FireBomb();
        // GUARD: held while fist (+ visible cyan tint so it's obvious)
        if (health) health.SetGuarding(fist);
        if (playerSr) playerSr.color = fist ? new Color(0.4f, 0.9f, 1f) : Color.white;
        // DRAGON: thumbs-up posture is also the charge gate; firing happens
        // on the falling edge with full charge (handled inside DragonBeam).
        if (shooter) shooter.Suppressed = thumbsUp;
        if (dragon) dragon.UpdateGesture(thumbsUp, prevThumbsUp);

        // DASH: swipe detection from fingertip velocity
        var tip = lm[8];
        float dx = tip.x - lastTipX, dy = tip.y - lastTipY;
        float sp = Mathf.Sqrt(dx * dx + dy * dy);
        lastSpeed = sp;
        if (sp > DashSpeed && Time.time - lastDashAt > 0.5f && !fist && !pinch)
        {
            lastDashAt = Time.time;
            DoDash(new Vector2(dx / sp, -dy / sp));   // y is flipped to world
            CameraShake.Pulse(0.2f, 0.12f);           // visible dash feedback
        }
        lastTipX = tip.x; lastTipY = tip.y;

        prevPinch = pinch; prevFist = fist; prevPeace = peace;
        prevThumbBent = bent; prevThumbsUp = thumbsUp;
    }

    void FireBomb()
    {
        // Clear every enemy bullet on screen, damage all enemies & boss
        foreach (var b in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            if (!b.isPlayerShot) Destroy(b.gameObject);
        foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None)) e.TakeDamage(20);
        foreach (var bo in FindObjectsByType<Boss>(FindObjectsSortMode.None)) bo.TakeDamage(40);
        foreach (var m in FindObjectsByType<Meteor>(FindObjectsSortMode.None)) Destroy(m.gameObject);
        CameraShake.Pulse(0.5f, 0.35f);
        ProceduralSFX.Bomb();
        if (GameDirector.Instance) GameDirector.Instance.AddScore(50);
    }

    void OnGUI()
    {
        if (!ShowDebug) return;
        var style = new GUIStyle { fontSize = 18, normal = { textColor = Color.white } };
        GUI.Box(new Rect(6, 6, 560, 70), GUIContent.none);
        var hm = HandManager.Instance;
        if (hm == null) { GUI.Label(new Rect(12, 12, 560, 30), "HandManager: null", style); return; }
        if (!hm.HandSeen) { GUI.Label(new Rect(12, 12, 560, 30), "hand: NOT seen — show your hand to the camera", style); return; }
        var lm = hm.Landmarks;
        string l1 = $"pinchRatio={GestureClassifier.PinchRatioNow(lm):0.00} (fires < {GestureClassifier.PinchRatio:0.00})   tipSpeed={lastSpeed:0.000} (dash > {DashSpeed:0.000})   curls={GestureClassifier.CurledCount(lm)}";
        string l2 = $"PINCH={GestureClassifier.IsPinch(lm)}  FIST={GestureClassifier.IsFist(lm)}  PEACE={GestureClassifier.IsPeace(lm)}  THUMBSUP={GestureClassifier.IsThumbsUp(lm)}  BENT={GestureClassifier.IsThumbBent(lm)}";
        GUI.Label(new Rect(12, 12, 560, 30), l1, style);
        GUI.Label(new Rect(12, 40, 560, 30), l2, style);
    }

    void DoDash(Vector2 dir)
    {
        if (health == null) return;
        var t = health.transform;
        t.position += (Vector3)(dir * 0.6f);
        // brief invuln equivalent: piggyback on the existing invuln window
        var typeof_ph = typeof(PlayerHealth);
        var field = typeof_ph.GetField("invulnUntil",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            float cur = (float)field.GetValue(health);
            field.SetValue(health, Mathf.Max(cur, Time.time + 0.45f));
        }
        CameraShake.Pulse(0.15f, 0.1f);
    }
}
