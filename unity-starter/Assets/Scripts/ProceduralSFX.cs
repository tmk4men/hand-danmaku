using UnityEngine;

/// <summary>
/// Synthesizes AudioClips at runtime so the project has zero audio assets.
/// Mirrors the chunky chiptune feel of the JS version's Web Audio code.
/// </summary>
public class ProceduralSFX : MonoBehaviour
{
    public static ProceduralSFX Instance { get; private set; }
    private const int RATE = 44100;
    private static int _throttleShoot;

    void Awake()
    {
        Instance = this;
        // Pre-warm: AudioSource on this GameObject so unity initializes the audio system.
        var pre = gameObject.AddComponent<AudioSource>();
        pre.playOnAwake = false;
    }

    // --- Public sound presets ---

    public static void Shoot()
    {
        // Throttle to avoid an audio storm on rapid fire
        if (Instance == null || (++_throttleShoot % 3) != 0) return;
        Instance.PlayTone(880, 0.04f, 0.05f, Wave.Square, -300);
    }
    public static void Hit()      { Instance?.PlayNoise(0.03f, 0.05f); }
    public static void Explode()  { Instance?.PlayNoise(0.18f, 0.15f);
                                    Instance?.PlayTone(240, 0.18f, 0.07f, Wave.Saw, -120); }
    public static void BossDie()  { Instance?.PlayNoise(0.8f, 0.28f);
                                    Instance?.PlayTone(130, 0.6f, 0.14f, Wave.Saw, -60); }
    public static void Bomb()     { Instance?.PlayNoise(0.32f, 0.25f);
                                    Instance?.PlayTone(70, 0.45f, 0.14f, Wave.Square, -30); }
    public static void DragonFire(){ Instance?.PlayNoise(0.4f, 0.3f);
                                     Instance?.PlayTone(140, 0.8f, 0.18f, Wave.Square, -60); }
    public static void Pickup()   { Instance?.PlayTone(1000, 0.08f, 0.13f, Wave.Triangle, 600); }
    public static void PickupLife(){
        Instance?.PlayTone(880, 0.08f, 0.16f, Wave.Triangle);
        Instance?.PlayTone(1320, 0.18f, 0.16f, Wave.Triangle, 0, 0.09f);
    }
    public static void PlayerHit(){ Instance?.PlayNoise(0.28f, 0.20f);
                                    Instance?.PlayTone(220, 0.3f, 0.14f, Wave.Saw, -180); }
    public static void StageStart(){
        Instance?.PlayTone(660, 0.10f, 0.14f, Wave.Triangle);
        Instance?.PlayTone(880, 0.12f, 0.14f, Wave.Triangle, 0, 0.10f);
        Instance?.PlayTone(1100, 0.18f, 0.16f, Wave.Triangle, 0, 0.22f);
    }
    public static void Warning()  { Instance?.PlayTone(880, 0.08f, 0.12f, Wave.Square); }

    // --- Synthesis ---

    public enum Wave { Square, Saw, Triangle, Sine }

    void PlayTone(float freq, float dur, float vol, Wave w, float slideHz = 0, float delay = 0)
    {
        var clip = BuildTone(freq, dur, vol, w, slideHz);
        PlayClip(clip, delay);
    }

    void PlayNoise(float dur, float vol, float delay = 0)
    {
        var clip = BuildNoise(dur, vol);
        PlayClip(clip, delay);
    }

    void PlayClip(AudioClip clip, float delay)
    {
        var go = new GameObject("sfx_oneshot");
        var src = go.AddComponent<AudioSource>();
        src.clip = clip; src.spatialBlend = 0; src.volume = 1f;
        src.PlayDelayed(delay);
        Destroy(go, clip.length + delay + 0.05f);
    }

    AudioClip BuildTone(float freq, float dur, float vol, Wave w, float slideHz)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(RATE * dur));
        var samples = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / RATE;
            float f = Mathf.Max(20, freq + slideHz * (t / dur));
            phase += f / RATE;
            float ph = (float)(phase - System.Math.Floor(phase));
            float s = 0;
            switch (w)
            {
                case Wave.Square:   s = ph < 0.5f ? 1f : -1f; break;
                case Wave.Saw:      s = ph * 2f - 1f; break;
                case Wave.Triangle: s = 1f - 4f * Mathf.Abs(ph - 0.5f); break;
                case Wave.Sine:     s = Mathf.Sin(ph * Mathf.PI * 2f); break;
            }
            float env = Mathf.Exp(-3.5f * (t / dur));
            samples[i] = s * vol * env;
        }
        var clip = AudioClip.Create("tone", n, 1, RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip BuildNoise(float dur, float vol)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(RATE * dur));
        var samples = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / RATE;
            float env = Mathf.Exp(-4f * (t / dur));
            samples[i] = (Random.value * 2f - 1f) * vol * env;
        }
        var clip = AudioClip.Create("noise", n, 1, RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
