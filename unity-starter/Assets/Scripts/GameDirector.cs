using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    /// <summary>Survives the scene reload used by Restart() so RETRY skips the
    /// title screen and drops straight back into play.</summary>
    public static bool ResumeImmediately;

    public int Score { get; private set; }
    public int HiScore { get; private set; }
    public bool Running { get; private set; }
    public Transform Player;

    public int Bombs { get; private set; }
    public int Charges { get; private set; }
    public int Stage { get; private set; } = 1;

    public void AddBomb()   { Bombs = Mathf.Min(6, Bombs + 1); if (HUD.Instance) HUD.Instance.Refresh(); }
    public bool UseBomb()   { if (Bombs <= 0) return false; Bombs--; if (HUD.Instance) HUD.Instance.Refresh(); return true; }
    public void AddCharge() { Charges = Mathf.Min(3, Charges + 1); if (HUD.Instance) HUD.Instance.Refresh(); }
    public bool UseCharge() { if (Charges <= 0) return false; Charges--; if (HUD.Instance) HUD.Instance.Refresh(); return true; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HiScore = Persistence.HiScore;
    }

    public void StartGame()
    {
        Score = 0;
        Stage = 1;
        Running = true;
        Slowmo.Clear();

        // Per-run resources (with loadout bonuses), matching the JS start().
        Bombs = 4 + (Persistence.LoadoutBomb && Persistence.OwnedBomb ? 1 : 0);
        Charges = Persistence.ApplyCharge();
        var sh = Player ? Player.GetComponent<PlayerShooter>() : null;
        if (sh) sh.SetPower(1 + (Persistence.LoadoutPower && Persistence.OwnedPower ? 1 : 0));

        if (HUD.Instance)
        {
            HUD.Instance.HideTitle();
            HUD.Instance.HideGameOver();
            HUD.Instance.Refresh();
        }
        ProceduralSFX.StageStart();
        StageBanner.Show(Strings.T("title"), new Color(0.55f, 0.88f, 1f), 2f);
    }

    /// <summary>Called on boss death: next stage, +1 bomb, new background theme.</summary>
    public void AdvanceStage()
    {
        Stage++;
        AddBomb();
        var bg = FindAnyObjectByType<Background>();
        if (bg) bg.SetTheme((Stage - 1) % 5);
        StageBanner.Show("STAGE " + Stage, new Color(0.55f, 0.88f, 1f), 2f);
        ProceduralSFX.StageStart();
        if (HUD.Instance) HUD.Instance.Refresh();
    }

    /// <summary>Reload the scene and resume play immediately (RETRY button).</summary>
    public void Restart()
    {
        ResumeImmediately = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        Running = false;
        if (Score > HiScore)
        {
            HiScore = Score;
            Persistence.HiScore = HiScore;
        }
        int best = ComboMeter.Instance ? ComboMeter.Instance.Best : 0;
        int earned = Persistence.AwardCoins(Score, Mathf.Max(0, Stage - 1), best);
        if (earned > 0) FloatingText.Spawn(Player ? Player.position : Vector3.zero,
                                            Strings.T("earned", earned),
                                            new Color(1f, 0.82f, 0.4f), 24);
        if (HUD.Instance) HUD.Instance.ShowGameOver();
    }

    public void AddScore(int n)
    {
        Score += n;
        if (HUD.Instance) HUD.Instance.Refresh();
    }
}
