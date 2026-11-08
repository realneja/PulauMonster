// Pulau Monster · P4 Data Monster & Monster Liar (diperluas P5, P6) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using UnityEngine;

// Partner pemain + Ramuan + waktu bermain. Service biasa, dibuat di GameBootstrap.
public class PlayerParty
{
    public MonsterInstance Partner { get; private set; }
    public int Potions { get; private set; } = 3;
    public float Playtime { get; set; }

    public event Action PartnerChanged;
    public event Action PotionsChanged;

    public void SetPartner(MonsterInstance monster)
    {
        Partner = monster;
        PartnerChanged?.Invoke();
    }

    public void AddPotions(int count)
    {
        Potions = Mathf.Max(0, Potions + count);
        PotionsChanged?.Invoke();
    }

    public bool UsePotion()
    {
        if (Potions <= 0) return false;
        Potions--;
        PotionsChanged?.Invoke();
        return true;
    }

    // Mulai baru: Ramuan 3, tanpa partner.
    public void Reset()
    {
        Partner = null;
        Potions = 3;
        Playtime = 0f;
        PartnerChanged?.Invoke();
        PotionsChanged?.Invoke();
    }

    public void Load(MonsterInstance partner, int potions, float playtime)
    {
        Partner = partner;
        Potions = Mathf.Max(0, potions);
        Playtime = Mathf.Max(0f, playtime);
        PartnerChanged?.Invoke();
        PotionsChanged?.Invoke();
    }
}
