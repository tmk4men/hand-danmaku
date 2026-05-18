using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural parallax starfield. Three layers fall at different speeds
/// with the slowest dimmer. No sprite imports needed.
/// </summary>
public class Background : MonoBehaviour
{
    struct Layer { public Transform[] stars; public float speed; }
    private Layer[] layers;
    private float halfW, halfH;

    void Awake()
    {
        var cam = Camera.main;
        if (cam == null) return;
        halfH = cam.orthographicSize;
        halfW = halfH * cam.aspect;

        layers = new Layer[] {
            MakeLayer(70, 0.4f,  new Color(0.45f, 0.50f, 0.75f, 0.55f), 0.04f),
            MakeLayer(35, 1.0f,  new Color(0.85f, 0.90f, 1.00f, 0.85f), 0.06f),
            MakeLayer(15, 2.2f,  new Color(1.00f, 1.00f, 1.00f, 1.0f),  0.10f),
        };
    }

    Layer MakeLayer(int n, float speed, Color color, float worldSize)
    {
        var arr = new Transform[n];
        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("star");
            go.transform.parent = transform;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SolidSquare(2, color);
            sr.sortingOrder = -50;
            go.transform.position = new Vector3(
                Random.Range(-halfW, halfW),
                Random.Range(-halfH, halfH),
                2f);
            go.transform.localScale = Vector3.one * worldSize * SpriteFactory.PPU;
            arr[i] = go.transform;
        }
        return new Layer { stars = arr, speed = speed };
    }

    void Update()
    {
        if (layers == null) return;
        foreach (var l in layers)
        {
            foreach (var s in l.stars)
            {
                var p = s.position;
                p.y -= l.speed * Time.deltaTime;
                if (p.y < -halfH - 0.5f) p.y = halfH + 0.3f;
                s.position = p;
            }
        }
    }
}
