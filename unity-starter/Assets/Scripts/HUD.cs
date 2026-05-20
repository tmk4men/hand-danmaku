using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Builds a runtime Canvas + Text fields. No prefab needed.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }
    private Text scoreTxt, livesTxt, hiTxt, statusTxt, infoTxt, comboTxt;
    private GameObject overlayPanel, titlePanel, shopPanel;
    private Text shopCoins;
    private readonly Text[] shopLabel = new Text[4];
    private readonly Button[] shopBuy = new Button[4];
    private readonly Button[] shopUse = new Button[4];
    static readonly string[] ShopNames  = { "BOMB +1", "LIFE +1", "POWER +1", "USE CHARGE" };
    static readonly int[]    ShopPrices = { 200, 400, 600, 100 };

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
        infoTxt   = MakeText(canvas.transform, "Info",   new Vector2(20, -108), TextAnchor.UpperLeft, 16);
        comboTxt  = MakeText(canvas.transform, "Combo",  new Vector2(20, -134), TextAnchor.UpperLeft, 18);
        comboTxt.color = new Color(1f, 0.82f, 0.4f);
        statusTxt = MakeText(canvas.transform, "Status", new Vector2(0, 0),    TextAnchor.MiddleCenter, 36);
        statusTxt.color = new Color(1f, 0.82f, 0.4f);
        statusTxt.text = "";

        // Game-over overlay (hidden by default)
        overlayPanel = MakePanel(canvas.transform, "GameOver", new Color(0, 0, 0, 0.78f));
        var t = MakeText(overlayPanel.transform, "GameOver", new Vector2(0, 60), TextAnchor.MiddleCenter, 48);
        t.text = "GAME OVER";
        t.color = Color.white;
        MakeButton(overlayPanel.transform, "RETRY", new Vector2(0, -40),
                   () => GameDirector.Instance.Restart());
        MakeButton(overlayPanel.transform, "TITLE / SHOP", new Vector2(0, -118),
                   () => GameDirector.Instance.ToTitle(), new Vector2(260, 56), 22);
        overlayPanel.SetActive(false);

        // Title / home screen (shown on first load). ASCII only so it renders in
        // WebGL (the built-in font has no CJK/emoji glyphs in-browser).
        titlePanel = MakePanel(canvas.transform, "TitlePanel", new Color(0.02f, 0.03f, 0.10f, 0.97f));
        var ttl = MakeText(titlePanel.transform, "Title", new Vector2(0, 120), TextAnchor.MiddleCenter, 60);
        ttl.text = "HAND DANMAKU"; ttl.color = new Color(0.6f, 0.9f, 1f);
        var sub = MakeText(titlePanel.transform, "Sub", new Vector2(0, 55), TextAnchor.MiddleCenter, 20);
        sub.text = "Point your index finger to move.\nFist = GUARD   Pinch = BOMB";
        sub.color = new Color(0.85f, 0.9f, 1f);
        MakeButton(titlePanel.transform, "START", new Vector2(0, -20),
                   () => GameDirector.Instance.StartGame());
        MakeButton(titlePanel.transform, "SHOP", new Vector2(0, -105),
                   () => ShowShop(), new Vector2(220, 60), 24);
        titlePanel.SetActive(false);

        BuildShop(canvas.transform);
    }

    void BuildShop(Transform parent)
    {
        shopPanel = MakePanel(parent, "ShopPanel", new Color(0.02f, 0.03f, 0.10f, 0.98f));
        var tt = MakeText(shopPanel.transform, "ShopTitle", new Vector2(0, 150), TextAnchor.MiddleCenter, 40);
        tt.text = "SHOP"; tt.color = new Color(1f, 0.85f, 0.4f);
        shopCoins = MakeText(shopPanel.transform, "ShopCoins", new Vector2(0, 108), TextAnchor.MiddleCenter, 22);

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            float y = 50 - i * 56;
            shopLabel[idx] = MakeText(shopPanel.transform, "row", new Vector2(-150, y), TextAnchor.MiddleCenter, 18);
            shopBuy[idx] = MakeButton(shopPanel.transform, "BUY", new Vector2(90, y),
                              () => TryBuy(idx), new Vector2(120, 44), 18);
            shopUse[idx] = MakeButton(shopPanel.transform, "USE", new Vector2(215, y),
                              () => ToggleLoadout(idx), new Vector2(120, 44), 16);
        }
        MakeButton(shopPanel.transform, "CLOSE", new Vector2(0, -150),
                   () => HideShop(), new Vector2(220, 56), 22);
        shopPanel.SetActive(false);
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
        => MakeButton(parent, label, pos, onClick, new Vector2(280, 70), 28);

    Button MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick,
                      Vector2 size, int fontSize)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
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
        lt.fontSize = fontSize; lt.color = Color.white; lt.alignment = TextAnchor.MiddleCenter;
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
        var dir = GameDirector.Instance;
        var ph = dir.Player ? dir.Player.GetComponent<PlayerHealth>() : null;
        var sh = dir.Player ? dir.Player.GetComponent<PlayerShooter>() : null;
        int lives = ph ? ph.lives : 0;
        livesTxt.text = $"{Strings.T("lives")}  " + new string('*', Mathf.Max(0, lives));
        infoTxt.text  = $"BOMB {dir.Bombs}   POWER {(sh ? sh.Power : 1)}   USE {dir.Charges}";
    }

    void Update()
    {
        if (comboTxt == null) return;
        int chain = ComboMeter.Instance ? ComboMeter.Instance.Count : 0;
        comboTxt.text = chain > 1 ? $"CHAIN x{chain}" : "";
    }

    public void ShowStatus(string s) { if (statusTxt) statusTxt.text = s; }
    public void ShowGameOver() { if (overlayPanel) overlayPanel.SetActive(true); }
    public void HideGameOver() { if (overlayPanel) overlayPanel.SetActive(false); }
    public void ShowTitle() { if (titlePanel) titlePanel.SetActive(true); }
    public void HideTitle() { if (titlePanel) titlePanel.SetActive(false); }

    // ---------- Shop ----------
    public void ShowShop()
    {
        if (titlePanel) titlePanel.SetActive(false);
        if (shopPanel) { shopPanel.SetActive(true); RefreshShop(); }
    }
    public void HideShop()
    {
        if (shopPanel) shopPanel.SetActive(false);
        if (titlePanel) titlePanel.SetActive(true);
    }

    void RefreshShop()
    {
        shopCoins.text = $"COINS: {Persistence.Coins}";
        for (int i = 0; i < 4; i++)
        {
            bool owned = Owned(i);
            shopLabel[i].text = $"{ShopNames[i]}   {ShopPrices[i]}c" + (owned ? "  [OWNED]" : "");
            bool canBuy = !owned && Persistence.Coins >= ShopPrices[i];
            shopBuy[i].interactable = canBuy;
            shopBuy[i].GetComponentInChildren<Text>().text = owned ? "OWNED" : "BUY";
            shopUse[i].interactable = owned;
            shopUse[i].GetComponentInChildren<Text>().text = owned ? (Loadout(i) ? "USE: ON" : "USE: OFF") : "--";
        }
    }

    void TryBuy(int i)
    {
        if (Owned(i) || Persistence.Coins < ShopPrices[i]) return;
        Persistence.Coins -= ShopPrices[i];
        SetOwned(i);
        ProceduralSFX.Pickup();
        RefreshShop();
    }

    void ToggleLoadout(int i)
    {
        if (!Owned(i)) return;
        SetLoadout(i, !Loadout(i));
        RefreshShop();
    }

    static bool Owned(int i)
    {
        switch (i) { case 0: return Persistence.OwnedBomb; case 1: return Persistence.OwnedLife;
                     case 2: return Persistence.OwnedPower; default: return Persistence.OwnedCharge; }
    }
    static void SetOwned(int i)
    {
        switch (i) { case 0: Persistence.OwnedBomb = true; break; case 1: Persistence.OwnedLife = true; break;
                     case 2: Persistence.OwnedPower = true; break; default: Persistence.OwnedCharge = true; break; }
    }
    static bool Loadout(int i)
    {
        switch (i) { case 0: return Persistence.LoadoutBomb; case 1: return Persistence.LoadoutLife;
                     case 2: return Persistence.LoadoutPower; default: return Persistence.LoadoutCharge; }
    }
    static void SetLoadout(int i, bool v)
    {
        switch (i) { case 0: Persistence.LoadoutBomb = v; break; case 1: Persistence.LoadoutLife = v; break;
                     case 2: Persistence.LoadoutPower = v; break; default: Persistence.LoadoutCharge = v; break; }
    }
}
