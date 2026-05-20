using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Builds a runtime Canvas + Text fields. No prefab needed.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }
    private Text scoreTxt, livesTxt, hiTxt, statusTxt;
    private GameObject overlayPanel, titlePanel;

    void Awake()
    {
        Instance = this;
        BuildUI();
        Refresh();
    }

    void BuildUI()
    {
        // UI buttons need an EventSystem to receive clicks/taps.
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvasGO = new GameObject("HUD_Canvas");
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(960, 720);
        canvasGO.AddComponent<GraphicRaycaster>();

        scoreTxt  = MakeText(canvas.transform, "Score",  new Vector2(20, -20), TextAnchor.UpperLeft, 22);
        hiTxt     = MakeText(canvas.transform, "Hi",     new Vector2(20, -50), TextAnchor.UpperLeft, 14);
        livesTxt  = MakeText(canvas.transform, "Lives",  new Vector2(20, -80), TextAnchor.UpperLeft, 18);
        statusTxt = MakeText(canvas.transform, "Status", new Vector2(0, 0),    TextAnchor.MiddleCenter, 36);
        statusTxt.color = new Color(1f, 0.82f, 0.4f);
        statusTxt.text = "";

        // Game-over overlay (hidden by default)
        overlayPanel = MakePanel(canvas.transform, "GameOver", new Color(0, 0, 0, 0.78f));
        var t = MakeText(overlayPanel.transform, "GameOver", new Vector2(0, 60), TextAnchor.MiddleCenter, 48);
        t.text = "GAME OVER";
        t.color = Color.white;
        MakeButton(overlayPanel.transform, "RETRY", new Vector2(0, -50),
                   () => GameDirector.Instance.Restart());
        overlayPanel.SetActive(false);

        // Title / home screen (shown on first load). ASCII only so it renders in
        // WebGL (the built-in font has no CJK/emoji glyphs in-browser).
        titlePanel = MakePanel(canvas.transform, "TitlePanel", new Color(0.02f, 0.03f, 0.10f, 0.97f));
        var ttl = MakeText(titlePanel.transform, "Title", new Vector2(0, 120), TextAnchor.MiddleCenter, 60);
        ttl.text = "HAND DANMAKU"; ttl.color = new Color(0.6f, 0.9f, 1f);
        var sub = MakeText(titlePanel.transform, "Sub", new Vector2(0, 55), TextAnchor.MiddleCenter, 20);
        sub.text = "Point your index finger to move.\nFist = GUARD   Pinch = BOMB";
        sub.color = new Color(0.85f, 0.9f, 1f);
        MakeButton(titlePanel.transform, "START", new Vector2(0, -40),
                   () => GameDirector.Instance.StartGame());
        titlePanel.SetActive(false);
    }

    GameObject MakePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
        return go;
    }

    Button MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280, 70);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.55f, 0.9f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var lt = lblGO.AddComponent<Text>();
        lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lt.fontSize = 28; lt.color = Color.white; lt.alignment = TextAnchor.MiddleCenter;
        lt.text = label;
        return btn;
    }

    Text MakeText(Transform parent, string name, Vector2 pos, TextAnchor anchor, int size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = (anchor == TextAnchor.UpperLeft) ? new Vector2(0, 1) : new Vector2(0.5f, 0.5f);
        rt.anchorMax = rt.anchorMin;
        rt.pivot = rt.anchorMin;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, 100);
        var tx = go.AddComponent<Text>();
        tx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tx.fontSize = size;
        tx.color = Color.white;
        tx.alignment = anchor;
        tx.text = "";
        return tx;
    }

    public void Refresh()
    {
        if (GameDirector.Instance == null) return;
        scoreTxt.text  = $"{Strings.T("score")}  {GameDirector.Instance.Score:D7}";
        hiTxt.text     = $"{Strings.T("hi")}     {GameDirector.Instance.HiScore:D7}";
        var ph = GameDirector.Instance.Player ? GameDirector.Instance.Player.GetComponent<PlayerHealth>() : null;
        int lives = ph ? ph.lives : 0;
        livesTxt.text = $"{Strings.T("lives")}  " + new string('♥', Mathf.Max(0, lives));
    }

    public void ShowStatus(string s) { if (statusTxt) statusTxt.text = s; }
    public void ShowGameOver() { if (overlayPanel) overlayPanel.SetActive(true); }
    public void HideGameOver() { if (overlayPanel) overlayPanel.SetActive(false); }
    public void ShowTitle() { if (titlePanel) titlePanel.SetActive(true); }
    public void HideTitle() { if (titlePanel) titlePanel.SetActive(false); }
}
