using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Meteor : MonoBehaviour
{
    public float speed = 2.0f;
    public float radius = 0.32f;
    public float rotSpeed = 50f;

    public static Meteor Spawn()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = Random.Range(-halfW + 0.5f, halfW - 0.5f);
        var go = new GameObject("Meteor");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Meteor();
        sr.sortingOrder = 4;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.32f; col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
        var m = go.AddComponent<Meteor>();
        go.transform.position = new Vector3(x, halfH + 0.5f, 0);
        go.transform.localScale = Vector3.one * 1.4f;
        CameraShake.Pulse(0.1f, 0.4f);
        return m;
    }

    void Update()
    {
        transform.position += Vector3.down * (speed * Time.deltaTime);
        transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
        if (Camera.main && Camera.main.WorldToViewportPoint(transform.position).y < -0.15f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            if (ph.IsGuarding()) { Destroy(gameObject); CameraShake.Pulse(0.6f, 0.3f); return; }
            if (!ph.IsInvulnerable()) { ph.InstantKill(); Destroy(gameObject); }
        }
    }
}
