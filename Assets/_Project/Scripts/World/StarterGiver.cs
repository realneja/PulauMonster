// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Tetua Rahmat: Pilih partner → Yakin? → Ya / Pilih lagi. Lalu flag has_starter membuka jalan ke hutan.
public class StarterGiver : Npc
{
    [SerializeField] private MonsterData[] starters;
    [SerializeField] private int starterLevel = 3;
    [SerializeField] private AudioClip getClip; // SFX_LevelUp

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

    protected override IEnumerator Talk()
    {
        if (Flags.Has(StoryFlags.HasStarter))
        {
            yield return Dialogue.Say(NpcName, "Rawat partnermu baik-baik. Raijugar menunggu di Gua Petir, di timur laut.");
            yield return Dialogue.Say(NpcName, "Batu Simpan di sebelahku menyimpan perjalananmu dan memulihkan partnermu.");
            yield break;
        }

        yield return Dialogue.Say(NpcName, "Ah, kamu datang juga! Hutan di utara penuh monster liar.");
        yield return Dialogue.Say(NpcName, "Tidak ada yang boleh masuk hutan tanpa partner. Pilih satu dari tiga ini.");

        var options = new string[starters.Length];
        for (int i = 0; i < starters.Length; i++) options[i] = starters[i].DisplayName;

        MonsterData pick = null;
        while (pick == null) // ulangi sampai "Ya"
        {
            int choice = 0, sure = 0;
            yield return Dialogue.Choose(NpcName, "Siapa yang akan menjadi partnermu?", options, c => choice = c);
            MonsterData m = starters[choice];
            yield return Dialogue.Choose(NpcName, $"{m.DisplayName} tipe {m.Element}. Yakin?",
                new[] { "Ya", "Pilih lagi" }, c => sure = c);
            if (sure == 0) pick = m;
        }

        Party.SetPartner(new MonsterInstance(pick, starterLevel));
        Flags.Set(StoryFlags.HasStarter); // Penjaga bergeser, palang hilang
        if (Audio != null) Audio.PlaySfx(getClip, 0f);
        yield return Dialogue.Say("", $"Kamu mendapat {pick.DisplayName}!");
        yield return Dialogue.Say(NpcName, "Rawat dia baik-baik. Menang bertarung membuatnya naik level, lalu berevolusi.");
        yield return Dialogue.Say(NpcName, "Batu Simpan di sebelahku menyimpan perjalananmu dan memulihkan partnermu.");
        if (Toast != null) Toast.Show("Penjaga Batas sudah membuka jalan ke hutan");
    }
}
