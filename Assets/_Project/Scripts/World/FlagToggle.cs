// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Menyembunyikan objek (palang Gate_Hutan) begitu sebuah flag cerita terpasang.
public class FlagToggle : MonoBehaviour
{
    [SerializeField] private string flagKey = StoryFlags.HasStarter;
    [SerializeField] private GameObject hideWhenSet;

    private StoryFlags flags;

    public void Construct(StoryFlags storyFlags)
    {
        flags = storyFlags;
        flags.Changed += OnFlag;
        Apply();
    }

    void OnDestroy()
    {
        if (flags != null) flags.Changed -= OnFlag;
    }

    private void OnFlag(string key) => Apply();

    private void Apply()
    {
        if (hideWhenSet != null) hideWhenSet.SetActive(!flags.Has(flagKey));
    }
}
