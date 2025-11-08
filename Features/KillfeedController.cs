using System.Collections.Generic;
using System.Linq;
using DragonDen.Killfeed.Models;
using DragonDen.Killfeed.Utilities;
using UnityEngine;

namespace DragonDen.Killfeed.Features;

public class KillfeedController : MonoBehaviour
{
    static Texture2D _tex1x1;
    readonly List<KillEntry> kills = new();

    void OnEnable()
    {
        EventBus.OnKill += OnKill;
    }

    void OnDisable()
    {
        EventBus.OnKill -= OnKill;
    }

    void Update()
    {
        if (kills.Count == 0) return;
        var now = Time.unscaledTime;
        kills.RemoveAll(k => now - k.Evt.Time > Settings.KillfeedLineSeconds.Value);
    }

    void OnGUI()
    {
        if (!Settings.KillfeedEnabled.Value) return;
        if (Event.current.type != EventType.Repaint) return;

        var leftCorner = Settings.KillfeedCorner.Value == KillfeedAnchor.TopLeft || Settings.KillfeedCorner.Value == KillfeedAnchor.BottomLeft;
        var topCorner = Settings.KillfeedCorner.Value == KillfeedAnchor.TopLeft || Settings.KillfeedCorner.Value == KillfeedAnchor.TopRight;

        var startX = leftCorner ? Settings.KillfeedMargin.Value.x : Screen.width - Settings.KillfeedMargin.Value.x;
        var startY = topCorner ? Settings.KillfeedMargin.Value.y : Screen.height - Settings.KillfeedMargin.Value.y;

        var now = Time.unscaledTime;
        var list = kills.OrderBy(k => k.Evt.Time).ToList();

        for (var i = 0; i < list.Count; i++)
        {
            var e = list[i].Evt;
            var age = now - e.Time;
            var fade = 1f - Mathf.Clamp01(age / Settings.KillfeedLineSeconds.Value);
            var col = Settings.KillfeedColor.Value;
            col.a *= fade;

            var lineH = Settings.KillfeedLineHeight.Value;
            int font = Settings.KillfeedFontSize.Value;
            var y = topCorner ? startY + i * lineH : startY - i * lineH;

            var meters = Mathf.RoundToInt(e.DistanceMeters);
            var text = Settings.KillfeedTemplate.Value
                .Replace("{attacker}", e.AttackerName)
                .Replace("{victim}", e.VictimName)
                .Replace("{bp}", e.BodyPart ?? "")
                .Replace("{weapon}", e.WeaponLabel ?? "")
                .Replace("{ammo}", e.AmmoName ?? "")
                .Replace("{dist}", meters.ToString());

            var faction = Settings.KillfeedShowFactionIcon.Value ? TextureBank.Faction(e.AttackerSide) : null;

            var iconH = font + 6f;
            var iconW = iconH;
            var pad = 6f;

            var textW = GLDrawer.MeasureTextWidth(text, font, Settings.Font);
            var totalW = textW + (faction ? iconW + pad : 0f);
            var drawX = leftCorner ? startX : startX - totalW;

            var prev = GUI.color;
            var bgRect = new Rect(drawX - 6f, y - 2f, totalW + 12f, iconH + 4f);
            GUI.color = new Color(0f, 0f, 0f, col.a * 0.25f);
            GUI.DrawTexture(bgRect, Texture1x1());
            GUI.color = prev;

            if (faction)
            {
                var r = new Rect(drawX, y, iconW, iconH);
                var tint = Settings.KillfeedFactionTint.Value;
                if (tint.a <= 0f) tint.a = col.a;
                var before = GUI.color;
                GUI.color = new Color(tint.r, tint.g, tint.b, col.a);
                GUI.DrawTexture(r, faction, ScaleMode.ScaleToFit, true);
                GUI.color = before;
                drawX += iconW + pad;
            }

            GLDrawer.DrawText(text, new Vector2(drawX, y + iconH * 0.5f), font, col, TextAnchor.MiddleLeft, Settings.Font, false);
        }
    }

    void OnKill(DamageEvent e)
    {
        if (!e.IsLocalAttacker && !(Settings.KillfeedShowOtherKills.Value && !e.IsLocalVictim))
            return;

        kills.Add(new KillEntry { Evt = e });

        var cap = Mathf.Max(0, Settings.KillfeedMaxLines.Value);
        if (cap > 0)
            while (kills.Count > cap)
                kills.RemoveAt(0);
    }

    static Texture2D Texture1x1()
    {
        if (_tex1x1) return _tex1x1;
        _tex1x1 = new Texture2D(1, 1, TextureFormat.ARGB32, false) { filterMode = FilterMode.Point };
        _tex1x1.SetPixel(0, 0, Color.white);
        _tex1x1.Apply();
        return _tex1x1;
    }

    class KillEntry
    {
        public DamageEvent Evt;
    }
}