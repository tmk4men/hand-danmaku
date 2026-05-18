using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    private Vector3 home;
    private float amp;
    private float decay;

    void Awake()
    {
        Instance = this;
        home = transform.localPosition;
    }

    public static void Pulse(float amplitude, float seconds = 0.2f)
    {
        if (Instance == null) return;
        Instance.amp = Mathf.Max(Instance.amp, amplitude);
        Instance.decay = Mathf.Max(Instance.decay, amplitude / Mathf.Max(0.05f, seconds));
    }

    void LateUpdate()
    {
        if (amp <= 0.001f) { transform.localPosition = home; return; }
        transform.localPosition = home + (Vector3)Random.insideUnitCircle * amp;
        amp = Mathf.Max(0, amp - decay * Time.deltaTime);
    }
}
