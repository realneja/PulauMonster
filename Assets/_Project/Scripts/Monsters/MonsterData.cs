// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Data statis satu spesies (aset Mon_*). Field private + property read-only: class lain hanya membaca.
[CreateAssetMenu(fileName = "Mon_", menuName = "Pulau Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private string id;            // "MON_001"
    [SerializeField] private string displayName;
    [SerializeField] private Element element;
    [SerializeField] private Sprite[] frames;      // Idle0, Idle1, Attack, Hurt
    [SerializeField] private int baseHP, baseATK, baseDEF, baseSPD;
    [SerializeField] private string skillName = "Semburan Api";
    [SerializeField] private MonsterData evolvesTo;
    [SerializeField] private int evolveLevel;

    public string Id => id;
    public string DisplayName => displayName;
    public Element Element => element;
    public Sprite[] Frames => frames;
    public int BaseHP => baseHP;
    public int BaseATK => baseATK;
    public int BaseDEF => baseDEF;
    public int BaseSPD => baseSPD;
    public string SkillName => skillName;
    public MonsterData EvolvesTo => evolvesTo;
    public int EvolveLevel => evolveLevel;

    public Sprite IdleFrame(int index) =>
        frames != null && frames.Length > 0 ? frames[Mathf.Clamp(index, 0, frames.Length - 1)] : null;

#if UNITY_EDITOR
    // Dipakai DemoBuilder untuk mengisi aset dari MonsterDatabase.csv.
    public void EditorSet(string id, string displayName, Element element, Sprite[] frames,
        int hp, int atk, int def, int spd, string skillName, MonsterData evolvesTo, int evolveLevel)
    {
        this.id = id; this.displayName = displayName; this.element = element; this.frames = frames;
        baseHP = hp; baseATK = atk; baseDEF = def; baseSPD = spd;
        this.skillName = skillName; this.evolvesTo = evolvesTo; this.evolveLevel = evolveLevel;
    }
#endif
}
