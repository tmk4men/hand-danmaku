using UnityEngine;

/// <summary>
/// Themed parallax backdrop ported from the JS version's drawBackground():
/// 3 stacked sky bands, drifting nebula squares, a slow planet disc, three
/// star layers, and a faint horizon line. SetTheme() recolours everything so
/// the background cycles as the player clears stages.
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
    SpriteRenderer[] skySr;
    SpriteRenderer[] nebSr;
    SpriteRenderer planetSr, horizonSr;
    Transform planet;
    float[] nebSpeed;
    float halfW, halfH;

    static Color H(string hex) => SpriteFactory.H(hex);

    void Awake()
    {
        var cam = Camera.main;
        if (cam == null) return;
        halfH = cam.orthographicSize;
        halfW = halfH * cam.aspect;

        BuildSky();
        BuildNebula();
        BuildPlanet();
        BuildStars();
        BuildHorizon();
        SetTheme(themeIndex);
    }

    SpriteRenderer MakeQuad(string name, float w, float h, Vector2 pos, int sorting)
    {
        var go = new GameObject(name);
        go.transform.parent = transform;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.SolidSquare(1, Color.white);
        sr.sortingOrder = sorting;
        go.transform.position = new Vector3(pos.x, pos.y, 3f);
        go.transform.localScale = new Vector3(w, h, 1f) * SpriteFactory.PPU;
        return sr;
    }

    void BuildSky()
    {
        skySr = new SpriteRenderer[3];
        float bandH = (halfH * 2f) / 3f;
        for (int i = 0; i < 3; i++)
        {
            float y = halfH - (i + 0.5f) * bandH;
            skySr[i] = MakeQuad("sky", halfW * 2f + 0.5f, bandH + 0.05f, new Vector2(0, y), -100);
        }
    }

    void BuildNebula()
    {
        int n = 6;
        nebSr = new SpriteRenderer[n];
        nebSpeed = new float[n];
        for (int i = 0; i < n; i++)
        {
            float s = Random.Range(1.6f, 3.6f);
            nebSr[i] = MakeQuad("nebula", s, s,
                new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH)), -90);
            nebSpeed[i] = Random.Range(0.05f, 0.18f);
        }
    }

    void BuildPlanet()
    {
        var go = new GameObject("planet");
        go.transform.parent = transform;
        planetSr = go.AddComponent<SpriteRenderer>();
        planetSr.sortingOrder = -80;
        go.transform.position = new Vector3(halfW * 0.5f, halfH * 0.55f, 3f);
        planet = go.transform;
    }

    void BuildStars()
    {
        layers = new Layer[] {
            MakeStarLayer(80, 0.35f, H("#6a72a8"), 0.030f, -70),
            MakeStarLayer(45, 0.85f, H("#cad0ff"), 0.045f, -65),
            MakeStarLayer(20, 1.70f, Color.white,  0.065f, -60),
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

    void BuildHorizon()
    {
        horizonSr = MakeQuad("horizon", halfW * 2f, 0.04f, new Vector2(0, -0.32f * halfH), -55);
    }

    /// <summary>Recolour the whole backdrop to a stage theme (0-4).</summary>
    public void SetTheme(int idx)
    {
        themeIndex = Mathf.Clamp(idx, 0, THEMES.Length - 1);
        var th = THEMES[themeIndex];
        var cam = Camera.main;
        if (cam) cam.backgroundColor = th.sky[0];

        if (skySr != null)
            for (int i = 0; i < skySr.Length && i < th.sky.Length; i++)
                if (skySr[i]) skySr[i].sprite = SpriteFactory.SolidSquare(1, th.sky[i]);

        if (nebSr != null)
        {
            var nc = new Color(th.neb.r, th.neb.g, th.neb.b, 0.16f);
            foreach (var s in nebSr) if (s) s.sprite = SpriteFactory.SolidSquare(1, nc);
        }
        if (planetSr) planetSr.sprite = SpriteFactory.Disc(16, th.planet, th.planetHi, th.accent);
        if (horizonSr) horizonSr.sprite = SpriteFactory.SolidSquare(1,
            new Color(th.accent.r, th.accent.g, th.accent.b, 0.10f));
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

        if (nebSr != null)
            for (int i = 0; i < nebSr.Length; i++)
            {
                if (!nebSr[i]) continue;
                var p = nebSr[i].transform.position;
                p.y -= nebSpeed[i] * Time.deltaTime;
                if (p.y < -halfH - 2f) { p.y = halfH + 2f; p.x = Random.Range(-halfW, halfW); }
                nebSr[i].transform.position = p;
            }

        if (planet)
        {
            var p = planet.position;
            p.x -= 0.04f * Time.deltaTime;
            if (p.x < -halfW - 2f) p.x = halfW + 2f;
            planet.position = p;
        }
    }
}
