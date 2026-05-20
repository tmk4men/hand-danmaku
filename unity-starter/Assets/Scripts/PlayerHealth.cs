using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 5;
    public float invulnSeconds = 2f;       // JS: 120 frames

    public int lives;
    public float guardMax = 10f;           // JS: guard.remaining caps at 600 frames
    public float guardRemaining;

    private float invulnUntil;
    private bool guarding, guardRequested;
    private CircleCollider2D col;

    void Awake()
    {
        lives = Persistence.ApplyLife(maxLives);
        guardRemaining = guardMax;
        col = GetComponent<CircleCollider2D>();
        col.radius = 0.06f;
        col.isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public bool IsInvulnerable() => Time.time < invulnUntil;
    public bool IsGuarding() => guarding;
    public void SetGuarding(bool g) => guardRequested = g;     // honoured in Update vs budget
    public void GrantInvuln(float s) => invulnUntil = Mathf.Max(invulnUntil, Time.time + s);
    public void AddLife() { lives = Mathf.Min(lives + 1, 7); if (HUD.Instance) HUD.Instance.Refresh(); }
    public void AddGuard(float s) { guardRemaining = Mathf.Min(guardMax, guardRemaining + s); }
    public void SetFocus(bool on) { if (col) col.radius = on ? 0.035f : 0.06f; }   // tiny hitbox in FOCUS

    public void TakeHit()
    {
        if (IsInvulnerable() || IsGuarding()) return;
        lives--;
        invulnUntil = Time.time + invulnSeconds;
        CameraShake.Pulse(0.25f, 0.15f);
        Particles.Burst(transform.position, Color.white, 24);
        ProceduralSFX.PlayerHit();
        if (ComboMeter.Instance) ComboMeter.Instance.Reset();
        if (lives <= 0) GameDirector.Instance.GameOver();
        if (HUD.Instance) HUD.Instance.Refresh();
    }

    public void InstantKill()
    {
        if (IsInvulnerable() || IsGuarding()) return;
        lives = 0;
        invulnUntil = Time.time + invulnSeconds;
        CameraShake.Pulse(0.5f, 0.35f);
        GameDirector.Instance.GameOver();
    }

    void Update()
    {
        // Guard is only active while the fist is held AND budget remains.
        guarding = guardRequested && guardRemaining > 0f;
        if (guarding)
        {
            guardRemaining -= Time.deltaTime;
            if (guardRemaining <= 0f) { guardRemaining = 0f; guarding = false; }
        }

        // Blink while invulnerable
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = !(IsInvulnerable() && Mathf.FloorToInt(Time.time * 12) % 2 == 0);
    }
}
