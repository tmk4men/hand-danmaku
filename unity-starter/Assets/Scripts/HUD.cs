using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds a runtime Canvas + Text fields. No prefab needed.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }
    private Text scoreTxt, livesTxt, hiTxt, statusTxt;
    private GameObject overlayPanel;

    void Awake()
    {
        Instance = this;
        BuildUI();
        Refresh();
    }

    void BuildUI()
    {
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
        overlayPanel = new GameObject("GameOver");
        overlayPanel.transform.SetParent(canvas.transform, false);
        var oRT = overlayPanel.AddComponent<RectTransform>();
        oRT.anchorMin = Vector2.zero; oRT.anchorMax = Vector2.one;
        oRT.offsetMin = Vector2.zero; oRT.offsetMax = Vector2.zero;
        var bg = overlayPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.75f);
        var t = MakeText(overlayPanel.transform, "GameOver", new Vector2(0, 0), TextAnchor.MiddleCenter, 48);
        t.text = "GAME OVER\nshow hand to retry";
        t.color = Color.white;
        overlayPanel.SetActive(false);
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
}
