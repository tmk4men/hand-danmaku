using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public float fireInterval = 0.10f;
    public float bulletSpeed = 12f;
    public Color color = new Color(0.5f, 0.85f, 1f);
    private float nextFire;

    /// <summary>Suppress fire (e.g. while charging DRAGON).</summary>
    public bool Suppressed { get; set; }
    /// <summary>FOCUS mode (peace sign): precise homing bolts, slower cadence.</summary>
    public bool Focus { get; set; }
    /// <summary>Weapon power 1..3 — adds firing lanes.</summary>
    public int Power { get; private set; } = 1;

    private float boostUntil;
    public bool Boosting => Time.time < boostUntil;

    public void SetPower(int p) { Power = Mathf.Clamp(p, 1, 3); }
    public void AddPower() { Power = Mathf.Min(3, Power + 1); }
    public void Boost(float seconds) { boostUntil = Time.time + seconds; }   // graze MAX POWER

    void Update()
    {
        if (Suppressed) return;
        if (!GameDirector.Instance || !GameDirector.Instance.Running) return;
        if (HandManager.Instance == null || !HandManager.Instance.HandSeen) return;

        float interval = Boosting ? 0.067f : (Focus ? 0.18f : fireInterval);
        if (Time.time < nextFire) return;
        Fire();
        nextFire = Time.time + interval;
    }

    void Fire()
    {
        Vector3 p = transform.position + Vector3.up * 0.1f;

        if (Focus)
        {
            int lanes = Power;
            for (int i = 0; i < lanes; i++)
            {
                float off = (i - (lanes - 1) / 2f) * 0.12f;
                var b = Bullet.Spawn(p + new Vector3(off, 0, 0), Vector2.up * (bulletSpeed * 0.9f),
                                     new Color(0.53f, 0.88f, 1f), true, 1);
                b.homing = true;
                b.SetSprite(SpriteFactory.PlayerHoming(new Color(0.53f, 0.88f, 1f)));
            }
        }
        else if (Boosting)
        {
            for (int i = -1; i <= 1; i++)
            {
                var v = new Vector2(i * 2.2f, bulletSpeed);
                var b = Bullet.Spawn(p, v, color, true, 1);
                b.SetSprite(SpriteFactory.PlayerShot(color));
            }
        }
        else
        {
            int lanes = Power;   // 1, 2, or 3 straight shots
            for (int i = 0; i < lanes; i++)
            {
                float off = (i - (lanes - 1) / 2f) * 0.16f;
                var b = Bullet.Spawn(p + new Vector3(off, 0, 0), Vector2.up * bulletSpeed, color, true, 1);
                b.SetSprite(SpriteFactory.PlayerShot(color));
            }
        }
        ProceduralSFX.Shoot();
    }
}
