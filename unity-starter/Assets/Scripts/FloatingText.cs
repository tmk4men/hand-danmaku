using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns short-lived score popups attached to the HUD Canvas at the
/// screen position of a world point. Drifts up + fades out.
/// </summary>
public class FloatingText : MonoBehaviour
{
    private Text txt;
    private float maxLife = 1.0f;
    private float life;
    private Vector2 baseScreen;

    public static void Spawn(Vector3 worldPos, string text, Color color, int size = 22)
    {
        var hud = HUD.Instance;
        if (hud == null) return;
        var canvas = hud.GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("FloatingText");
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 60);

        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.fontStyle = FontStyle.Bold;

        var screen = Camera.main.WorldToScreenPoint(worldPos);
        rt.position = screen;

        var ft = go.AddComponent<FloatingText>();
        ft.txt = t;
        ft.maxLife = 1.0f;
        ft.life = ft.maxLife;
        ft.baseScreen = screen;
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }
        baseScreen.y += 60f * Time.deltaTime;
        var rt = (RectTransform)transform;
        rt.position = baseScreen;

        var a = Mathf.Clamp01(life / maxLife);
        var c = txt.color; c.a = a; txt.color = c;
    }
}
