// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Pak Dagang: hadiah 3 Ramuan sekali saja (flag gift).
public class MerchantNpc : Npc
{
    [SerializeField] private int giftPotions = 3;
    [SerializeField] private AudioClip giftClip; // SFX_Heal

    protected override IEnumerator Talk()
    {
        if (!Flags.Has(StoryFlags.Gift))
        {
            yield return Dialogue.Say(NpcName, "Petualang baru, ya? Ramuan ini untukmu, gratis untuk hari pertama.");
            Party.AddPotions(giftPotions);
            Flags.Set(StoryFlags.Gift);
            if (Audio != null) Audio.PlaySfx(giftClip, 0f);
            yield return Dialogue.Say("", $"Kamu mendapat {giftPotions} Ramuan!");
            if (Toast != null) Toast.Show($"Ramuan x{Party.Potions}");
            yield return Dialogue.Say(NpcName, "Ramuan memulihkan 40% HP partnermu di tengah battle. Pakai saat terdesak.");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Stokku sudah habis. Cari Batu Simpan kalau partnermu lelah.");
        }
    }
}
