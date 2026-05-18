using UnityEngine;

/// <summary>
/// Wraps PlayerPrefs with a small API for coins, hi-score, owned starters,
/// loadout toggles, and language. Equivalent of the JS localStorage keys.
/// </summary>
public static class Persistence
{
    // ----- Hi-score -----
    public static int HiScore
    {
        get => PlayerPrefs.GetInt("hd_hi", 0);
        set { PlayerPrefs.SetInt("hd_hi", value); PlayerPrefs.Save(); }
    }

    // ----- Coins -----
    public static int Coins
    {
        get => PlayerPrefs.GetInt("hd_coins", 0);
        set { PlayerPrefs.SetInt("hd_coins", value); PlayerPrefs.Save(); }
    }

    /// <summary>Adds score-derived coins. Returns the earned amount.</summary>
    public static int AwardCoins(int score, int stagesCleared, int bestChain)
    {
        int earned = Mathf.FloorToInt(score / 200f)
                   + stagesCleared * 50
                   + bestChain * 5;
        Coins += earned;
        return earned;
    }

    // ----- Owned starters (each capped at 1) -----
    public static bool OwnedBomb   { get => GetFlag("hd_st_bomb"); set => SetFlag("hd_st_bomb", value); }
    public static bool OwnedLife   { get => GetFlag("hd_st_life"); set => SetFlag("hd_st_life", value); }
    public static bool OwnedPower  { get => GetFlag("hd_st_power"); set => SetFlag("hd_st_power", value); }
    public static bool OwnedCharge { get => GetFlag("hd_st_charge"); set => SetFlag("hd_st_charge", value); }

    // ----- Loadout (which owned starters to bring) -----
    public static bool LoadoutBomb   { get => GetFlag("hd_lo_bomb",   true); set => SetFlag("hd_lo_bomb", value); }
    public static bool LoadoutLife   { get => GetFlag("hd_lo_life",   true); set => SetFlag("hd_lo_life", value); }
    public static bool LoadoutPower  { get => GetFlag("hd_lo_power",  true); set => SetFlag("hd_lo_power", value); }
    public static bool LoadoutCharge { get => GetFlag("hd_lo_charge", true); set => SetFlag("hd_lo_charge", value); }

    // ----- Item prices -----
    public const int PriceBomb   = 200;
    public const int PriceLife   = 400;
    public const int PricePower  = 600;
    public const int PriceCharge = 100;

    // ----- Language -----
    public static int LangId
    {
        get => PlayerPrefs.GetInt("hd_lang", 0);
        set { PlayerPrefs.SetInt("hd_lang", value); PlayerPrefs.Save(); }
    }

    // ----- Helpers -----
    static bool GetFlag(string k, bool def = false) =>
        PlayerPrefs.GetInt(k, def ? 1 : 0) == 1;
    static void SetFlag(string k, bool v)
    {
        PlayerPrefs.SetInt(k, v ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static int ApplyBomb(int baseLives) =>
        baseLives + (LoadoutBomb && OwnedBomb ? 1 : 0);
    public static int ApplyLife(int baseLives) =>
        baseLives + (LoadoutLife && OwnedLife ? 1 : 0);
    public static int ApplyPower(int basePower) =>
        basePower + (LoadoutPower && OwnedPower ? 1 : 0);
    public static int ApplyCharge() =>
        (LoadoutCharge && OwnedCharge ? 1 : 0);
}
