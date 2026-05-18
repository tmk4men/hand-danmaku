using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float interval = 2.0f;
    public float meteorEvery = 18f;
    private float nextSpawn, nextMeteor;
    private int waveCount;

    void Update()
    {
        if (!GameDirector.Instance || !GameDirector.Instance.Running) return;
        if (Time.time >= nextSpawn) { SpawnWave(); nextSpawn = Time.time + interval; }
        if (Time.time >= nextMeteor) { Meteor.Spawn(); nextMeteor = Time.time + meteorEvery; }
    }

    void SpawnWave()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float yTop = halfH + 0.3f;
        waveCount++;

        var pattern = (waveCount % 5 == 0) ? Enemy.Pattern.Spin4
                   : (waveCount % 3 == 0) ? Enemy.Pattern.Spread3
                                          : Enemy.Pattern.Aimed;
        var color = (waveCount % 5 == 0) ? new Color(0.6f, 0.95f, 1f)
                  : (waveCount % 3 == 0) ? new Color(1f, 0.82f, 0.45f)
                                         : new Color(0.85f, 0.4f, 0.7f);

        // 1-3 enemies per wave depending on count
        int n = 1 + Mathf.Min(waveCount / 5, 2);
        for (int i = 0; i < n; i++)
        {
            float x = Random.Range(-halfW + 0.5f, halfW - 0.5f);
            Enemy.Spawn(new Vector3(x, yTop, 0), color, pattern);
        }
    }
}
