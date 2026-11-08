// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using UnityEngine;

// Daftar 12 MonsterData; save menyimpan id, lalu GetById mengembalikan asetnya.
[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Pulau Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();

    public IReadOnlyList<MonsterData> Monsters => monsters;

    public MonsterData GetById(string id)
    {
        foreach (MonsterData m in monsters)
            if (m != null && m.Id == id) return m;
        return null;
    }

    public MonsterData GetByName(string displayName)
    {
        foreach (MonsterData m in monsters)
            if (m != null && m.DisplayName == displayName) return m;
        return null;
    }

#if UNITY_EDITOR
    public void EditorSet(List<MonsterData> list) => monsters = list;
#endif
}
