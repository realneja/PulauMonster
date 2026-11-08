// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Penjaga Batas: berdiri di celah pagar sampai pemain punya partner, lalu bergeser ke samping.
public class GuardNpc : Npc
{
    [SerializeField] private Vector2 asidePosition = new Vector2(22.5f, 10.4f);

    protected override void OnConstructed()
    {
        Flags.Changed += OnFlag;
        if (Flags.Has(StoryFlags.HasStarter)) StepAside();
    }

    void OnDestroy()
    {
        if (Flags != null) Flags.Changed -= OnFlag;
    }

    private void OnFlag(string key)
    {
        if (key == StoryFlags.HasStarter) StepAside();
    }

    private void StepAside() => transform.position = asidePosition;

    protected override IEnumerator Talk()
    {
        if (!Flags.Has(StoryFlags.HasStarter))
            yield return Dialogue.Say(NpcName, "Berhenti! Hutan terlalu berbahaya tanpa monster partner. Temui Tetua dulu.");
        else
            yield return Dialogue.Say(NpcName, "Jalan terbuka. Monster di rumput tinggi akan mengejarmu kalau terlalu dekat.");
    }
}
