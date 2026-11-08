// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections.Generic;
using UnityEngine;

// Data hidup satu monster: level, EXP, HP. Bukan MonoBehaviour: bisa dibawa ke battle dan disimpan ke file.
public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int CurrentHP { get; private set; }

    public event Action Changed; // P5: HUD dan panel battle

    private float Scale => 1f + (Level - 1) * 0.1f;
    public int MaxHP => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed => Mathf.RoundToInt(Data.BaseSPD * Scale);
    public int ExpToNext => Level * 10;
    public bool IsFainted => CurrentHP <= 0;

    public MonsterInstance(MonsterData data, int level)
    {
        Data = data;
        Level = Mathf.Max(1, level);
        CurrentHP = MaxHP;
    }

    public MonsterInstance(MonsterData data, int level, int exp, int hp)
    {
        Data = data;
        Level = Mathf.Max(1, level);
        Exp = Mathf.Max(0, exp);
        CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Changed?.Invoke();
    }

    // Mengembalikan HP yang benar-benar pulih.
    public int Heal(int amount)
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHP);
        Changed?.Invoke();
        return CurrentHP - before;
    }

    public void HealFull() => Heal(MaxHP);

    // Naik level selama EXP cukup; bila Level >= EvolveLevel, Data berganti ke EvolvesTo.
    public List<GrowthEvent> GainExp(int amount)
    {
        var events = new List<GrowthEvent>();
        Exp += Mathf.Max(0, amount);
        while (Exp >= ExpToNext)
        {
            Exp -= ExpToNext;
            int oldMax = MaxHP;
            Level++;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + (MaxHP - oldMax));
            events.Add(new GrowthEvent(GrowthType.LevelUp, Data, Data, Level,
                $"{Data.DisplayName} naik ke Lv {Level}!"));

            if (Data.EvolvesTo != null && Data.EvolveLevel > 0 && Level >= Data.EvolveLevel)
            {
                MonsterData from = Data;
                int beforeMax = MaxHP;
                Data = Data.EvolvesTo;
                CurrentHP = Mathf.Min(MaxHP, CurrentHP + Mathf.Max(0, MaxHP - beforeMax));
                events.Add(new GrowthEvent(GrowthType.Evolve, from, Data, Level,
                    $"{from.DisplayName} berevolusi menjadi {Data.DisplayName}!"));
            }
        }
        Changed?.Invoke();
        return events;
    }
}
