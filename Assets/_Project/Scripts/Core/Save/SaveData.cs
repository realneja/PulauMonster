// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections.Generic;
using UnityEngine;

// Isi save_0.json: posisi pemain, partner (id, bukan aset), Ramuan, flag, waktu bermain.
[Serializable]
public class SaveData
{
    public int version = SaveService.CurrentVersion;
    public string savedAt;
    public string scene = SceneFlow.WorldScene;
    public Vector2 playerPosition;
    public string partnerId;
    public int partnerLevel, partnerExp, partnerHp;
    public int potions;
    public float playtime;
    public List<string> flags = new List<string>();
}
