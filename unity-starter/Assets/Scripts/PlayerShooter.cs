using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public float fireInterval = 0.10f;
    public float bulletSpeed = 12f;
    public Color color = new Color(0.5f, 0.85f, 1f);
    private float nextFire;

    /// <summary>Suppress fire (e.g. while charging DRAGON).</summary>
    public bool Suppressed { get; set; }

    void Update()
    {
        if (Suppressed) return;
        if (!GameDirector.Instance || !GameDirector.Instance.Running) return;
        if (HandManager.Instance == null || !HandManager.Instance.HandSeen) return;
        if (Time.time < nextFire) return;

        Fire();
        nextFire = Time.time + fireInterval;
    }

    void Fire()
    {
        var p = transform.position;
        var v = Vector2.up * bulletSpeed;
        Bullet.Spawn(p + Vector3.up * 0.10f, v, color, true, 1);
        ProceduralSFX.Shoot();
    }
}
