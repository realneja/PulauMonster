// Pulau Monster · P4 Data Monster & Monster Liar (versi P5, P6, P7) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Installer Scn_Battle: latar sesuai boss, musik, Construct BattleSystem, presenter, audio, game feel, lalu Begin.
public class BattleInstaller : SceneInstaller
{
    [SerializeField] private BattleSystem battle;
    [SerializeField] private BattleView view;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private Sprite caveBackground;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private BattleAudio battleAudio;
    [SerializeField] private BattleFeel feel;

    public override void Install(GameServices s)
    {
        if (background != null) background.sprite = s.Battle.IsBoss ? caveBackground : forestBackground;
        if (s.Audio != null) s.Audio.PlayBattleMusic(s.Battle.IsBoss ? bossMusic : battleMusic);
        view.Menu.Sfx = s.Audio;
        battle.Construct(s.Party, s.Battle, s.Scenes);
        new BattlePresenter(battle, view);
        if (battleAudio != null) battleAudio.Construct(battle, s.Audio);
        if (feel != null) feel.Construct(battle);
        battle.Begin();
    }
}
