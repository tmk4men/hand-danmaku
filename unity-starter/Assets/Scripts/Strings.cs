using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Minimal JA/EN string table. Call Strings.T("key") in UI code.
/// Persist language via Persistence.LangId.
/// </summary>
public static class Strings
{
    public enum Lang { JA = 0, EN = 1 }
    public static Lang Current { get; private set; }

    static readonly Dictionary<string, string> JA = new() {
        { "title",       "HAND DANMAKU" },
        { "subtitle",    "手で操作する弾幕シューティング" },
        { "score",       "SCORE" },
        { "hi",          "HI" },
        { "lives",       "LIVES" },
        { "coins",       "COINS" },
        { "stage",       "STAGE" },
        { "chain",       "CHAIN" },
        { "gameOver",    "GAME OVER" },
        { "retry",       "↻ もう一度遊ぶ" },
        { "start",       "▶ スタート" },
        { "titleHint",   "人差し指で機体を操作 ／ ✊ガード 🤏ボム ／ カメラに手をかざしてね" },
        { "showHand",    "手をかざして開始" },
        { "warning",     "!! WARNING !!" },
        { "bossApproaching", "ボス出現" },
        { "wave",        "WAVE" },
        { "bombFx",      "弾消去 x{0}" },
        { "dashCut",     "ダッシュ x{0}" },
        { "earned",      "+{0} COINS" },
        { "noHit",       "★ ノーミス" },
        { "noBomb",      "★ ノーボム" },
        { "rank",        "RANK" },
        { "dragonReady", "RELEASE TO FIRE" },
        { "dragonLocked","FIND DRAGON CRYSTAL" },
        { "dragonUnlocked","DRAGON UNLOCKED" },
        { "powerUp",     "POWER UP!" },
        { "lifeUp",      "1 UP!" },
        { "bombUp",      "BOMB +1" },
        { "guardUp",     "GUARD +3s" },
        { "toolUp",      "TOOLBOX" },
    };
    static readonly Dictionary<string, string> EN = new() {
        { "title",       "HAND DANMAKU" },
        { "subtitle",    "Hand-tracking bullet hell" },
        { "score",       "SCORE" },
        { "hi",          "HI" },
        { "lives",       "LIVES" },
        { "coins",       "COINS" },
        { "stage",       "STAGE" },
        { "chain",       "CHAIN" },
        { "gameOver",    "GAME OVER" },
        { "retry",       "↻ PLAY AGAIN" },
        { "start",       "▶ START" },
        { "titleHint",   "Index finger moves your ship ／ ✊ guard 🤏 bomb ／ show your hand to the camera" },
        { "showHand",    "Show hand to start" },
        { "warning",     "!! WARNING !!" },
        { "bossApproaching", "BOSS APPROACHING" },
        { "wave",        "WAVE" },
        { "bombFx",      "BULLET ERASE x{0}" },
        { "dashCut",     "DASH CUT x{0}" },
        { "earned",      "+{0} COINS" },
        { "noHit",       "★ NO HIT" },
        { "noBomb",      "★ NO BOMB" },
        { "rank",        "RANK" },
        { "dragonReady", "RELEASE TO FIRE" },
        { "dragonLocked","FIND DRAGON CRYSTAL" },
        { "dragonUnlocked","DRAGON UNLOCKED" },
        { "powerUp",     "POWER UP!" },
        { "lifeUp",      "1 UP!" },
        { "bombUp",      "BOMB +1" },
        { "guardUp",     "GUARD +3s" },
        { "toolUp",      "TOOLBOX" },
    };

    public static void Load()
    {
        // The WebGL build only ships the built-in Latin font (Arial), which has
        // no CJK glyphs in-browser — Japanese would render as blank boxes. Force
        // EN until a Japanese font asset is added, then restore the line below.
        //   Current = (Lang)Mathf.Clamp(Persistence.LangId, 0, 1);
        Current = Lang.EN;
    }
    public static void SetLang(Lang l)
    {
        Current = l;
        Persistence.LangId = (int)l;
    }
    public static string T(string key)
    {
        var d = Current == Lang.JA ? JA : EN;
        return d.TryGetValue(key, out var s) ? s : key;
    }
    public static string T(string key, params object[] args)
    {
        return string.Format(T(key), args);
    }
}
