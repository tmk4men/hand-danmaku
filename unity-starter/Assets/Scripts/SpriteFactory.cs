using UnityEngine;

/// <summary>
/// Generates chunky pixel-art sprites at runtime so the starter doesn't
/// need any imported textures. Use FromGrid for sprite-like shapes and
/// SolidSquare for placeholders.
/// </summary>
public static class SpriteFactory
{
    /// <summary>Pixels per world unit. Higher = sprites appear smaller on screen.</summary>
    public const float PPU = 16f;

    public static Sprite SolidSquare(int size, Color color)
    {
        var tex = NewTex(size, size);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Wrap(tex);
    }

    /// <summary>
    /// Build a sprite from a string grid; '.' is transparent, every other
    /// char looks up the palette. Rows go top-down — they're inverted
    /// internally so the visual matches the array order.
    /// </summary>
    public static Sprite FromGrid(string[] grid, System.Collections.Generic.Dictionary<char, Color> pal)
    {
        int w = grid[0].Length, h = grid.Length;
        var tex = NewTex(w, h);
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            var row = grid[y];
            for (int x = 0; x < w; x++)
            {
                char c = x < row.Length ? row[x] : '.';
                Color col = Color.clear;
                if (c != '.' && pal.TryGetValue(c, out var pc)) col = pc;
                // Flip y: top row of the grid becomes the top row of the sprite
                pixels[(h - 1 - y) * w + x] = col;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Wrap(tex);
    }

    static Texture2D NewTex(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        return t;
    }

    static Sprite Wrap(Texture2D t)
    {
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                             new Vector2(0.5f, 0.5f), PPU,
                             0, SpriteMeshType.FullRect);
    }

    // ---------- Preset sprites ----------

    public static Sprite Player()
    {
        var pal = new System.Collections.Generic.Dictionary<char, Color> {
            { '#', H("#3563d6") }, { 'B', H("#1a2a78") }, { 'C', H("#9ad8ff") },
            { 'W', Color.white },  { 'E', H("#ffd066") },
        };
        return FromGrid(new[] {
            ".....##.....",
            "....####....",
            "....#WW#....",
            "...##WW##...",
            "..##BWWB##..",
            ".###BCCB###.",
            "###BCWWCB###",
            "#B#BCWWCB#B#",
            "#.BBCCCCBB.#",
            "...##EE##...",
            "....#EE#....",
            ".....EE.....",
        }, pal);
    }

    public static Sprite Enemy(Color body)
    {
        var pal = new System.Collections.Generic.Dictionary<char, Color> {
            { '#', body }, { 'E', Color.white }, { 'W', Color.Lerp(body, Color.white, 0.6f) },
        };
        return FromGrid(new[] {
            "..####..",
            ".######.",
            "##E##E##",
            "##E##E##",
            "########",
            ".##WW##.",
            "..####..",
            "...##...",
        }, pal);
    }

    public static Sprite Bullet(Color c)
    {
        // Match the JS look: a solid square pellet with a brighter inner core
        // (inner = half size, centered).
        var pal = new System.Collections.Generic.Dictionary<char, Color> {
            { '#', c }, { 'W', Color.white },
        };
        return FromGrid(new[] {
            "########",
            "########",
            "##WWWW##",
            "##WWWW##",
            "##WWWW##",
            "##WWWW##",
            "########",
            "########",
        }, pal);
    }

    /// <summary>Hollow ring (annulus) sprite — used for guard/bomb effects.</summary>
    public static Sprite RingSprite(int outerR, int thickness, Color c)
    {
        int size = (outerR + 1) * 2;
        var tex = NewTex(size, size);
        var px = new Color[size * size];
        float cx = (size - 1) / 2f, cy = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - cx, dy = y - cy;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            px[y * size + x] = (d <= outerR && d >= outerR - thickness) ? c : Color.clear;
        }
        tex.SetPixels(px);
        tex.Apply();
        return Wrap(tex);
    }

    /// <summary>Pixel planet disc: body colour, lighter upper-left, faint rim.</summary>
    public static Sprite Disc(int r, Color body, Color hi, Color rim)
    {
        int pad = 3, size = (r + pad) * 2;
        var tex = NewTex(size, size);
        var px = new Color[size * size];
        float cx = (size - 1) / 2f, cy = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - cx, dy = y - cy;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            int i = y * size + x;
            if (d <= r)
            {
                // upper-left is lit (texture y grows up, so +dy = up)
                float lit = Mathf.Clamp01(0.45f + (-dx + dy) / (r * 2.2f));
                px[i] = Color.Lerp(body, hi, lit * 0.75f);
            }
            else if (d <= r + 2)
            {
                px[i] = new Color(rim.r, rim.g, rim.b, 0.20f * (1f - (d - r) / 2f));
            }
            else px[i] = Color.clear;
        }
        tex.SetPixels(px);
        tex.Apply();
        return Wrap(tex);
    }

    public static Sprite Meteor()
    {
        var pal = new System.Collections.Generic.Dictionary<char, Color> {
            { '#', H("#8a6a4a") }, { 'W', H("#c8a878") }, { 'D', H("#3b2a14") },
        };
        return FromGrid(new[] {
            "..######..",
            ".#W######.",
            "###W######",
            "##W#####D#",
            "#######D##",
            "##W##D####",
            "#####D#W##",
            "##D###W###",
            ".######W#.",
            "..######..",
        }, pal);
    }

    public static Color H(string hex)
    {
        if (hex[0] == '#') hex = hex.Substring(1);
        int rgb = System.Convert.ToInt32(hex, 16);
        return new Color(((rgb >> 16) & 0xff) / 255f,
                         ((rgb >>  8) & 0xff) / 255f,
                         ( rgb        & 0xff) / 255f);
    }
}
