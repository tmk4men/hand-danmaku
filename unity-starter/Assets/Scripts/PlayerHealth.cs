using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 5;
    public float invulnSeconds = 1.5f;

    public int lives;
    private float invulnUntil;
    private bool guarding;

    void Awake()
    {
        lives = maxLives;
        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.06f;
        col.isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public bool IsInvulnerable() => Time.time < invulnUntil;
    public bool IsGuarding() => guarding;
    public void SetGuarding(bool g) => guarding = g;

    public void TakeHit()
    {
        if (IsInvulnerable() || IsGuarding()) return;
        lives--;
        invulnUntil = Time.time + invulnSeconds;
        CameraShake.Pulse(0.25f, 0.15f);
        ProceduralSFX.PlayerHit();
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
        // Blink while invulnerable
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = !(IsInvulnerable() && Mathf.FloorToInt(Time.time * 12) % 2 == 0);
    }
}
