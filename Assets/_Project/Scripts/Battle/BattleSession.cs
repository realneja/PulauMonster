// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using UnityEngine;

// Membawa data battle antar-scene: musuh + callback untuk monster yang memulai, event untuk dunia.
public class BattleSession
{
    public const float SafeSeconds = 2.2f;

    private Action<BattleResult> onFinished;
    private float safeUntil;

    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public bool IsActive => Enemy != null;
    public bool IsSafe => Time.time < safeUntil;

    public event Action Started;
    public event Action<BattleResult, bool> Ended; // (hasil, boss?)

    public void Begin(MonsterInstance enemy, bool isBoss, Action<BattleResult> callback)
    {
        Enemy = enemy;
        IsBoss = isBoss;
        onFinished = callback;
        Started?.Invoke();
    }

    public void Finish(BattleResult result)
    {
        Action<BattleResult> callback = onFinished;
        bool wasBoss = IsBoss;
        onFinished = null;
        Enemy = null;
        IsBoss = false;
        safeUntil = Time.time + SafeSeconds;
        callback?.Invoke(result);      // monster yang memulai
        Ended?.Invoke(result, wasBoss); // dunia (P7: ending)
    }
}
