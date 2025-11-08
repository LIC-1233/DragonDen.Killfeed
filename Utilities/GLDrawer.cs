using UnityEngine;

namespace DragonDen.Killfeed.Utilities;

public static class GLDrawer
{
    static GUIStyle _style;
    static Texture2D _tex;

    static GUIStyle Style(int size, TextAnchor anchor, Font font)
    {
        _style ??= new GUIStyle(GUI.skin.label);
        _style.fontSize = size;
        _style.alignment = anchor;
        _style.font = font;
        _style.richText = false;
        return _style;
    }

    static Texture2D Tex()
    {
        if (_tex) return _tex;
        _tex = new Texture2D(1, 1, TextureFormat.ARGB32, false) { filterMode = FilterMode.Bilinear };
        _tex.SetPixel(0, 0, Color.white);
        _tex.Apply();
        return _tex;
    }

    public static void DrawText(string text, Vector2 pos, int size, Color color, TextAnchor anchor, Font font, bool backdrop)
    {
        var s = Style(size, anchor, font);
        var content = new GUIContent(text);
        var dim = s.CalcSize(content);
        var rect = new Rect(pos.x, pos.y, dim.x + 8f, dim.y + 2f);

        if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleRight)
            rect.y -= rect.height * 0.5f;
        if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.LowerCenter)
            rect.x -= rect.width * 0.5f;
        if (anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight || anchor == TextAnchor.LowerRight)
            rect.x -= rect.width;

        if (backdrop)
        {
            var bgCol = new Color(0, 0, 0, color.a * 0.4f);
            var bgRect = new Rect(rect.x - 2, rect.y - 1, rect.width + 4, rect.height + 2);
            var prev = GUI.color;
            GUI.color = bgCol;
            GUI.DrawTexture(bgRect, Tex());
            GUI.color = prev;
        }

        GUI.color = color;
        GUI.Label(rect, text, s);
    }

    public static float MeasureTextWidth(string text, int size, Font font)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        var st = new GUIStyle(GUI.skin.label) { fontSize = size, font = font };
        return st.CalcSize(new GUIContent(text)).x;
    }
}