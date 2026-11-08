// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam

// Segitiga elemen plus Volt: Pyro > Sprout, Aqua > Pyro, Sprout > Aqua, Volt > Aqua. Kebalikannya 0,75.
public static class ElementChart
{
    public static float Multiplier(Element attacker, Element defender)
    {
        if (StrongAgainst(attacker) == defender) return 1.5f;
        if (StrongAgainst(defender) == attacker) return 0.75f;
        return 1f;
    }

    public static Element StrongAgainst(Element e)
    {
        switch (e)
        {
            case Element.Pyro: return Element.Sprout;
            case Element.Aqua: return Element.Pyro;
            case Element.Sprout: return Element.Aqua;
            default: return Element.Aqua; // Volt
        }
    }
}
