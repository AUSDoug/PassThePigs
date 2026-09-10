namespace PassThePigs.Core.Ai;

/// <summary>Builds strategies by the ids used across the apps and the old Settings.ini.</summary>
public static class Strategies
{
    public const int Basic = 0;
    public const int Random = 1;
    public const int Aggressive = 2;
    public const int Expert = 3;
    public const int Ev = 4;

    /// <summary>Display names in id order (for drop-downs).</summary>
    public static readonly string[] Names = { "Basic", "Random", "Aggressive", "Expert", "EV (stop at 23)" };

    public static IRollStrategy ById(int id, bool exactWin = false)
    {
        IRollStrategy s = id switch
        {
            Random => new RandomStrategy(),
            Aggressive => new AggressiveStrategy(),
            Expert => new ExpertStrategy(),
            Ev => new EvStrategy(),
            _ => new BasicStrategy(),
        };
        return exactWin ? new ExactFinishStrategy(s) : s;
    }

    /// <summary>
    /// Parses a benchmark spec: "basic" | "random" | "aggressive" | "ev" | "expert"
    /// | "expert:&lt;n&gt;" (Expert with an explicit stop threshold).
    /// </summary>
    public static IRollStrategy Parse(string spec)
    {
        string name = spec;
        int? arg = null;
        int colon = spec.IndexOf(':');
        if (colon >= 0)
        {
            name = spec[..colon];
            arg = int.Parse(spec[(colon + 1)..].Split(',')[0]);
        }

        return name switch
        {
            "random" => new RandomStrategy(),
            "aggressive" => new AggressiveStrategy(),
            "ev" => new EvStrategy(),
            "expert" => new ExpertStrategy(arg ?? 23),
            _ => new BasicStrategy(),
        };
    }
}
