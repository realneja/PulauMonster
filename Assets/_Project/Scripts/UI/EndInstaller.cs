// Pulau Monster · P7 Polish, Build & Demo · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;
using UnityEngine.UI;

// Layar DEMO SELESAI: partner akhir + level, waktu bermain, Enter / klik kembali ke judul.
public class EndInstaller : SceneInstaller
{
    [SerializeField] private Image partnerImage;
    [SerializeField] private Text partnerText;
    [SerializeField] private Text timeText;
    [SerializeField] private Button clickCatcher;

    private GameServices services;
    private MonsterInstance partner;
    private float ready;
    private bool leaving;

    public override void Install(GameServices s)
    {
        services = s;
        partner = s.Party.Partner;
        int total = Mathf.FloorToInt(s.Party.Playtime);
        partnerText.text = partner != null ? $"{partner.Data.DisplayName} · Lv {partner.Level}" : "";
        timeText.text = $"Waktu bermain {total / 60} menit {total % 60} detik";
        if (partnerImage != null) partnerImage.enabled = partner != null;
        if (clickCatcher != null) clickCatcher.onClick.AddListener(Leave);
        ready = Time.unscaledTime + 0.5f; // jeda 0,5 s
    }

    void Update()
    {
        if (services == null) return;
        if (partner != null && partnerImage != null) // 2 frame, 0,35 s
            partnerImage.sprite = partner.Data.IdleFrame(Mathf.FloorToInt(Time.unscaledTime / 0.35f) % 2);
        if (MenuInput.Confirm) Leave();
    }

    private void Leave()
    {
        if (leaving || Time.unscaledTime < ready) return;
        leaving = true;
        services.Scenes.GoTo(SceneFlow.TitleScene);
    }
}
