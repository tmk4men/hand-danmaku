using UnityEngine;

/// <summary>
/// Drop this on a single empty GameObject in an otherwise empty 2D scene.
/// Press Play (in a WebGL build) and the whole game wires itself up.
/// You do NOT need to manually create the Player, HUD, Canvas, etc.
/// </summary>
[DefaultExecutionOrder(-500)]
public class GameBootstrap : MonoBehaviour
{
    [Header("Camera")]
    public float orthographicSize = 4f;
    public Color bgColor = new Color(0.02f, 0.03f, 0.10f);

    [Header("Player")]
    public Vector3 playerStart = new Vector3(0, -2.6f, 0);

    void Awake()
    {
        SetupCamera();
        SetupHand();
        SetupGameDirector();
        var player = SetupPlayer();
        SetupSpawner();
        SetupHUD();
        SetupGestures(player);

        GameDirector.Instance.Player = player.transform;
        GameDirector.Instance.StartGame();
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.backgroundColor = bgColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.gameObject.AddComponent<CameraShake>();
    }

    void SetupHand()
    {
        if (HandManager.Instance != null) return;
        var go = new GameObject("HandManager");
        go.AddComponent<HandManager>();
    }

    void SetupGameDirector()
    {
        if (GameDirector.Instance != null) return;
        var go = new GameObject("GameDirector");
        go.AddComponent<GameDirector>();
    }

    GameObject SetupPlayer()
    {
        var go = new GameObject("Player");
        go.transform.position = playerStart;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Player();
        sr.sortingOrder = 4;
        go.AddComponent<PlayerShip>();
        go.AddComponent<PlayerShooter>();
        go.AddComponent<PlayerHealth>();
        var dragon = go.AddComponent<DragonBeam>();
        dragon.player = go.transform;
        return go;
    }

    void SetupSpawner()
    {
        var go = new GameObject("EnemySpawner");
        go.AddComponent<EnemySpawner>();
    }

    void SetupHUD()
    {
        var go = new GameObject("HUD");
        go.AddComponent<HUD>();
    }

    void SetupGestures(GameObject player)
    {
        var go = new GameObject("GestureController");
        var gc = go.AddComponent<GestureController>();
        gc.health = player.GetComponent<PlayerHealth>();
        gc.shooter = player.GetComponent<PlayerShooter>();
        gc.dragon = player.GetComponent<DragonBeam>();
    }
}
