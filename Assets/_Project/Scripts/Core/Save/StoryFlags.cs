// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections.Generic;

// Flag cerita: has_starter, gift, intro_seen, boss_defeated. Disimpan sebagai daftar string di save.
public class StoryFlags
{
    public const string HasStarter = "has_starter";
    public const string Gift = "gift";
    public const string IntroSeen = "intro_seen";
    public const string BossDefeated = "boss_defeated";

    private readonly HashSet<string> flags = new HashSet<string>();

    public event Action<string> Changed;

    public bool Has(string key) => flags.Contains(key);

    public void Set(string key)
    {
        if (flags.Add(key)) Changed?.Invoke(key);
    }

    public void Load(List<string> keys)
    {
        flags.Clear();
        if (keys != null) foreach (string key in keys) flags.Add(key);
        Changed?.Invoke("");
    }

    public List<string> ToList() => new List<string>(flags);
}
