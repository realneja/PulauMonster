// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Layar judul: latar peta bergeser, 4 starter beranimasi, menu Lanjutkan / Mulai baru (2 slot) / Keluar.
public class TitleInstaller : SceneInstaller
{
    [SerializeField] private TextMenu menu;
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private RectTransform background;
    [SerializeField] private float scrollRange = 240f; // sin(t / 6) x 240 unit kanvas
    [SerializeField] private Image[] starterImages;
    [SerializeField] private MonsterData[] starterData;

    private readonly List<string> options = new List<string>();
    private GameServices services;

    public override void Install(GameServices s)
    {
        services = s;
        options.Clear();
        for (int slot = 0; slot < 2; slot++)
            if (s.Save.HasSave(slot)) options.Add($"Lanjutkan Slot {slot + 1}");
        options.Add("Mulai baru Slot 1");
        options.Add("Mulai baru Slot 2");
        options.Add("Keluar");
        menu.Sfx = s.Audio;
        menu.Picked += OnPicked;
        menu.Open(options.ToArray());
        if (s.Audio != null) s.Audio.PlayWorldMusic(titleMusic);
    }

    void Update()
    {
        if (background != null)
            background.anchoredPosition = new Vector2(Mathf.Sin(Time.unscaledTime / 6f) * scrollRange, background.anchoredPosition.y);
        if (starterImages == null || starterData == null) return;
        int frame = Mathf.FloorToInt(Time.unscaledTime / 0.35f) % 2;
        for (int i = 0; i < starterImages.Length && i < starterData.Length; i++)
            if (starterImages[i] != null && starterData[i] != null)
                starterImages[i].sprite = starterData[i].IdleFrame(frame);
    }

    // Tantangan tugas kelompok P6: dua slot save (save_0 dan save_1).
    private void OnPicked(int index)
    {
        if (services == null) return;
        string choice = options[index];
        int slot = choice.EndsWith("2") ? 1 : 0;
        services.Save.Slot = slot;
        if (choice.StartsWith("Lanjutkan")) Continue();
        else if (choice.StartsWith("Mulai baru")) NewGame();
        else Quit();
    }

    private void Continue()
    {
        SaveData data = services.Save.Load();
        if (data == null) { NewGame(); return; }
        SaveMapper.Apply(data, services);
        services.Save.PendingLoad = data; // posisi dipakai WorldInstaller
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void NewGame()
    {
        services.Party.Reset();                 // Ramuan 3, tanpa partner
        services.Flags.Load(new List<string>()); // narasi diputar lagi
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
