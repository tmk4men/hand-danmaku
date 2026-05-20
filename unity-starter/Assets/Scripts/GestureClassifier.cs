using UnityEngine;

/// <summary>
/// Stateless gesture detection from MediaPipe Hands landmarks.
/// Mirrors the logic used in the JS version of HAND DANMAKU.
/// All inputs use the 21-landmark array from HandManager.
/// </summary>
public static class GestureClassifier
{
    // Landmark indices we care about
    public const int WRIST   = 0;
    public const int THUMB_T = 4;   // thumb tip
    public const int IDX_T   = 8;   // index tip
    public const int IDX_PIP = 6;
    public const int MID_T   = 12;
    public const int MID_PIP = 10;
    public const int RNG_T   = 16;
    public const int RNG_PIP = 14;
    public const int PNK_T   = 20;
    public const int PNK_PIP = 18;
    public const int IDX_MCP = 5;
    public const int PNK_MCP = 17;
    public const int MID_MCP = 9;

    /// <summary>Pinch threshold as a fraction of hand span (scale-invariant).
    /// Tune this one number if pinch is too eager / too strict.</summary>
    public static float PinchRatio = 0.50f;

    public static bool IsPinch(Vector3[] lm)
    {
        float span = Mathf.Max(0.0001f, HandSpan(lm));
        return Dist2D(lm[IDX_T], lm[THUMB_T]) < span * PinchRatio;
    }

    /// <summary>index-thumb gap / hand span — exposed for the debug overlay.</summary>
    public static float PinchRatioNow(Vector3[] lm)
    {
        float span = Mathf.Max(0.0001f, HandSpan(lm));
        return Dist2D(lm[IDX_T], lm[THUMB_T]) / span;
    }

    /// <summary>Count of curled non-thumb fingers (0..4).</summary>
    public static int CurledCount(Vector3[] lm)
    {
        var w = lm[WRIST];
        int c = 0;
        if (Dist2D(lm[IDX_T], w) < Dist2D(lm[IDX_PIP], w) * 1.05f) c++;
        if (Dist2D(lm[MID_T], w) < Dist2D(lm[MID_PIP], w) * 1.05f) c++;
        if (Dist2D(lm[RNG_T], w) < Dist2D(lm[RNG_PIP], w) * 1.05f) c++;
        if (Dist2D(lm[PNK_T], w) < Dist2D(lm[PNK_PIP], w) * 1.05f) c++;
        return c;
    }

    public static bool[] CurlBits(Vector3[] lm)
    {
        var w = lm[WRIST];
        return new bool[] {
            Dist2D(lm[IDX_T], w) < Dist2D(lm[IDX_PIP], w) * 1.05f,
            Dist2D(lm[MID_T], w) < Dist2D(lm[MID_PIP], w) * 1.05f,
            Dist2D(lm[RNG_T], w) < Dist2D(lm[RNG_PIP], w) * 1.05f,
            Dist2D(lm[PNK_T], w) < Dist2D(lm[PNK_PIP], w) * 1.05f,
        };
    }

    public static float HandSpan(Vector3[] lm)
    {
        return Dist2D(lm[IDX_MCP], lm[PNK_MCP]);
    }

    public static Vector2 PalmCenter(Vector3[] lm)
    {
        return new Vector2((lm[WRIST].x + lm[MID_MCP].x) * 0.5f,
                           (lm[WRIST].y + lm[MID_MCP].y) * 0.5f);
    }

    public static bool ThumbExtended(Vector3[] lm)
    {
        var p = PalmCenter(lm);
        var t = (Vector2)lm[THUMB_T];
        return Vector2.Distance(t, p) > HandSpan(lm) * 0.85f;
    }

    public static bool ThumbInward(Vector3[] lm)
    {
        var p = PalmCenter(lm);
        var t = (Vector2)lm[THUMB_T];
        return Vector2.Distance(t, p) < HandSpan(lm) * 0.78f;
    }

    // Pinch must be checked first; the helpers below assume isPinch is false.
    public static bool IsPeace(Vector3[] lm)
    {
        var c = CurlBits(lm);
        return !IsPinch(lm) && !c[0] && !c[1] && c[2] && c[3];
    }

    public static bool IsThumbsUp(Vector3[] lm)
    {
        return ThumbExtended(lm) && CurledCount(lm) == 4 && !IsPinch(lm) && !IsPeace(lm);
    }

    public static bool IsFist(Vector3[] lm)
    {
        return !IsPinch(lm) && !IsThumbsUp(lm) && CurledCount(lm) >= 3;
    }

    public static bool IsThumbBent(Vector3[] lm)
    {
        return ThumbInward(lm) && CurledCount(lm) == 0
            && !IsPinch(lm) && !IsFist(lm) && !IsPeace(lm);
    }

    // ---- helpers ----
    private static float Dist2D(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
