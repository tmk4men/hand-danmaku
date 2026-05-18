using UnityEngine;

/// <summary>
/// Chain counter: each enemy kill within ~2s of the last bumps the chain.
/// Multiplier scales score, taper at 19. Singleton — call Bump() on kill.
/// </summary>
public class ComboMeter : MonoBehaviour
{
    public static ComboMeter Instance { get; private set; }

    public int Count { get; private set; }
    public int Best  { get; private set; }
    public float Timer { get; private set; }
    public float Window = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bump()
    {
        Count++;
        Timer = Window;
        if (Count > Best) Best = Count;
    }

    public void Reset()
    {
        Count = 0; Timer = 0;
    }

    public float Multiplier => 1f + Mathf.Min(Count, 19) * 0.1f;

    void Update()
    {
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0) Count = 0;
        }
    }
}
