// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using UnityEngine;

// Capture: Party + Flags + posisi menjadi SaveData. Apply: SaveData kembali ke service (partner lewat Database).
public static class SaveMapper
{
    public static SaveData Capture(GameServices s, Vector2 playerPosition)
    {
        MonsterInstance partner = s.Party.Partner;
        return new SaveData
        {
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            scene = SceneFlow.WorldScene,
            playerPosition = playerPosition,
            partnerId = partner != null ? partner.Data.Id : "",
            partnerLevel = partner != null ? partner.Level : 0,
            partnerExp = partner != null ? partner.Exp : 0,
            partnerHp = partner != null ? partner.CurrentHP : 0,
            potions = s.Party.Potions,
            playtime = s.Party.Playtime,
            flags = s.Flags.ToList(),
        };
    }

    public static void Apply(SaveData data, GameServices s)
    {
        MonsterInstance partner = null;
        MonsterData species = string.IsNullOrEmpty(data.partnerId) ? null : s.Database.GetById(data.partnerId);
        if (species != null)
            partner = new MonsterInstance(species, data.partnerLevel, data.partnerExp, data.partnerHp);
        s.Party.Load(partner, data.potions, data.playtime);
        s.Flags.Load(data.flags);
    }
}
