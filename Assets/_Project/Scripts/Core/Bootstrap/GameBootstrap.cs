// Pulau Monster · P2 Arsitektur & World Tilemap (diperluas P4, P6) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Composition Root: satu-satunya tempat service dibuat. Scn_Bootstrap selalu hidup, scene lain dimuat di atasnya.
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = SceneFlow.TitleScene;
    [SerializeField] private MonsterDatabase monsterDatabase;
    [SerializeField] private ScreenFader fader;
    [SerializeField] private DialogueRunner dialogue;
    [SerializeField] private ToastView toast;
    [SerializeField] private AudioService audio;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private bool disableMoveInput;

    private GameServices services;

    public GameServices Services => services;

    void Awake()
    {
        IMoveInput move = disableMoveInput ? new DisabledMoveInput() : new InputSystemMoveInput();
        var gate = new InputGate(move, new InputSystemActionInput());
        services = new GameServices
        {
            Input = gate,
            Database = monsterDatabase,
            Party = new PlayerParty(),
            Battle = new BattleSession(),
            Flags = new StoryFlags(),
            Save = new SaveService(),
            Toast = toast,
            Dialogue = dialogue,
            Audio = audio,
        };
        services.Scenes = new SceneFlow(this, fader, services);
        if (dialogue != null) dialogue.Construct(services);
        if (pauseMenu != null) pauseMenu.Construct(services);
    }

    void Start() => services.Scenes.GoTo(firstScene);

    void Update()
    {
        bool paused = pauseMenu != null && pauseMenu.IsOpen;
        if ((services.Scenes.InGame || services.Scenes.InBattle) && !paused)
            services.Party.Playtime += Time.deltaTime;

        if (MenuInput.Mute && audio != null)
        {
            bool muted = audio.ToggleMute();
            if (toast != null) toast.Show(muted ? "Suara dimatikan (M)" : "Suara dinyalakan (M)");
        }
    }
}
