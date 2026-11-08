// Pulau Monster · P6 Tugas Kelompok · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;

// NPC baru turunan Npc: Pak Nelayan di tepi Danau Sunyi. Dua cabang dialog dan satu flag (nelayan_tips).
public class FishermanNpc : Npc
{
    public const string TipsFlag = "nelayan_tips";

    protected override IEnumerator Talk()
    {
        if (!Flags.Has(TipsFlag))
        {
            int answer = 1;
            yield return Dialogue.Say(NpcName, "Danau ini sunyi, tapi ada Finnox yang suka berjemur di tepinya.");
            yield return Dialogue.Choose(NpcName, "Mau dengar tips dari nelayan tua ini?", new[] { "Mau", "Nanti saja" }, c => answer = c);
            if (answer != 0)
            {
                yield return Dialogue.Say(NpcName, "Ya sudah. Danau tidak ke mana-mana.");
                yield break;
            }
            Flags.Set(TipsFlag);
            yield return Dialogue.Say(NpcName, "Finnox bertipe Aqua. Jurus Sprout atau Volt membuatnya cepat kalah.");
            if (Toast != null) Toast.Show("Tips Pak Nelayan dicatat");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Sudah bertemu Finnox? Ingat, jangan lawan dia dengan jurus Pyro.");
        }
    }
}
