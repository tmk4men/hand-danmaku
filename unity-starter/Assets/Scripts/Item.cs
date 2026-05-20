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
        if (type == ItemType.Dragon) transform.Rotate(0, 0, 40f * Time.deltaTime);

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
        Particles.Burst(transform.position, ItemColor(type), 10);
        CameraShake.Pulse(0.15f, 0.1f);
    }

    void Pop(string s, Color c) => FloatingText.Spawn(transform.position, s, c, 18);

    public static Color ItemColor(ItemType type)
    {
        switch (type)
        {
            case ItemType.Power:  return SpriteFactory.H("#ffd066");
            case ItemType.Bomb:   return SpriteFactory.H("#ff7c98");
            case ItemType.Guard:  return SpriteFactory.H("#7fffd4");
            case ItemType.Tool:   return SpriteFactory.H("#c8a878");
            case ItemType.Dragon: return SpriteFactory.H("#ff77c8");
            default:              return Color.white;
        }
    }

    static Sprite SpriteForItem(ItemType type) => SpriteFactory.Item(type);
}
