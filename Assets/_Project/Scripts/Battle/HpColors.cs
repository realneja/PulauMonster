// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Warna bar HP: hijau > 50%, emas > 20%, merah selebihnya.
public static class HpColors
{
    public static readonly Color High = new Color32(98, 214, 112, 255);
    public static readonly Color Mid = new Color32(255, 198, 26, 255);
    public static readonly Color Low = new Color32(232, 72, 72, 255);

    public static Color For(float fill) => fill > 0.5f ? High : fill > 0.2f ? Mid : Low;
}
