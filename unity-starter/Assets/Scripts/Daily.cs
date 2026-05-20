using UnityEngine;
using System;

/// <summary>
/// Deterministic seeded RNG for Daily Challenge. Replace gameplay
/// Random.Range calls with GameRng.Range to get reproducible runs
/// across players on the same date.
/// </summary>
public static class Daily
{
    public static bool Enabled { get; private set; }
    public static string Key { get; private set; } = "";
    private static System.Random rng;

    public static void EnableToday()
    {
        Enabled = true;
        Key = DateTime.Today.ToString("yyyy-MM-dd");
        int seed = Key.GetHashCode();
        rng = new System.Random(seed);
        Debug.Log("[Daily] enabled key=" + Key + " seed=" + seed);
    }

    public static void Enable(string yyyymmdd)
    {
        Enabled = true;
        Key = yyyymmdd;
        rng = new System.Random(yyyymmdd.GetHashCode());
    }

    public static void Disable()
    {
        Enabled = false;
        Key = "";
        rng = null;
    }

    public static float NextFloat() =>
        rng != null ? (float)rng.NextDouble() : UnityEngine.Random.value;

    public static int NextInt(int maxExclusive) =>
        rng != null ? rng.Next(maxExclusive) : UnityEngine.Random.Range(0, maxExclusive);
}

public static class GameRng
{
    public static float Float01() => Daily.Enabled ? Daily.NextFloat() : UnityEngine.Random.value;
    public static float Range(float a, float b) => a + Float01() * (b - a);
    public static int Range(int a, int b) =>
        Daily.Enabled ? a + Daily.NextInt(Mathf.Max(1, b - a)) : UnityEngine.Random.Range(a, b);
}
