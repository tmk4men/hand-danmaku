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

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HiScore = Persistence.HiScore;
    }

    public void StartGame()
    {
        Score = 0;
        Running = true;
        if (HUD.Instance)
        {
            HUD.Instance.HideTitle();
            HUD.Instance.HideGameOver();
            HUD.Instance.Refresh();
        }
        ProceduralSFX.StageStart();
        StageBanner.Show(Strings.T("title"), new Color(0.55f, 0.88f, 1f), 2f);
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
        int earned = Persistence.AwardCoins(Score, 0, best);
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
