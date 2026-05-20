using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Themed parallax backdrop ported from the JS version's drawBackground():
/// 3 stacked sky bands, drifting nebula squares, a slow planet disc, three
/// star layers, and a faint horizon line. No texture imports.
/// </summary>
public class Background : MonoBehaviour
{
    struct Theme
    {
        public Color[] sky;          // top -> bottom (darkest -> lightest)
        public Color planet, planetHi, accent, neb;
    }

    // STAGE_THEMES from index.html
    static readonly Theme[] THEMES = {
        new Theme { sky = new[]{H("#06081a"),H("#0b0e2e"),H("#181b46")}, planet=H("#3b2078"), planetHi=H("#5a3aa8"), accent=H("#88a8ff"), neb=H("#243072") },
        new Theme { sky = new[]{H("#16070d"),H("#280c1a"),H("#421430")}, planet=H("#7a1f3a"), planetHi=H("#b03857"), accent=H("#ff77c8"), neb=H("#5a1638") },
        new Theme { sky = new[]{H("#03140e"),H("#06281e"),H("#0d4636")}, planet=H("#0d5a3a"), planetHi=H("#28a070"), accent=H("#7fffd4"), neb=H("#0d4632") },
        new Theme { sky = new[]{H("#1a1006"),H("#2e1f0c"),H("#46341a")}, planet=H("#7a4a14"), planetHi=H("#b87a2a"), accent=H("#ffd066"), neb=H("#5a3814") },
        new Theme { sky = new[]{H("#05102a"),H("#0a2055"),H("#1244a0")}, planet=H("#1244a8"), planetHi=H("#2a78d0"), accent=H("#88e0ff"), neb=H("#1a3a78") },
    };

    public int themeIndex = 0;

    struct Layer { public Transform[] stars; public float speed; }
    Layer[] layers;
    Transform[] nebula;
    float[] nebSpeed;
    Transform planet;
    float halfW, halfH;

    static Color H(string hex) => SpriteFactory.H(hex);

    void Awake()
    {
        var cam = Camera.main;
        if (cam == null) return;
        halfH = cam.orthographicSize;
        halfW = halfH * cam.aspect;

        var th = THEMES[Mathf.Clamp(themeIndex, 0, THEMES.Length - 1)];
        cam.backgroundColor = th.sky[0];

        BuildSky(th);
        BuildNebula(th);
        BuildPlanet(th);
        BuildStars();
        BuildHorizon(th);
    }

    Transform MakeQuad(string name, Color c, float w, float h, Vector2 pos, int sorting)
    {
        var go = new GameObject(name);
        go.transform.parent = transform;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.SolidSquare(1, c);   // colour + alpha baked in
        sr.sortingOrder = sorting;
        go.transform.position = new Vector3(pos.x, pos.y, 3f);
        go.transform.localScale = new Vector3(w, h, 1f) * SpriteFactory.PPU;
        return go.transform;
    }

    void BuildSky(Theme th)
    {
        float bandH = (halfH * 2f) / 3f;
        for (int i = 0; i < 3; i++)
        {
            float y = halfH - (i + 0.5f) * bandH;     // band 0 at top
            MakeQuad("sky", th.sky[i], halfW * 2f + 0.5f, bandH + 0.05f, new Vector2(0, y), -100);
        }
    }

    void BuildNebula(Theme th)
    {
        int n = 6;
        nebula = new Transform[n];
        nebSpeed = new float[n];
        var c = new Color(th.neb.r, th.neb.g, th.neb.b, 0.16f);
        for (int i = 0; i < n; i++)
        {
            float s = Random.Range(1.6f, 3.6f);
            nebula[i] = MakeQuad("nebula", c, s, s,
                new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH)), -90);
            nebSpeed[i] = Random.Range(0.05f, 0.18f);
        }
    }

    void BuildPlanet(Theme th)
    {
        var go = new GameObject("planet");
        go.transform.parent = transform;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Disc(16, th.planet, th.planetHi, th.accent);
        sr.sortingOrder = -80;
        go.transform.position = new Vector3(halfW * 0.5f, halfH * 0.55f, 3f);
        planet = go.transform;
    }

    void BuildStars()
    {
        layers = new Layer[] {
            MakeStarLayer(80, 0.35f, H("#6a72a8"), 0.030f, -70),  // far
            MakeStarLayer(45, 0.85f, H("#cad0ff"), 0.045f, -65),  // mid
            MakeStarLayer(20, 1.70f, Color.white,  0.065f, -60),  // near
        };
    }

    Layer MakeStarLayer(int n, float speed, Color color, float worldSize, int sorting)
    {
        var arr = new Transform[n];
        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("star");
            go.transform.parent = transform;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SolidSquare(1, color);
            sr.sortingOrder = sorting;
            go.transform.position = new Vector3(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH), 2.5f);
            go.transform.localScale = Vector3.one * worldSize * SpriteFactory.PPU;
            arr[i] = go.transform;
        }
        return new Layer { stars = arr, speed = speed };
    }

    void BuildHorizon(Theme th)
    {
        var c = new Color(th.accent.r, th.accent.g, th.accent.b, 0.10f);
        MakeQuad("horizon", c, halfW * 2f, 0.04f, new Vector2(0, -0.32f * halfH), -55);
    }

    void Update()
    {
        if (layers != null)
            foreach (var l in layers)
                foreach (var s in l.stars)
                {
                    var p = s.position;
                    p.y -= l.speed * Time.deltaTime;
                    if (p.y < -halfH - 0.2f) { p.y = halfH + 0.2f; p.x = Random.Range(-halfW, halfW); }
                    s.position = p;
                }

        if (nebula != null)
            for (int i = 0; i < nebula.Length; i++)
            {
                var p = nebula[i].position;
                p.y -= nebSpeed[i] * Time.deltaTime;
                if (p.y < -halfH - 2f) { p.y = halfH + 2f; p.x = Random.Range(-halfW, halfW); }
                nebula[i].position = p;
            }

        if (planet)
        {
            var p = planet.position;
            p.x -= 0.04f * Time.deltaTime;
            p.y += Mathf.Sin(Time.time * 0.2f) * 0.0008f;
            if (p.x < -halfW - 2f) p.x = halfW + 2f;
            planet.position = p;
        }
    }
}
