// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Uji MonsterInstance tanpa battle: titik tiga component di Inspector saat Play.
public class MonsterDebugger : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private int level = 4;

    private MonsterInstance monster;

    [ContextMenu("1. Buat monster")]
    private void Create()
    {
        monster = new MonsterInstance(species, level);
        Debug.Log($"{monster.Data.DisplayName} Lv {monster.Level}: HP {monster.CurrentHP}/{monster.MaxHP}, ATK {monster.Attack}, EXP {monster.ExpToNext}");
    }

    [ContextMenu("2. Beri 10 damage")]
    private void Damage()
    {
        if (monster == null) Create();
        monster.TakeDamage(10);
        Debug.Log($"{monster.Data.DisplayName} HP {monster.CurrentHP}/{monster.MaxHP}");
    }

    [ContextMenu("3. Beri 100 EXP")]
    private void Exp()
    {
        if (monster == null) Create();
        foreach (GrowthEvent growth in monster.GainExp(100))
            Debug.Log(growth.Message);
    }
}
