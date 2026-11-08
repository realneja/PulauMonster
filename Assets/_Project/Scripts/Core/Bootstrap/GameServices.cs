// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam

// Kumpulan service yang dibuat sekali di Composition Root (GameBootstrap) dan dibagikan ke setiap scene.
public class GameServices
{
    public InputGate Input;
    public MonsterDatabase Database;
    public PlayerParty Party;
    public BattleSession Battle;
    public SceneFlow Scenes;
    public StoryFlags Flags;
    public SaveService Save;
    public ToastView Toast;
    public DialogueRunner Dialogue;
    public AudioService Audio;
}
