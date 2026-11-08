// Pulau Monster · P2 Arsitektur & World Tilemap (WorldInstaller versi P7) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Menyambungkan isi Scn_World dengan service: Player, NPC, monster liar, HUD, narasi pembuka, ending.
public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private PartnerHud hud;
    [SerializeField] private AudioClip villageMusic;
    [SerializeField] private Vector2 respawnPoint = new Vector2(21.5f, 6f); // depan Batu Simpan
    [SerializeField, TextArea(2, 4)] private string[] introLines =
    {
        "Di ujung Pulau Batam ada desa kecil yang dijaga monster-monster elemen.",
        "Petir di gua utara makin sering menyambar. Tetua desa menunggu seorang petualang baru: kamu.",
    };
    [SerializeField, TextArea(2, 4)] private string[] endingLines =
    {
        "Raijugar menunduk dan membiarkanmu lewat. Petir di Gua Petir perlahan reda.",
        "Desa aman kembali. Perjalananmu bersama partnermu baru saja dimulai...",
    };

    private GameServices services;

    public override void Install(GameServices s)
    {
        services = s;
        player.Construct(s.Input);
        interactor.Construct(s.Input);
        if (follower != null) follower.Construct(s.Party, player);
        if (hud != null) hud.Construct(s.Party, s.Battle);

        foreach (Npc npc in FindInScene<Npc>()) npc.Construct(s);
        foreach (SaveStone stone in FindInScene<SaveStone>()) stone.Construct(s, player.transform);
        foreach (FlagToggle toggle in FindInScene<FlagToggle>()) toggle.Construct(s.Flags);
        foreach (AreaZone zone in FindInScene<AreaZone>()) zone.Construct(s.Audio, hud);
        foreach (WildMonster wild in FindInScene<WildMonster>())
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform, s.Flags, s.Input, s.Dialogue);

        s.Battle.Ended += OnBattleEnded;
        if (s.Audio != null && villageMusic != null) s.Audio.PlayWorldMusic(villageMusic);

        if (s.Save.PendingLoad != null)
        {
            player.Teleport(s.Save.PendingLoad.playerPosition, Vector2.down);
            s.Save.PendingLoad = null;
            if (s.Toast != null) s.Toast.Show("Perjalanan dilanjutkan");
        }
        else if (!s.Flags.Has(StoryFlags.IntroSeen))
        {
            s.Scenes.HoldFade = true; // layar tetap hitam selama narasi
            StartCoroutine(Intro());
        }
    }

    void OnDestroy()
    {
        if (services != null) services.Battle.Ended -= OnBattleEnded;
    }

    private IEnumerator Intro()
    {
        yield return null;
        services.Dialogue.Run(IntroLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;
        services.Flags.Set(StoryFlags.IntroSeen);
        yield return services.Scenes.ReleaseFade(); // fade-in
        if (services.Toast != null) services.Toast.Show("Temui Tetua Rahmat (kanan jalan desa)");
    }

    private IEnumerator IntroLines()
    {
        foreach (string line in introLines)
            yield return services.Dialogue.Narrate(line);
    }

    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lose)
        {
            player.Teleport(respawnPoint, Vector2.up);
            services.Party.Partner?.HealFull();
            if (services.Toast != null) services.Toast.Show("Kamu dibawa kembali ke depan Batu Simpan");
        }
        else if (result == BattleResult.Win && wasBoss)
        {
            StartCoroutine(Ending());
        }
    }

    private IEnumerator Ending()
    {
        while (services.Scenes.IsBusy) yield return null;
        services.Dialogue.Run(EndingLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;
        Vector3 pos = player.transform.position;
        services.Save.Save(SaveMapper.Capture(services, pos));
        services.Scenes.GoTo(SceneFlow.EndScene);
    }

    private IEnumerator EndingLines()
    {
        foreach (string line in endingLines)
            yield return services.Dialogue.Narrate(line);
    }
}
