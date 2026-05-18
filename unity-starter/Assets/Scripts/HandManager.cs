using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Receives MediaPipe Hands landmarks from the WebGL .jslib bridge.
/// Place this on a GameObject named exactly "HandManager" (the .jslib
/// uses SendMessage("HandManager", ...)).
/// </summary>
[DefaultExecutionOrder(-1000)]
public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    /// <summary>21 landmarks. Each is normalized 0..1 with origin at top-left.</summary>
    public Vector3[] Landmarks { get; private set; } = new Vector3[21];

    public bool HandSeen { get; private set; }
    public string LastError { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void MP_Init();
#endif

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MP_Init();
#else
        Debug.Log("[HandManager] MediaPipe bridge only runs in WebGL builds, not the editor.");
#endif
    }

    // -- Called from MediaPipeBridge.jslib via SendMessage --

    public void OnHandResult(string csv)
    {
        try
        {
            var parts = csv.Split(',');
            int n = Mathf.Min(21, parts.Length / 3);
            for (int i = 0; i < n; i++)
            {
                float x = float.Parse(parts[i * 3 + 0], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(parts[i * 3 + 1], System.Globalization.CultureInfo.InvariantCulture);
                float z = float.Parse(parts[i * 3 + 2], System.Globalization.CultureInfo.InvariantCulture);
                Landmarks[i] = new Vector3(x, y, z);
            }
            HandSeen = true;
        }
        catch (Exception e)
        {
            LastError = e.Message;
            HandSeen = false;
            Debug.LogWarning("[HandManager] parse failed: " + e.Message);
        }
    }

    public void OnHandLost(string _)
    {
        HandSeen = false;
    }

    public void OnCameraError(string msg)
    {
        LastError = msg;
        HandSeen = false;
        Debug.LogError("[HandManager] camera error: " + msg);
    }
}
