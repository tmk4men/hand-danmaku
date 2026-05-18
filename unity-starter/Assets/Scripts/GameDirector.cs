using UnityEngine;

public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    public int Score { get; private set; }
    public int HiScore { get; private set; }
    public bool Running { get; private set; }
    public Transform Player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HiScore = PlayerPrefs.GetInt("hd_hi", 0);
    }

    public void StartGame()
    {
        Score = 0;
        Running = true;
        if (HUD.Instance) HUD.Instance.Refresh();
    }

    public void GameOver()
    {
        Running = false;
        if (Score > HiScore)
        {
            HiScore = Score;
            PlayerPrefs.SetInt("hd_hi", HiScore);
            PlayerPrefs.Save();
        }
        if (HUD.Instance) HUD.Instance.ShowGameOver();
    }

    public void AddScore(int n)
    {
        Score += n;
        if (HUD.Instance) HUD.Instance.Refresh();
    }
}
