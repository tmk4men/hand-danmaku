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
    public float lifetime = 6f;
    public float radius = 0.06f;

    private float age;

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
        transform.position += (Vector3)(velocity * Time.deltaTime);
        age += Time.deltaTime;
        if (age > lifetime) { Destroy(gameObject); return; }

        if (Camera.main != null)
        {
            var vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerShot)
        {
            var e = other.GetComponent<Enemy>();
            if (e != null) { e.TakeDamage(damage); Destroy(gameObject); }
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
