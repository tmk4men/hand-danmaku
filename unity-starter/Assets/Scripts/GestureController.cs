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
    private SpriteRenderer guardRing;   // visible shield ring while guarding
    private PlayerShip playerShip;      // for FOCUS movement smoothing

    /// <summary>On-screen live gesture readout. Off for release builds.</summary>
    public static bool ShowDebug = false;
    public static float DashSpeed = 0.065f;   // fingertip speed to trigger DASH

    [Header("References (auto-wired in GameBootstrap)")]
    public PlayerHealth health;
    public PlayerShooter shooter;
    public DragonBeam dragon;

    void Start()
    {
        if (!health) return;
        playerSr = health.GetComponent<SpriteRenderer>();
        playerShip = health.GetComponent<PlayerShip>();

        var ringGO = new GameObject("GuardRing");
        ringGO.transform.SetParent(health.transform, false);
        ringGO.transform.localPosition = Vector3.zero;
        guardRing = ringGO.AddComponent<SpriteRenderer>();
        guardRing.sprite = SpriteFactory.RingSprite(28, 4, Color.white);
        guardRing.color = new Color(0.5f, 1f, 0.83f, 0.9f);
        guardRing.sortingOrder = 6;
        ringGO.SetActive(false);
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

        // BOMB: pinch rising edge — consumes a bomb (no free spam, no score)
        if (pinch && !prevPinch && GameDirector.Instance != null && GameDirector.Instance.UseBomb())
        {
            FireBomb();
            if (health) health.GrantInvuln(1f);
        }
        // GUARD: held while fist (+ visible cyan tint so it's obvious)
        if (health) health.SetGuarding(fist);
        if (playerSr) playerSr.color = fist ? new Color(0.5f, 1f, 0.83f) : Color.white;
        if (guardRing)
        {
            if (guardRing.gameObject.activeSelf != fist) guardRing.gameObject.SetActive(fist);
            if (fist)
            {
                guardRing.transform.localScale = Vector3.one * (0.9f + 0.12f * Mathf.Sin(Time.time * 12f));
                var rc = guardRing.color; rc.a = 0.65f + 0.35f * Mathf.Sin(Time.time * 8f); guardRing.color = rc;
            }
        }
        // DRAGON: thumbs-up posture is also the charge gate; firing happens
        // on the falling edge with full charge (handled inside DragonBeam).
        if (shooter) shooter.Suppressed = thumbsUp;
        if (dragon) dragon.UpdateGesture(thumbsUp, prevThumbsUp);

        // FOCUS: peace sign = precise homing fire, slow movement, tiny hitbox
        if (shooter) shooter.Focus = peace;
        if (playerShip) playerShip.smoothing = peace ? 0.12f : 0.25f;
        if (health) health.SetFocus(peace);

        // BULLET TIME: thumb-bend rising edge consumes a charge -> slow enemy bullets
        if (bent && !prevThumbBent && GameDirector.Instance != null && GameDirector.Instance.UseCharge())
        {
            Slowmo.Trigger(3f);
            Fx.ScreenFlash(new Color(0.4f, 0.6f, 1f, 0.22f), 0.2f);
            CameraShake.Pulse(0.15f, 0.1f);
        }

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
        Vector3 bp = health ? health.transform.position
                   : (GameDirector.Instance && GameDirector.Instance.Player
                        ? GameDirector.Instance.Player.position : Vector3.zero);
        Fx.Bomb(bp);   // screen flash + expanding ring

        // Clear every enemy bullet on screen, damage all enemies & boss
        foreach (var b in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            if (!b.isPlayerShot) Destroy(b.gameObject);
        foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None)) e.TakeDamage(30);
        foreach (var bo in FindObjectsByType<Boss>(FindObjectsSortMode.None)) bo.TakeDamage(35);
        foreach (var m in FindObjectsByType<Meteor>(FindObjectsSortMode.None)) Destroy(m.gameObject);
        CameraShake.Pulse(0.5f, 0.35f);
        ProceduralSFX.Bomb();
        // JS bomb grants no score (prevents bomb-spam farming).
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
        t.position += (Vector3)(dir * 0.9f);   // JS ~70px teleport
        health.GrantInvuln(0.45f);

        // Clear enemy bullets around the landing spot ("DASH CUT x N")
        int cut = 0;
        foreach (var b in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            if (!b.isPlayerShot && (b.transform.position - t.position).sqrMagnitude < 0.55f * 0.55f)
            { Destroy(b.gameObject); cut++; }
        if (cut > 0)
            FloatingText.Spawn(t.position, Strings.T("dashCut", cut), new Color(0.68f, 0.96f, 1f), 16);

        CameraShake.Pulse(0.15f, 0.1f);
    }
}
