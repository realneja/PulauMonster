// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Alur giliran sebagai coroutine + event. Logika saja: tampilan diurus BattlePresenter/BattleView (MVP).
public class BattleSystem : MonoBehaviour
{
    [SerializeField] private float secondsPerChar = 0.018f;
    [SerializeField] private float evolveSeconds = 2.4f;
    [SerializeField] private int skillUses = 3;

    private readonly List<BattleAction> actions = new List<BattleAction>
    {
        new AttackAction(), new SkillAction(), new PotionAction(), new RunAction(), new GuardAction()
    };

    private BattleSession session;
    private SceneFlow scenes;
    private BattleAction chosen;
    private BattleResult result;
    private int enemySkillUses = 2;

    public MonsterInstance Player { get; private set; }
    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public PlayerParty Party { get; private set; }
    public int SkillUsesLeft { get; private set; }
    public Element? LastHitElement { get; private set; }
    public bool LastHitCritical { get; private set; }
    public IReadOnlyList<BattleAction> Actions => actions;

    public event Action Started;
    public event Action<string> MessageShown;
    public event Action ChoiceRequested;
    public event Action<MonsterInstance> Attacked;
    public event Action<MonsterInstance, int> Hit;
    public event Action Missed;
    public event Action<MonsterInstance> Fainted;
    public event Action Victory;
    public event Action<MonsterData> LevelUp;
    public event Action<MonsterData, MonsterData> Evolving;
    public event Action<MonsterInstance> Evolved;
    public event Action<MonsterInstance> Healed;
    public event Action<BattleResult> Ended;

    public void Construct(PlayerParty party, BattleSession battleSession, SceneFlow sceneFlow)
    {
        Party = party;
        Player = party.Partner;
        session = battleSession;
        Enemy = battleSession.Enemy;
        IsBoss = battleSession.IsBoss;
        scenes = sceneFlow;
        SkillUsesLeft = skillUses;
        result = BattleResult.None;
    }

    public void Begin() => StartCoroutine(Run());

    public void Choose(BattleAction action) => chosen = action;
    public void UseSkill() => SkillUsesLeft = Mathf.Max(0, SkillUsesLeft - 1);
    public void SetResult(BattleResult value) => result = value;
    public float DamageScale(MonsterInstance user) => user == Enemy && !IsBoss ? 0.8f : 1f;
    public MonsterInstance Guarding { get; private set; }
    public void SetGuarding(MonsterInstance m) => Guarding = m;
    public float GuardScale(MonsterInstance target) => target == Guarding ? 0.5f : 1f;

    public void RaiseAttacked(MonsterInstance user) => Attacked?.Invoke(user);
    public void RaiseMissed() => Missed?.Invoke();
    public void RaiseHealed(MonsterInstance target) => Healed?.Invoke(target);

    public void RaiseHit(MonsterInstance target, int damage, bool critical, Element? element)
    {
        LastHitElement = element;
        LastHitCritical = critical;
        Hit?.Invoke(target, damage);
    }

    // Pesan tampil, lalu lanjut setelah (hold + panjang teks x 18 ms). E / Spasi / Enter mempercepat setelah teks selesai.
    public IEnumerator Say(string text, float hold = 0.8f)
    {
        MessageShown?.Invoke(text);
        float wait = hold + text.Length * secondsPerChar;
        for (float t = 0f; t < wait; t += Time.deltaTime)
        {
            if (t > text.Length * secondsPerChar && MenuInput.Confirm) break;
            yield return null;
        }
    }

    private IEnumerator Run()
    {
        Started?.Invoke();
        yield return new WaitForSeconds(0.5f); // tirai pembuka
        yield return Say(IsBoss
            ? $"{Enemy.Data.DisplayName}, penguasa Gua Petir, muncul!"
            : $"{Enemy.Data.DisplayName} liar Lv{Enemy.Level} muncul!", 0.9f);
        yield return Say($"Maju, {Player.Data.DisplayName}!", 0.7f);

        while (result == BattleResult.None)
        {
            chosen = null;
            Guarding = null;
            ChoiceRequested?.Invoke();
            yield return new WaitUntil(() => chosen != null);

            if (chosen is RunAction && IsBoss) // boss: giliran tidak hilang
            {
                yield return chosen.Execute(this, Player, Enemy);
                continue;
            }

            BattleAction enemyAction = PickEnemyAction(); // 35% Jurus, maksimal 2 kali
            bool playerFirst = chosen.Priority > 0 || Player.Speed > Enemy.Speed
                               || (Player.Speed == Enemy.Speed && UnityEngine.Random.value < 0.5f);
            if (playerFirst)
            {
                yield return Turn(chosen, Player, Enemy);
                yield return Turn(enemyAction, Enemy, Player);
            }
            else
            {
                yield return Turn(enemyAction, Enemy, Player);
                yield return Turn(chosen, Player, Enemy);
            }
        }

        Ended?.Invoke(result);
        yield return new WaitForSeconds(0.3f); // jeda sebelum fade
        scenes.EndBattle(result);
    }

    private IEnumerator Turn(BattleAction action, MonsterInstance user, MonsterInstance target)
    {
        if (result != BattleResult.None || user.IsFainted) yield break;
        yield return action.Execute(this, user, target);
        if (result != BattleResult.None) yield break;
        if (Enemy.IsFainted) yield return Win();
        else if (Player.IsFainted) yield return Lose();
    }

    private IEnumerator Win()
    {
        result = BattleResult.Win;
        Fainted?.Invoke(Enemy); // lawan tenggelam & memudar
        yield return new WaitForSeconds(0.6f);
        yield return Say(IsBoss ? $"{Enemy.Data.DisplayName} kalah!" : $"{Enemy.Data.DisplayName} liar kalah!", 0.8f);
        Victory?.Invoke(); // jingle menang
        int exp = (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1);
        yield return Say($"{Player.Data.DisplayName} mendapat {exp} EXP.", 0.9f);

        foreach (GrowthEvent growth in Player.GainExp(exp))
        {
            if (growth.Type == GrowthType.LevelUp)
            {
                LevelUp?.Invoke(growth.From);
                yield return Say(growth.Message, 0.9f);
                continue;
            }
            yield return Say($"Apa? {growth.From.DisplayName} mulai bersinar...", 0.9f);
            Evolving?.Invoke(growth.From, growth.To); // animasi 2,4 detik
            yield return new WaitForSeconds(evolveSeconds);
            Evolved?.Invoke(Player); // kilat putih + sprite baru
            yield return Say(growth.Message, 1.2f);
        }

        if (!IsBoss)
        {
            int healed = Player.Heal(Mathf.RoundToInt(Player.MaxHP * 0.25f));
            if (healed > 0)
            {
                Healed?.Invoke(Player);
                yield return Say($"{Player.Data.DisplayName} beristirahat sejenak dan pulih {healed} HP.", 0.8f);
            }
        }
    }

    private IEnumerator Lose()
    {
        result = BattleResult.Lose;
        Fainted?.Invoke(Player);
        yield return new WaitForSeconds(0.6f);
        yield return Say($"{Player.Data.DisplayName} pingsan...", 0.9f);
        yield return Say("Kamu dibawa kembali ke depan Batu Simpan.", 0.9f);
    }

    private BattleAction PickEnemyAction()
    {
        if (enemySkillUses > 0 && UnityEngine.Random.value < 0.35f)
        {
            enemySkillUses--;
            return new SkillAction();
        }
        return new AttackAction();
    }
}
