using UnityEngine;

/// <summary>
/// Generic projectile. Set via Configure(); destroys itself off-screen.
/// Uses CircleCollider2D + Trigger collision.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public Vector2 velocity;
    public int damage = 1;
    public bool isPlayerShot;
    public bool homing;            // focus shots steer toward the nearest target
    public float lifetime = 6f;
    public float radius = 0.06f;

    private float age;

    public void SetSprite(Sprite s)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.sprite = s;
    }

    public static Bullet Spawn(Vector2 pos, Vector2 vel, Color color,
        bool playerShot, int dmg = 1, float r = 0.06f)
    {
        var go = new GameObject(playerShot ? "PlayerBullet" : "EnemyBullet");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Bullet(color);
        sr.sortingOrder = 5;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = r;
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
        var b = go.AddComponent<Bullet>();
        b.velocity = vel; b.isPlayerShot = playerShot; b.damage = dmg; b.radius = r;
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.8f;
        return b;
    }

    void Update()
    {
        if (homing && isPlayerShot)
        {
            var tgt = NearestTarget();
            if (tgt != null)
            {
                Vector2 want = ((Vector2)tgt.position - (Vector2)transform.position).normalized * velocity.magnitude;
                velocity = Vector2.Lerp(velocity, want, 0.14f);
            }
        }

        // Enemy projectiles obey bullet-time; player shots never slow.
        float ts = isPlayerShot ? 1f : Slowmo.EnemyScale;
        transform.position += (Vector3)(velocity * (Time.deltaTime * ts));
        age += Time.deltaTime;
        if (age > lifetime) { Destroy(gameObject); return; }

        if (Camera.main != null)
        {
            var vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
                Destroy(gameObject);
        }
    }

    Transform NearestTarget()
    {
        Transform best = null;
        float bd = float.MaxValue;
        foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = e.transform; }
        }
        var boss = FindAnyObjectByType<Boss>();
        if (boss != null)
        {
            float d = (boss.transform.position - transform.position).sqrMagnitude;
            if (d < bd) best = boss.transform;
        }
        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerShot)
        {
            var e = other.GetComponent<Enemy>();
            if (e != null) { e.TakeDamage(damage); Particles.Burst(transform.position, new Color(0.6f, 0.9f, 1f), 4); ProceduralSFX.Hit(); Destroy(gameObject); return; }
            var b = other.GetComponent<Boss>();
            if (b != null) { b.TakeDamage(damage); Particles.Burst(transform.position, new Color(0.6f, 0.9f, 1f), 4); ProceduralSFX.Hit(); Destroy(gameObject); return; }
        }
        else
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null && !ph.IsInvulnerable() && !ph.IsGuarding())
            {
                ph.TakeHit();
                Destroy(gameObject);
            }
        }
    }
}
