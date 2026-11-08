// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Suara battle: mendengarkan event BattleSystem lalu memutar SFX lewat AudioService.
public class BattleAudio : MonoBehaviour
{
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip hitCritical;
    [SerializeField] private AudioClip miss;
    [SerializeField] private AudioClip levelUp;
    [SerializeField] private AudioClip evolve;
    [SerializeField] private AudioClip victory;
    [SerializeField] private AudioClip faint;
    [SerializeField] private AudioClip heal;
    [SerializeField] private AudioClip encounter;

    private BattleSystem battle;
    private AudioService audio;

    public void Construct(BattleSystem battleSystem, AudioService audioService)
    {
        battle = battleSystem;
        audio = audioService;
        if (audio == null) return;
        battle.Started += () => audio.PlaySfx(encounter);
        battle.Hit += (_, __) => audio.PlaySfx(battle.LastHitCritical ? hitCritical : hit);
        battle.Missed += () => audio.PlaySfx(miss);
        battle.LevelUp += _ => audio.PlaySfx(levelUp, 0f);
        battle.Evolving += (_, __) => audio.PlaySfx(evolve, 0f);
        battle.Victory += () => audio.PlaySfx(victory, 0f);
        battle.Fainted += _ => audio.PlaySfx(faint);
        battle.Healed += _ => audio.PlaySfx(heal);
    }
}
