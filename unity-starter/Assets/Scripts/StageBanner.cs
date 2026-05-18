using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Centered large banner. Used for STAGE/WAVE/BOSS APPROACHING messages.
/// Auto-creates its own Text child on the HUD canvas.
/// </summary>
public class StageBanner : MonoBehaviour
{
    public static StageBanner Instance { get; private set; }
    private Text label;
    private float life;
    private float maxLife;

    void Awake()
    {
        Instance = this;
        BuildUI();
    }

    void BuildUI()
    {
        var hud = HUD.Instance;
        if (hud == null) return;
        var canvas = hud.GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("StageBanner");
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(800, 100);

        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 48;
        t.color = new Color(1f, 0.82f, 0.4f);
        t.text = "";
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        label = t;
        label.gameObject.SetActive(false);
    }

    public static void Show(string text, Color color, float duration = 2f)
    {
        if (Instance == null || Instance.label == null) return;
        Instance.label.text = text;
        Instance.label.color = color;
        Instance.maxLife = duration;
        Instance.life = duration;
        Instance.label.gameObject.SetActive(true);
    }

    void Update()
    {
        if (life <= 0f) return;
        life -= Time.deltaTime;
        if (life <= 0f) { label.gameObject.SetActive(false); return; }
        float t = Mathf.Clamp01(life / maxLife);
        var c = label.color; c.a = t; label.color = c;
    }
}
