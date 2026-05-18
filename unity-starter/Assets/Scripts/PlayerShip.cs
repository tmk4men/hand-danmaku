using UnityEngine;

/// <summary>
/// Moves the GameObject toward the fingertip detected by HandManager.
/// Drop on a sprite (e.g. a Square sprite) and Press Play in a WebGL build.
/// </summary>
public class PlayerShip : MonoBehaviour
{
    [Tooltip("Index fingertip landmark (8) by default.")]
    public int tipIndex = 8;

    [Range(0.05f, 1f)]
    [Tooltip("0..1 lerp factor each frame. Higher = snappier.")]
    public float smoothing = 0.25f;

    [Tooltip("Active normalized region on the camera (e.g. 0.20-0.80).")]
    public float regionMin = 0.20f;
    public float regionMax = 0.80f;

    [Tooltip("Edge padding in world units so the ship doesn't clip viewport.")]
    public float worldPadding = 0.3f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null) Debug.LogError("[PlayerShip] Main Camera not found.");
    }

    void Update()
    {
        if (cam == null) return;
        if (HandManager.Instance == null || !HandManager.Instance.HandSeen) return;

        Vector3 lm = HandManager.Instance.Landmarks[tipIndex];

        float range = Mathf.Max(0.001f, regionMax - regionMin);
        float nx = Mathf.Clamp01((lm.x - regionMin) / range);
        float ny = Mathf.Clamp01((lm.y - regionMin) / range);

        // Map (0..1) to world space. y is flipped because image y grows downward.
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 target = new Vector3(
            Mathf.Lerp(-halfW + worldPadding,  halfW - worldPadding, nx),
            Mathf.Lerp( halfH - worldPadding, -halfH + worldPadding, ny),
            transform.position.z
        );
        transform.position = Vector3.Lerp(transform.position, target, smoothing);
    }
}
