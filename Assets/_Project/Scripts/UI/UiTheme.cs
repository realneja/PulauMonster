// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Satu tempat untuk warna tema navy-emas.
public static class UiTheme
{
    public static readonly Color Ink = new Color32(0x12, 0x10, 0x27, 255);
    public static readonly Color Panel = new Color32(0x1C, 0x15, 0x40, 255);
    public static readonly Color Gold = new Color32(0xFF, 0xC6, 0x1A, 255);
    public static readonly Color Text = new Color32(0xED, 0xE9, 0xF7, 255);
    public static readonly Color Muted = new Color32(0xA9, 0xA3, 0xC9, 255);
    public static readonly Color Disabled = new Color32(0x5C, 0x56, 0x80, 255);
    public static readonly Color Exp = new Color32(0x6F, 0x9B, 0xFF, 255);
    public static readonly Color BarBack = new Color32(0x0B, 0x09, 0x18, 255);

    public static Color ElementColor(Element element)
    {
        switch (element)
        {
            case Element.Pyro: return new Color32(255, 122, 61, 255);
            case Element.Aqua: return new Color32(86, 170, 255, 255);
            case Element.Sprout: return new Color32(124, 212, 97, 255);
            default: return new Color32(255, 230, 80, 255); // Volt
        }
    }

    public const int PixelScale = 4;              // 1 piksel demo = 4 unit kanvas (layar 320x180 → 1280x720)
    public const float SlicedMultiplier = 1.5625f; // 6,25 ÷ 1,5625 = 4 unit per piksel panel
}
