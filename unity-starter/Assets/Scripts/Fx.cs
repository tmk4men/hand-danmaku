using UnityEngine;

/// <summary>
/// Throwaway runtime visual effects (no prefabs). Each spawns a GameObject that
/// animates and destroys itself.
/// </summary>
public static class Fx
{
    public static void Bomb(Vector3 pos)
    {
        ScreenFlash(new Color(0.72f, 0.86f, 1f, 0.55f), 0.28f);
        ExpandRing(pos, new Color(0.72f, 0.86f, 1f, 0.9f), 0.4f, 7.5f, 0.45f);
    }

    public static void ScreenFlash(Color c, float dur)
    {
        var cam = Camera.main;
        if (cam == null) return;
        var go = new GameObject("FxFlash");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.SolidSquare(1, Color.white);
        sr.color = c;
        sr.sortingOrder = 900;
        float hH = cam.orthographicSize, hW = hH * cam.aspect;
        go.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        go.transform.localScale = new Vector3(hW * 2f + 1f, hH * 2f + 1f, 1f) * SpriteFactory.PPU;
        go.AddComponent<FxFade>().Init(sr, dur);
    }

    public static void ExpandRing(Vector3 pos, Color c, float fromR, float toR, float dur)
    {
        var go = new GameObject("FxRing");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.RingSprite(24, 4, Color.white);
        sr.color = c;
        sr.sortingOrder = 60;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.AddComponent<FxExpand>().Init(sr, fromR, toR, dur);
    }
}

/// <summary>Fade alpha to zero over dur, then destroy.</summary>
public class FxFade : MonoBehaviour
{
    SpriteRenderer sr; float dur, t, a0;
    public void Init(SpriteRenderer s, float d) { sr = s; dur = d; a0 = s.color.a; }
    void Update()
    {
        t += Time.deltaTime;
        var c = sr.color; c.a = a0 * (1f - Mathf.Clamp01(t / dur)); sr.color = c;
        if (t >= dur) Destroy(gameObject);
    }
}

/// <summary>Expand a ring sprite from fromR to toR (world radius) while fading.</summary>
public class FxExpand : MonoBehaviour
{
    SpriteRenderer sr; float fromR, toR, dur, t, a0;
    // RingSprite(24,...) has a 24px outer radius -> this world radius at scale 1.
    static float BaseR => 24f / SpriteFactory.PPU;
    public void Init(SpriteRenderer s, float fr, float tr, float d) { sr = s; fromR = fr; toR = tr; dur = d; a0 = s.color.a; }
    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / dur);
        float r = Mathf.Lerp(fromR, toR, k);
        transform.localScale = Vector3.one * (r / BaseR);
        var c = sr.color; c.a = a0 * (1f - k); sr.color = c;
        if (t >= dur) Destroy(gameObject);
    }
}
