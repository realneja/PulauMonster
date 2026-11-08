// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// static: tanpa state, mudah diuji. Kritis 8% → x1,5; acak 0,9 sampai 1,1; minimal 1.
public static class DamageFormula
{
    public const float CriticalChance = 0.08f;

    public static int Calculate(MonsterInstance user, MonsterInstance target, int power,
        float multiplier, out bool critical)
    {
        critical = Random.value < CriticalChance;
        float raw = user.Attack * power / (target.Defense * 0.5f + 10f);
        raw *= multiplier * (critical ? 1.5f : 1f) * Random.Range(0.9f, 1.1f);
        return Mathf.Max(1, Mathf.FloorToInt(raw));
    }
}
