// Pulau Monster · P6 Dialog, Save & Audio (kilau P7) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Batu Simpan: pulihkan HP partner, simpan save_0.json, kilau biru.
public class SaveStone : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip saveClip; // SFX_Save
    [SerializeField] private PixelBurst burst;

    private GameServices services;
    private Transform player;

    public void Construct(GameServices gameServices, Transform playerTransform)
    {
        services = gameServices;
        player = playerTransform;
    }

    public void Interact(PlayerInteractor who)
    {
        if (services == null || services.Dialogue.IsRunning) return;
        services.Dialogue.Run(Use());
    }

    private IEnumerator Use()
    {
        if (services.Audio != null) services.Audio.PlaySfx(saveClip, 0f);
        if (services.Party.Partner != null)
        {
            services.Party.Partner.HealFull();
            if (burst != null)
                burst.Play(player.position + Vector3.up * 0.5f, new Color32(110, 231, 242, 255), 16, 50f, 0.7f);
        }
        bool saved = services.Save.Save(SaveMapper.Capture(services, player.position));
        yield return services.Dialogue.Say("", saved
            ? "Batu Simpan bersinar. HP partner pulih dan perjalanan tersimpan."
            : "Batu Simpan bersinar. HP partner pulih, tetapi penyimpanan gagal.");
    }
}
