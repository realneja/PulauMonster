// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Satu class abstrak, empat turunan: Serang, Jurus (elemen), Ramuan (prioritas 1), Kabur (prioritas 2).
public abstract class BattleAction
{
    public virtual int Priority => 0;
    public abstract string Label(BattleSystem battle);
    public virtual bool IsUsable(BattleSystem battle) => true;
    public abstract IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target);
}

public class AttackAction : BattleAction
{
    protected virtual int Power => 14;
    protected virtual float MissChance => 0.06f;

    public override string Label(BattleSystem battle) => "Serang";

    // null = Serang biasa; Jurus menulis ulang dengan nama jurus elemen.
    protected virtual string Move(MonsterInstance user) => null;
    protected virtual float ElementBonus(MonsterInstance user, MonsterInstance target) => 1f;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        string who = (user == battle.Enemy && !battle.IsBoss ? "Lawan " : "") + user.Data.DisplayName;
        string move = Move(user);
        yield return battle.Say(move == null ? $"{who} menyerang!" : $"{who} memakai {move}!", 0.7f);
        battle.RaiseAttacked(user); // animasi menerjang
        yield return new WaitForSeconds(0.2f);
        if (Random.value < MissChance)
        {
            battle.RaiseMissed();
            yield return battle.Say("Serangan meleset!", 0.8f);
            yield break;
        }

        float bonus = ElementBonus(user, target);
        int damage = DamageFormula.Calculate(user, target, Power, bonus * battle.DamageScale(user) * battle.GuardScale(target), out bool critical);
        target.TakeDamage(damage);
        battle.RaiseHit(target, damage, critical, move != null ? user.Data.Element : (Element?)null);
        yield return new WaitForSeconds(0.45f);
        if (critical) yield return battle.Say("Serangan kritis!", 0.7f);
        if (bonus > 1f) yield return battle.Say("Sangat efektif!", 0.7f);
        else if (bonus < 1f) yield return battle.Say("Kurang efektif...", 0.7f);
    }
}

public class SkillAction : AttackAction
{
    protected override int Power => 22;

    public override string Label(BattleSystem battle) => battle.Player.Data.SkillName;
    public override bool IsUsable(BattleSystem battle) => battle.SkillUsesLeft > 0;

    protected override string Move(MonsterInstance user) => user.Data.SkillName;
    protected override float ElementBonus(MonsterInstance user, MonsterInstance target) =>
        ElementChart.Multiplier(user.Data.Element, target.Data.Element);

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (user == battle.Player) battle.UseSkill();
        yield return base.Execute(battle, user, target);
    }
}

public class PotionAction : BattleAction
{
    public override int Priority => 1;
    public override string Label(BattleSystem battle) => $"Ramuan x{battle.Party.Potions}";
    public override bool IsUsable(BattleSystem battle) =>
        battle.Party.Potions > 0 && battle.Player.CurrentHP < battle.Player.MaxHP;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (!battle.Party.UsePotion())
        {
            yield return battle.Say("Ramuan sudah habis!", 0.7f);
            yield break;
        }
        int healed = user.Heal(Mathf.RoundToInt(user.MaxHP * 0.4f));
        battle.RaiseHealed(user);
        yield return battle.Say($"{user.Data.DisplayName} meminum Ramuan dan pulih {healed} HP.", 0.8f);
    }
}

public class RunAction : BattleAction
{
    public override int Priority => 2;
    public override string Label(BattleSystem battle) => "Kabur";

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (battle.IsBoss)
        {
            yield return battle.Say("Tidak bisa kabur dari penguasa Gua Petir!", 0.8f);
            yield break;
        }
        if (Random.value < 0.7f)
        {
            battle.SetResult(BattleResult.Run);
            yield return battle.Say("Berhasil kabur!", 0.8f);
        }
        else
        {
            yield return battle.Say("Gagal kabur!", 0.8f);
        }
    }
}

// Tugas kelompok P5: Bertahan (prioritas 1), damage yang diterima pada giliran itu x 0,5.
public class GuardAction : BattleAction
{
    public override int Priority => 1;

    public override string Label(BattleSystem battle) => "Bertahan";

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        battle.SetGuarding(user);
        yield return battle.Say($"{user.Data.DisplayName} bersiap bertahan!", 0.7f);
    }
}
