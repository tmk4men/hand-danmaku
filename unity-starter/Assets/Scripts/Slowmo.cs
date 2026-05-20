using UnityEngine;

/// <summary>
/// Global bullet-time. Enemy bullets / meteors multiply their speed by
/// EnemyScale; the player is unaffected. Triggered by the thumb-bend gesture
/// (consumes a charge). Mirrors the JS "BULLET TIME" (slowMul 0.32).
/// </summary>
public static class Slowmo
{
    static float until;
    public static float EnemyScale => Time.time < until ? 0.32f : 1f;
    public static bool Active => Time.time < until;
    public static void Trigger(float seconds) { until = Time.time + seconds; }
    public static void Clear() { until = 0f; }
}
