using System.IO;
using System.Reflection;
using EFT;
using UnityEngine;

namespace DragonDen.Killfeed.Utilities;

public static class TextureBank
{
    static string baseUiDir;
    static Texture2D fUsec;
    static Texture2D fBear;
    static Texture2D fScav;
    static Texture2D fUnknown;

    static void EnsureIcons()
    {
        if (baseUiDir != null) return;
        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        baseUiDir = Path.Combine(dllDir, "UI");
    }

    static Texture2D LoadPng(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false) { filterMode = FilterMode.Bilinear };
            if (!tex.LoadImage(bytes)) return null;
            return tex;
        }
        catch { return null; }
    }

    public static Texture2D Faction(EPlayerSide side)
    {
        EnsureIcons();
        switch (side)
        {
            case EPlayerSide.Usec:
                return fUsec ??= LoadPng(Path.Combine(baseUiDir, "Faction_USEC.png"));
            case EPlayerSide.Bear:
                return fBear ??= LoadPng(Path.Combine(baseUiDir, "Faction_BEAR.png"));
            case EPlayerSide.Savage:
                return fScav ??= LoadPng(Path.Combine(baseUiDir, "Faction_Scav.png"));
            default:
                return fUnknown ??= LoadPng(Path.Combine(baseUiDir, "Faction_Unknown.png"));
        }
    }
}