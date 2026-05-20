using UnityEngine;

public enum ItemType { Power, Bomb, Guard, Life, Tool, Dragon }

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Item : MonoBehaviour
{
    public ItemType type;
    public float fallSpeed = 1.2f;
    public float magnetRange = 1.4f;

    private float life;

    public static Item Spawn(Vector3 pos, ItemType type)
    {
        var go = new GameObject("Item_" + type);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteForItem(type);
        sr.sortingOrder = 5;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.18f; col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic;
        var item = go.AddComponent<Item>();
        item.type = type;
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.9f;
        return item;
    }

    void Update()
    {
        life += Time.deltaTime;
        // bob + fall
        float bob = Mathf.Sin(life * 6f) * 0.05f;
        transform.position += new Vector3(0, -fallSpeed * Time.deltaTime + bob * Time.deltaTime * 0.5f, 0);

        // Magnet toward player when close
        var p = GameDirector.Instance != null ? GameDirector.Instance.Player : null;
        if (p != null)
        {
            var dist = Vector2.Distance(p.position, transform.position);
            if (dist < magnetRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, p.position, 5f * Time.deltaTime);
            }
        }

        // Despawn off screen
        if (Camera.main && Camera.main.WorldToViewportPoint(transform.position).y < -0.15f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) { Apply(ph); Destroy(gameObject); }
    }

    void Apply(PlayerHealth ph)
    {
        var dir = GameDirector.Instance;
        switch (type)
        {
            case ItemType.Life:
                ph.AddLife(); dir?.AddScore(1000); ProceduralSFX.PickupLife();
                Pop(Strings.T("lifeUp"), Color.white);
                break;
            case ItemType.Dragon:
                ph.GetComponent<DragonBeam>()?.UnlockCharge(); dir?.AddScore(300); ProceduralSFX.PickupLife();
                Pop(Strings.T("dragonUnlocked"), new Color(1f, 0.47f, 0.78f));
                break;
            case ItemType.Power:
                ph.GetComponent<PlayerShooter>()?.AddPower(); dir?.AddScore(150); ProceduralSFX.Pickup();
                Pop(Strings.T("powerUp"), SpriteFactory.H("#ffd066"));
                break;
            case ItemType.Bomb:
                dir?.AddBomb(); dir?.AddScore(150); ProceduralSFX.Pickup();
                Pop(Strings.T("bombUp"), SpriteFactory.H("#ff7c98"));
                break;
            case ItemType.Guard:
                ph.AddGuard(3f); dir?.AddScore(150); ProceduralSFX.Pickup();
                Pop(Strings.T("guardUp"), SpriteFactory.H("#7fffd4"));
                break;
            case ItemType.Tool:
                dir?.AddCharge(); dir?.AddScore(150); ProceduralSFX.Pickup();
                Pop(Strings.T("toolUp"), SpriteFactory.H("#c8a878"));
                break;
        }
        if (HUD.Instance) HUD.Instance.Refresh();
        CameraShake.Pulse(0.15f, 0.1f);
    }

    void Pop(string s, Color c) => FloatingText.Spawn(transform.position, s, c, 18);

    static Sprite SpriteForItem(ItemType type)
    {
        Color c;
        char glyph = ' ';
        switch (type)
        {
            case ItemType.Power:  c = SpriteFactory.H("#ffd066"); glyph = 'P'; break;
            case ItemType.Bomb:   c = SpriteFactory.H("#ff7c98"); glyph = 'B'; break;
            case ItemType.Guard:  c = SpriteFactory.H("#7fffd4"); glyph = 'G'; break;
            case ItemType.Life:   c = Color.white;                glyph = '1'; break;
            case ItemType.Tool:   c = SpriteFactory.H("#c8a878"); glyph = 'T'; break;
            case ItemType.Dragon: c = SpriteFactory.H("#ff77c8"); glyph = 'D'; break;
            default: c = Color.white; break;
        }
        // Simple bordered square — replace with grids later if you want JS-version parity.
        return SpriteFactory.SolidSquare(10, c);
    }
}
