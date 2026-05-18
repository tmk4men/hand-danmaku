using UnityEngine;

public class DragonBeam : MonoBehaviour
{
    public float chargeRequired = 10f; // seconds of held thumbs-up
    public float firingDuration = 3f;
    public float beamHalfWidth = 0.30f;
    public Transform player;

    public float Charge { get; private set; }
    public bool Firing { get; private set; }
    private float firingUntil;
    private GameObject beamGO;

    public void UpdateGesture(bool thumbsUp, bool wasThumbsUp)
    {
        if (Firing)
        {
            if (Time.time > firingUntil) StopFire();
            else DoBeam();
            return;
        }
        if (thumbsUp) Charge = Mathf.Min(chargeRequired, Charge + Time.deltaTime);
        // Falling edge: was holding, now released
        if (!thumbsUp && wasThumbsUp && Charge >= chargeRequired) FireBeam();
    }

    void FireBeam()
    {
        Charge = 0;
        Firing = true;
        firingUntil = Time.time + firingDuration;
        if (beamGO == null) BuildBeam();
        beamGO.SetActive(true);
        CameraShake.Pulse(0.6f, 0.4f);
    }

    void StopFire()
    {
        Firing = false;
        if (beamGO) beamGO.SetActive(false);
    }

    void BuildBeam()
    {
        beamGO = new GameObject("DragonBeam");
        var sr = beamGO.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.SolidSquare(8, new Color(1f, 0.9f, 0.95f, 0.9f));
        sr.sortingOrder = 6;
        beamGO.SetActive(false);
    }

    void DoBeam()
    {
        if (player == null || beamGO == null) return;
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float top = halfH + 1f;
        float baseY = player.position.y + 0.2f;
        float midY = (baseY + top) * 0.5f;
        float height = (top - baseY);
        beamGO.transform.position = new Vector3(player.position.x, midY, 0);
        beamGO.transform.localScale = new Vector3(beamHalfWidth * 2 * 16, height * 16, 1);

        // Sweep enemies in column
        foreach (var e in FindObjectsOfType<Enemy>())
            if (Mathf.Abs(e.transform.position.x - player.position.x) < beamHalfWidth + 0.18f
                && e.transform.position.y > baseY)
                e.TakeDamage(2);
        foreach (var b in FindObjectsOfType<Bullet>())
            if (!b.isPlayerShot
                && Mathf.Abs(b.transform.position.x - player.position.x) < beamHalfWidth + 0.08f
                && b.transform.position.y > baseY)
                Destroy(b.gameObject);
        foreach (var m in FindObjectsOfType<Meteor>())
            if (Mathf.Abs(m.transform.position.x - player.position.x) < beamHalfWidth + 0.3f
                && m.transform.position.y > baseY)
                Destroy(m.gameObject);
    }
}
