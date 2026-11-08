// Pulau Monster · P3 Animasi, Kamera & Interaksi (diperluas P5, P6) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using UnityEngine;

// Area dunia (Desa, Hutan, Danau, Gua): nama untuk HUD dan musik area saat Player masuk.
[RequireComponent(typeof(Collider2D))]
public class AreaZone : MonoBehaviour
{
    [SerializeField] private string areaName = "Desa Awal";
    [SerializeField] private AudioClip music;

    private AudioService audio;
    private PartnerHud hud;

    public string AreaName => areaName;
    public event Action<string> Entered;

    public void Construct(AudioService audioService, PartnerHud partnerHud)
    {
        audio = audioService;
        hud = partnerHud;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (audio != null && music != null) audio.PlayWorldMusic(music);
        if (hud != null) hud.SetArea(areaName);
        Entered?.Invoke(areaName);
    }
}
