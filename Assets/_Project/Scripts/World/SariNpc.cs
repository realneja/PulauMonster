// Pulau Monster · P6 Tugas Kelompok · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;

// Sari: Talk() menggantikan DialogueData, isinya berubah setelah flag boss_defeated.
public class SariNpc : Npc
{
    protected override IEnumerator Talk()
    {
        if (!Flags.Has(StoryFlags.BossDefeated))
        {
            yield return Dialogue.Say(NpcName, "Tips dariku: Pyro kuat melawan Sprout, Aqua kuat melawan Pyro.");
            yield return Dialogue.Say(NpcName, "Sprout kuat melawan Aqua, dan Volt juga kuat melawan Aqua. Pilih jurus yang tepat!");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Kamu mengalahkan Raijugar? Petir di gua sudah reda sejak tadi!");
            yield return Dialogue.Say(NpcName, "Terima kasih. Sekarang anak-anak desa berani bermain ke hutan lagi.");
        }
    }
}
