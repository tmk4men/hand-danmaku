using UnityEngine;

/// <summary>
/// One-of-a-kind enemy with a state machine and 3-4 attack patterns
/// matching the JS version. Spawned by EnemySpawner after enough waves.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Boss : MonoBehaviour
{
    public int maxHp = 1200;
    public int hp;
    public float entryDuration = 2.2f;
    public float patternDuration = 8f;

    public Color bodyColor = new Color(0.84f, 0.27f, 0.48f);

    private float entryTimer;
    private bool entered;
    private float elapsed;
    private float nextFire;

    private SpriteRenderer sr;
    private Vector3 entryFrom, entryTo;

    public static Boss Spawn()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        var go = new GameObject("Boss");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Enemy(new Color(0.84f, 0.27f, 0.48f));
        sr.sortingOrder = 6;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.42f; col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
        var b = go.AddComponent<Boss>();
        b.entryFrom = new Vector3(0, halfH + 1f, 0);
        b.entryTo   = new Vector3(0, halfH - 1.5f, 0);
        go.transform.position = b.entryFrom;
        go.transform.localScale = Vector3.one * 2.6f;
        b.hp = b.maxHp;
        b.sr = sr;
        ProceduralSFX.Warning();
        CameraShake.Pulse(0.4f, 0.4f);
        return b;
    }

    void Update()
    {
        if (!entered)
        {
            entryTimer += Time.deltaTime;
            float t = Mathf.Clamp01(entryTimer / entryDuration);
            transform.position = Vector3.Lerp(entryFrom, entryTo,
                                              1 - Mathf.Pow(1 - t, 3));
            if (t >= 1f) entered = true;
            return;
        }

        elapsed += Time.deltaTime;
        // Side-to-side drift
        float sway = Mathf.Sin(elapsed * 0.5f) * 1.8f;
        transform.position = new Vector3(sway, entryTo.y, 0);

        // Pattern rotation
        int pattern = ((int)(elapsed / patternDuration)) % 4;
        if (Time.time >= nextFire)
        {
            FirePattern(pattern);
            nextFire = Time.time + PatternInterval(pattern);
        }
    }

    float PatternInterval(int p)
    {
        switch (p)
        {
            case 0: return 0.45f;   // wide aimed fan
            case 1: return 0.12f;   // spiral
            case 2: return 0.9f;    // ring burst + aimed
            default: return 1.6f;   // laser-substitute: big aimed plus
        }
    }

    void FirePattern(int p)
    {
        var pl = GameDirector.Instance != null ? GameDirector.Instance.Player : null;
        if (pl == null) return;
        Vector2 origin = (Vector2)transform.position + Vector2.down * 0.4f;
        Vector2 toPl = ((Vector2)pl.position - origin).normalized;
        float baseAng = Mathf.Atan2(toPl.y, toPl.x);

        if (p == 0)
        {
            for (int i = -2; i <= 2; i++)
            {
                float a = baseAng + i * 0.18f;
                Bullet.Spawn(origin,
                    new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3.0f,
                    new Color(1f, 0.47f, 0.78f), false, 1, 0.07f);
            }
        }
        else if (p == 1)
        {
            float ang = elapsed * 4f;
            for (int k = 0; k < 3; k++)
            {
                float a = ang + k * Mathf.PI * 2f / 3f;
                Bullet.Spawn(origin,
                    new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 2.5f,
                    new Color(0.6f, 0.95f, 1f), false);
            }
        }
        else if (p == 2)
        {
            int n = 18;
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n + elapsed * 0.5f;
                Bullet.Spawn(origin,
                    new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 2.0f,
                    new Color(1f, 0.95f, 0.4f), false);
            }
            // plus aimed slow
            Bullet.Spawn(origin, toPl * 3.5f, new Color(1f, 0.3f, 0.3f), false, 1, 0.09f);
        }
        else
        {
            // big aimed plus to substitute the laser for the starter
            for (int i = -3; i <= 3; i++)
            {
                float a = baseAng + i * 0.14f;
                Bullet.Spawn(origin,
                    new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3.4f,
                    new Color(1f, 0.95f, 0.4f), false);
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (sr != null)
        {
            // brief white flash
            sr.color = Color.white;
            Invoke(nameof(ResetTint), 0.05f);
        }
        if (hp <= 0) Die();
    }

    void ResetTint()
    {
        if (sr != null) sr.color = Color.white;
    }

    void Die()
    {
        GameDirector.Instance?.AddScore(3000);
        // drop all six items
        for (int i = 0; i < 6; i++)
            Item.Spawn(transform.position + Random.insideUnitSphere * 0.3f,
                       (ItemType)i);
        ProceduralSFX.BossDie();
        CameraShake.Pulse(0.8f, 0.6f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeHit();
    }
}
