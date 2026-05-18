using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public int hp = 4;
    public int scoreReward = 100;
    public float fallSpeed = 1.4f;
    public float fireInterval = 1.5f;
    public Color bulletColor = new Color(1f, 0.6f, 0.9f);
    public float bulletSpeed = 3.5f;
    public enum Pattern { Aimed, Spread3, Spin4 }
    public Pattern pattern = Pattern.Aimed;
    private float nextFire;

    public static Enemy Spawn(Vector3 pos, Color body, Pattern pattern)
    {
        var go = new GameObject("Enemy");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Enemy(body);
        sr.sortingOrder = 3;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.18f;
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
        var e = go.AddComponent<Enemy>();
        e.pattern = pattern;
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.9f;
        e.nextFire = Time.time + 1.0f;
        return e;
    }

    void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (Time.time >= nextFire) { Fire(); nextFire = Time.time + fireInterval; }

        // Despawn after leaving the screen
        if (Camera.main && Camera.main.WorldToViewportPoint(transform.position).y < -0.1f)
            Destroy(gameObject);
    }

    void Fire()
    {
        var player = GameDirector.Instance != null ? GameDirector.Instance.Player : null;
        if (player == null) return;
        var origin = (Vector2)transform.position + Vector2.down * 0.1f;

        if (pattern == Pattern.Aimed)
        {
            var v = ((Vector2)player.position - origin).normalized * bulletSpeed;
            Bullet.Spawn(origin, v, bulletColor, false);
        }
        else if (pattern == Pattern.Spread3)
        {
            var aim = ((Vector2)player.position - origin).normalized;
            for (int i = -1; i <= 1; i++)
            {
                float ang = Mathf.Atan2(aim.y, aim.x) + i * 0.22f;
                var v = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * bulletSpeed;
                Bullet.Spawn(origin, v, bulletColor, false);
            }
        }
        else if (pattern == Pattern.Spin4)
        {
            float baseAng = Time.time * 1.4f;
            for (int k = 0; k < 4; k++)
            {
                float ang = baseAng + k * Mathf.PI * 0.5f;
                var v = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (bulletSpeed * 0.8f);
                Bullet.Spawn(origin, v, bulletColor, false);
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    public void Die()
    {
        if (GameDirector.Instance) GameDirector.Instance.AddScore(scoreReward);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) { ph.TakeHit(); }
    }
}
