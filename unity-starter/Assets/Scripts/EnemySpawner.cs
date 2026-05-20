using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float interval = 2.0f;
    public float meteorEvery = 18f;
    public int bossEveryWaves = 14;
    private float nextSpawn, nextMeteor;
    private int waveCount;
    private bool bossActive;

    void Update()
    {
        if (!GameDirector.Instance || !GameDirector.Instance.Running) return;
        if (bossActive) return; // pause grunt spawns during boss

        if (Time.time >= nextSpawn) { SpawnWave(); nextSpawn = Time.time + interval; }
        if (Time.time >= nextMeteor) { Meteor.Spawn(); nextMeteor = Time.time + meteorEvery; }
    }

    void LateUpdate()
    {
        // Detect boss death to resume normal spawning
        if (bossActive && FindAnyObjectByType<Boss>() == null)
        {
            bossActive = false;
            nextSpawn = Time.time + 1.5f;
        }
    }

    void SpawnWave()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float yTop = halfH + 0.3f;
        waveCount++;

        // Boss every N waves
        if (waveCount > 0 && waveCount % bossEveryWaves == 0)
        {
            Boss.Spawn();
            bossActive = true;
            ProceduralSFX.StageStart();
            StageBanner.Show(Strings.T("bossApproaching"), new Color(1f, 0.45f, 0.6f), 2.4f);
            return;
        }

        var pattern = (waveCount % 5 == 0) ? Enemy.Pattern.Spin4
                   : (waveCount % 3 == 0) ? Enemy.Pattern.Spread3
                                          : Enemy.Pattern.Aimed;
        // Exact grunt body colours from the JS version (spin / fan / aim).
        var color = (waveCount % 5 == 0) ? SpriteFactory.H("#3aa6c8")
                  : (waveCount % 3 == 0) ? SpriteFactory.H("#c89a3a")
                                         : SpriteFactory.H("#c845a8");

        // 1-3 enemies per wave depending on count
        int n = 1 + Mathf.Min(waveCount / 5, 2);
        for (int i = 0; i < n; i++)
        {
            float x = GameRng.Range(-halfW + 0.5f, halfW - 0.5f);
            Enemy.Spawn(new Vector3(x, yTop, 0), color, pattern);
        }
    }
}
