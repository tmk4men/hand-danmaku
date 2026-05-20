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

    // Laser (stage 2+ pattern 3)
    private GameObject laserGO;
    private SpriteRenderer laserSr;
    private float laserAngle;     // degrees
    private int laserPhase;       // 0 idle, 1 charge, 2 fire, 3 cooldown
    private float laserTimer;

    public static Boss Spawn()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        var go = new GameObject("Boss");
        int themeIdx = GameDirector.Instance ? GameDirector.Instance.Stage - 1 : 0;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Boss(themeIdx);    // per-theme 18x16 boss sprite
        sr.sortingOrder = 6;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.42f; col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
        var b = go.AddComponent<Boss>();
        b.entryFrom = new Vector3(0, halfH + 1f, 0);
        b.entryTo   = new Vector3(0, halfH - 1.5f, 0);
        go.transform.position = b.entryFrom;
        go.transform.localScale = Vector3.one * 1.8f;   // sprite is now 18x16 (was 8x8)
        int stage = GameDirector.Instance ? GameDirector.Instance.Stage : 1;
        b.maxHp = 200 + stage * 80;        // JS scaling (was a flat 1200 = 4x too tanky)
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

        // Pattern rotation; pattern 3 becomes the laser sweep from stage 2 on.
        int pattern = ((int)(elapsed / patternDuration)) % 4;
        bool useLaser = pattern == 3 && (GameDirector.Instance ? GameDirector.Instance.Stage : 1) >= 2;

        if (useLaser)
        {
            UpdateLaser();
        }
        else
        {
            if (laserGO) laserGO.SetActive(false);
            laserPhase = 0;
            if (Time.time >= nextFire)
            {
                FirePattern(pattern);
                nextFire = Time.time + PatternInterval(pattern);
            }
        }
    }

    // ---------- Laser ----------
    void UpdateLaser()
    {
        var pl = GameDirector.Instance ? GameDirector.Instance.Player : null;
        if (pl == null) return;
        EnsureLaser();

        switch (laserPhase)
        {
            case 0:   // begin charge
                laserPhase = 1; laserTimer = 0.7f;
                laserAngle = AngleToDeg(pl);
                ProceduralSFX.Warning();
                break;
            case 1:   // charging: track player, pulsing thin guide
                laserTimer -= Time.deltaTime;
                laserAngle = Mathf.LerpAngle(laserAngle, AngleToDeg(pl), 0.08f);
                DrawLaser(true);
                if (laserTimer <= 0f) { laserPhase = 2; laserTimer = 1.3f; CameraShake.Pulse(0.6f, 0.5f); ProceduralSFX.Bomb(); }
                break;
            case 2:   // firing: slow sweep + collision
                laserTimer -= Time.deltaTime;
                laserAngle = Mathf.MoveTowardsAngle(laserAngle, AngleToDeg(pl), 25f * Time.deltaTime);
                DrawLaser(false);
                CheckLaserHit(pl);
                if (laserTimer <= 0f) { laserPhase = 3; laserTimer = 0.8f; if (laserGO) laserGO.SetActive(false); }
                break;
            default:  // cooldown
                laserTimer -= Time.deltaTime;
                if (laserTimer <= 0f) laserPhase = 0;
                break;
        }
    }

    float AngleToDeg(Transform pl)
    {
        Vector2 d = (Vector2)pl.position - (Vector2)transform.position;
        return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
    }

    void EnsureLaser()
    {
        if (laserGO != null) return;
        laserGO = new GameObject("BossLaser");
        laserGO.transform.SetParent(transform, true);
        laserSr = laserGO.AddComponent<SpriteRenderer>();
        laserSr.sprite = SpriteFactory.WhitePixel();
        laserSr.sortingOrder = 7;
        laserGO.SetActive(false);
    }

    void DrawLaser(bool charging)
    {
        laserGO.SetActive(true);
        const float L = 26f;
        float rad = laserAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 bp = transform.position;
        laserGO.transform.position = new Vector3(bp.x + dir.x * L * 0.5f, bp.y + dir.y * L * 0.5f, 0f);
        laserGO.transform.rotation = Quaternion.Euler(0, 0, laserAngle);

        float w = charging ? 0.06f : 0.5f;
        Vector3 ps = transform.lossyScale;   // counter the boss's scale so width is world-units
        laserGO.transform.localScale = new Vector3(
            L / Mathf.Max(0.001f, ps.x), w / Mathf.Max(0.001f, ps.y), 1f) * SpriteFactory.PPU;

        laserSr.color = charging
            ? new Color(1f, 0.2f, 0.4f, 0.45f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 30f)))
            : new Color(1f, 0.47f, 0.78f, 0.92f);
    }

    void CheckLaserHit(Transform pl)
    {
        Vector2 p = pl.position, bp = transform.position;
        float rad = laserAngle * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad), sa = Mathf.Sin(rad);
        float dx = p.x - bp.x, dy = p.y - bp.y;
        float along = dx * ca + dy * sa;
        float across = -dx * sa + dy * ca;
        if (along > 0.4f && Mathf.Abs(across) < 0.32f)
        {
            var ph = pl.GetComponent<PlayerHealth>();
            if (ph != null && !ph.IsInvulnerable() && !ph.IsGuarding()) ph.TakeHit();
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
            // visible red flash (sprite colour is baked, so tint to red then back)
            sr.color = new Color(1f, 0.55f, 0.55f);
            CancelInvoke(nameof(ResetTint));
            Invoke(nameof(ResetTint), 0.06f);
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
        FloatingText.Spawn(transform.position, "BOSS DOWN +3000", new Color(1f, 0.82f, 0.4f), 28);
        // drop all six items
        for (int i = 0; i < 6; i++)
            Item.Spawn(transform.position + Random.insideUnitSphere * 0.3f,
                       (ItemType)i);
        Particles.Burst(transform.position, new Color(1f, 0.47f, 0.78f), 40);
        ProceduralSFX.BossDie();
        CameraShake.Pulse(0.8f, 0.6f);
        GameDirector.Instance?.AdvanceStage();   // next stage + theme + bomb
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeHit();
    }
}
