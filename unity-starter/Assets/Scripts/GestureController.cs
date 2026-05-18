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

    [Header("References (auto-wired in GameBootstrap)")]
    public PlayerHealth health;
    public PlayerShooter shooter;
    public DragonBeam dragon;

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
        // GUARD: held while fist
        if (health) health.SetGuarding(fist);
        // DRAGON: thumbs-up posture is also the charge gate; firing happens
        // on the falling edge with full charge (handled inside DragonBeam).
        if (shooter) shooter.Suppressed = thumbsUp;
        if (dragon) dragon.UpdateGesture(thumbsUp, prevThumbsUp);

        // DASH: swipe detection from fingertip velocity
        var tip = lm[8];
        float dx = tip.x - lastTipX, dy = tip.y - lastTipY;
        float sp = Mathf.Sqrt(dx * dx + dy * dy);
        if (sp > 0.085f && Time.time - lastDashAt > 0.5f && !fist && !pinch)
        {
            lastDashAt = Time.time;
            DoDash(new Vector2(dx / sp, -dy / sp));   // y is flipped to world
        }
        lastTipX = tip.x; lastTipY = tip.y;

        prevPinch = pinch; prevFist = fist; prevPeace = peace;
        prevThumbBent = bent; prevThumbsUp = thumbsUp;
    }

    void FireBomb()
    {
        // Clear every enemy bullet on screen, damage all enemies
        foreach (var b in FindObjectsOfType<Bullet>())
            if (!b.isPlayerShot) Destroy(b.gameObject);
        foreach (var e in FindObjectsOfType<Enemy>()) e.TakeDamage(20);
        CameraShake.Pulse(0.4f, 0.3f);
        if (GameDirector.Instance) GameDirector.Instance.AddScore(50);
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
