// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Esc di Scn_World: layar redup + menu Lanjut / Simpan sekarang / Kembali ke judul. Pemain terkunci selama terbuka.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMenu menu;

    private GameServices services;

    public bool IsOpen { get; private set; }

    public void Construct(GameServices gameServices)
    {
        services = gameServices;
        menu.Sfx = services.Audio;
        menu.Picked += OnPicked;
        root.SetActive(false);
    }

    private bool CanOpen =>
        services != null && services.Scenes.Current == SceneFlow.WorldScene && !services.Scenes.IsBusy
        && !services.Scenes.InBattle && !services.Input.IsLocked && !services.Dialogue.IsRunning;

    void Update()
    {
        if (services == null) return;
        if (!MenuInput.Cancel) return;
        if (IsOpen) Close();
        else if (CanOpen) Open();
    }

    private void Open()
    {
        IsOpen = true;
        services.Input.Lock();
        root.SetActive(true);
        menu.Open(new[] { "Lanjut", "Simpan sekarang", "Kembali ke judul" });
    }

    private void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        menu.Close();
        root.SetActive(false);
        services.Input.Unlock();
    }

    private void OnPicked(int index)
    {
        if (!IsOpen) return;
        switch (index)
        {
            case 1:
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                Vector2 pos = player != null ? (Vector2)player.transform.position : Vector2.zero;
                bool saved = services.Save.Save(SaveMapper.Capture(services, pos));
                services.Toast.Show(saved ? "Tersimpan" : "Penyimpanan gagal");
                Close();
                break;
            case 2:
                Close();
                services.Scenes.GoTo(SceneFlow.TitleScene);
                break;
            default:
                Close();
                break;
        }
    }
}
