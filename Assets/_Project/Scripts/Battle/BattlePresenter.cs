// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;

// Presenter (MVP): menghubungkan event BattleSystem ke BattleView/MonsterPanel, dan klik menu kembali ke BattleSystem.
public class BattlePresenter
{
    private readonly BattleSystem battle;
    private readonly BattleView view;

    public BattlePresenter(BattleSystem battleSystem, BattleView battleView)
    {
        battle = battleSystem;
        view = battleView;

        battle.Started += OnStarted; // isi panel + tirai pembuka
        battle.MessageShown += view.ShowMessage;
        battle.ChoiceRequested += OnChoiceRequested; // label, usable, prompt, hint
        battle.Attacked += m => PanelOf(m).PlayLunge();
        battle.Hit += OnHit; // Show + PlayHurt
        battle.Healed += m => PanelOf(m).Show(m, false);
        battle.Fainted += m => PanelOf(m).PlayFaint();
        battle.LevelUp += shown => view.PlayerPanel.Show(battle.Player, false, shown);
        battle.Evolving += (from, to) =>
        {
            view.PlayerPanel.PlayEvolve(from, to, 2.4f);
            view.PlayEvolveRings(2.4f);
        };
        battle.Evolved += m =>
        {
            view.PlayFlash();
            view.PlayerPanel.Show(m, false);
        };
        view.ActionClicked += OnActionClicked;
    }

    private void OnStarted()
    {
        view.EnemyPanel.Show(battle.Enemy);
        view.PlayerPanel.Show(battle.Player);
        view.PlayIntro();
    }

    private void OnChoiceRequested()
    {
        var labels = new List<string>();
        var usable = new List<bool>();
        foreach (BattleAction action in battle.Actions)
        {
            labels.Add(action.Label(battle));
            usable.Add(action.IsUsable(battle));
        }
        view.ShowMenu(labels, usable,
            $"Apa yang akan {battle.Player.Data.DisplayName} lakukan?",
            $"Jurus elemen: {battle.Player.Data.SkillName} ({battle.SkillUsesLeft}x)");
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        MonsterPanel panel = PanelOf(target);
        panel.Show(target, false);
        panel.PlayHurt();
        view.ShowDamage(panel.Body, damage, battle.LastHitCritical);
        if (target == battle.Enemy && battle.LastHitElement.HasValue
            && ElementChart.Multiplier(battle.LastHitElement.Value, target.Data.Element) > 1f)
            view.PlayerPanel.ShowStar();
    }

    private void OnActionClicked(int index)
    {
        if (index < 0 || index >= battle.Actions.Count) return;
        battle.Choose(battle.Actions[index]);
    }

    private MonsterPanel PanelOf(MonsterInstance m) => m == battle.Enemy ? view.EnemyPanel : view.PlayerPanel;
}
