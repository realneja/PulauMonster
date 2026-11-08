// Pulau Monster · P4 Data Monster & Monster Liar (versi P6) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Monster liar dengan FSM Wander -> Chase -> Return (+ Flee, tugas kelompok). Bersentuhan dengan Player -> battle.
[RequireComponent(typeof(Rigidbody2D))]
public class WildMonster : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private int level = 3;
    [SerializeField] private bool isBoss;
    [SerializeField] private bool stationary;
    [SerializeField] private float sightRadius = 3.25f;
    [SerializeField] private float giveUpRadius = 6.25f;
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderSpeed = 1.125f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float calmSeconds = 2.5f;
    [SerializeField] private float respawnSeconds = 20f;
    [SerializeField] private int fleeLevelGap = 5;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Collider2D hitbox;
    [SerializeField] private GameObject alert;
    [SerializeField, TextArea] private string roarText = "GRRRAAAH!! Raijugar menghalangi jalan ke dalam gua...";

    private BattleSession session;
    private SceneFlow scenes;
    private PlayerParty party;
    private StoryFlags flags;
    private InputGate gate;
    private DialogueRunner dialogue;
    private StateMachine fsm;
    private WanderState wander;
    private ChaseState chase;
    private ReturnState back;
    private FleeState flee;
    private Vector2 home;
    private Vector2 desiredVelocity;
    private float cooldown;
    private bool inBattle;
    private bool hidden;

    public MonsterData Species => species;
    public int Level => level;
    public bool IsBoss => isBoss;
    public Vector2 Home => home;
    public Vector2 Position => transform.position;
    public Transform Target { get; private set; }
    public float WanderRadius => wanderRadius;
    public float WanderSpeed => wanderSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float DistanceToTarget =>
        Target != null ? Vector2.Distance(transform.position, Target.position) : float.MaxValue;

    // Tenang 2,5 s setelah battle, 2,2 s waktu aman sesi, dan selama dialog (gate terkunci).
    private bool Calm => cooldown > 0f || (session != null && session.IsSafe) || (gate != null && gate.IsLocked);
    public bool CanSeeTarget => party != null && party.Partner != null && !Calm && DistanceToTarget < sightRadius;
    public bool LostTarget => Calm || DistanceToTarget > giveUpRadius;
    // Tugas kelompok: kabur bila partner pemain 5 level lebih tinggi.
    public bool ShouldFlee => !isBoss && party != null && party.Partner != null && party.Partner.Level >= level + fleeLevelGap;

    public void Construct(BattleSession battleSession, SceneFlow sceneFlow, PlayerParty playerParty,
        Transform target, StoryFlags storyFlags, InputGate inputGate, DialogueRunner dialogueRunner)
    {
        flags = storyFlags;
        gate = inputGate;
        dialogue = dialogueRunner;
        session = battleSession;
        scenes = sceneFlow;
        party = playerParty;
        Target = target;
        home = transform.position;
        wander = new WanderState(this);
        chase = new ChaseState(this);
        back = new ReturnState(this);
        flee = new FleeState(this);
        fsm = new StateMachine();
        fsm.ChangeTo(wander);
        if (isBoss && flags != null && flags.Has(StoryFlags.BossDefeated)) gameObject.SetActive(false);
    }

    public void EnterChase() => fsm.ChangeTo(ShouldFlee ? flee : chase);
    public void EnterReturn() => fsm.ChangeTo(back);
    public void EnterWander() => fsm.ChangeTo(wander);
    public void SetDesiredVelocity(Vector2 velocity) => desiredVelocity = velocity;

    void Update()
    {
        if (fsm == null) return;
        if (cooldown > 0f) cooldown -= Time.deltaTime;
        if (stationary || inBattle || Target == null) desiredVelocity = Vector2.zero;
        else fsm.Tick(Time.deltaTime);
        if (alert != null) alert.SetActive(fsm.Current == chase && !inBattle);
        if (body != null && Mathf.Abs(desiredVelocity.x) > 0.01f) body.flipX = desiredVelocity.x < 0f;
        Blink();
    }

    void FixedUpdate()
    {
        if (rb != null) rb.linearVelocity = desiredVelocity;
    }

    // Tantangan: berkedip selama waktu aman 2,2 detik setelah battle.
    private void Blink()
    {
        if (body == null || hidden) return;
        bool safe = session != null && session.IsSafe;
        body.enabled = !safe || Mathf.FloorToInt(Time.time / 0.1f) % 2 == 0;
    }

    void OnCollisionEnter2D(Collision2D collision) => TryBattle(collision.collider);
    void OnCollisionStay2D(Collision2D collision) => TryBattle(collision.collider);

    private void TryBattle(Collider2D other)
    {
        if (fsm == null || inBattle || Calm || !other.CompareTag("Player")) return;
        if (scenes.IsBusy || scenes.InBattle || session.IsActive || party.Partner == null) return;
        if (isBoss && dialogue != null) StartCoroutine(RoarThenBattle());
        else StartBattle();
    }

    // Bonus P6: boss mengaum lewat DialogueRunner, lalu battle dimulai.
    private IEnumerator RoarThenBattle()
    {
        inBattle = true;
        desiredVelocity = Vector2.zero;
        dialogue.Run(Roar());
        yield return null;
        while (dialogue.IsRunning) yield return null;
        StartBattle();
    }

    private IEnumerator Roar()
    {
        yield return dialogue.Say("", roarText);
    }

    private void StartBattle()
    {
        inBattle = true;
        desiredVelocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        session.Begin(new MonsterInstance(species, level), isBoss, OnBattleFinished);
        scenes.EnterBattle();
    }

    private void OnBattleFinished(BattleResult result)
    {
        inBattle = false;
        cooldown = calmSeconds;
        fsm.ChangeTo(wander);
        if (result != BattleResult.Win) return;
        if (isBoss)
        {
            flags?.Set(StoryFlags.BossDefeated);
            gameObject.SetActive(false);
        }
        else StartCoroutine(Respawn());
    }

    // Menang: monster hilang, muncul lagi di Home setelah 20 detik.
    private IEnumerator Respawn()
    {
        SetVisible(false);
        transform.position = home;
        yield return new WaitForSeconds(respawnSeconds);
        cooldown = calmSeconds;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        hidden = !visible;
        if (body != null) body.enabled = visible;
        if (hitbox != null) hitbox.enabled = visible;
        if (alert != null) alert.SetActive(false);
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = visible;
    }
}
