// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam

public enum GrowthType
{
    LevelUp,
    Evolve
}

// Satu kejadian pertumbuhan hasil GainExp: naik level atau evolusi, lengkap dengan pesannya.
public class GrowthEvent
{
    public GrowthType Type { get; }
    public MonsterData From { get; }
    public MonsterData To { get; }
    public int Level { get; }
    public string Message { get; }

    public GrowthEvent(GrowthType type, MonsterData from, MonsterData to, int level, string message)
    {
        Type = type;
        From = from;
        To = to;
        Level = level;
        Message = message;
    }
}
