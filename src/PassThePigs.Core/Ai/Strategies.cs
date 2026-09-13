namespace PassThePigs.Core.Ai;

/// <summary>Builds strategies by the ids used across the apps and the old Settings.ini.</summary>
public static class Strategies
{
    public const int Basic = 0;
    public const int Random = 1;
    public const int Aggressive = 2;
    public const int Expert = 3;
    public const int Ev = 4;

    /// <summary>Display names in id order (for drop-downs). Index matches the id constants above.</summary>
    public static readonly string[] Names = { "Basic", "Random Rolls", "Aggressive", "Expert", "Optimal" };

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
    /// | "expert:&lt;BaseTarget&gt;" | "expert:&lt;BaseTarget&gt;,&lt;EndgameZone&gt;".
    /// </summary>
    public static IRollStrategy Parse(string spec)
    {
        string name = spec;
        string? arg = null;
        int colon = spec.IndexOf(':');
        if (colon >= 0)
        {
            name = spec[..colon];
            arg = spec[(colon + 1)..];
        }

        if (name != "expert")
        {
            return name switch
            {
                "random" => new RandomStrategy(),
                "aggressive" => new AggressiveStrategy(),
                "ev" => new EvStrategy(),
                _ => new BasicStrategy(),
            };
        }

        if (arg is null) return new ExpertStrategy();

        // "23" or "23,68" (BaseTarget,EndgameZone); fields beyond the second are
        // ignored, tolerating the console's older "23,20,20,35,16" spec too.
        string[] fields = arg.Split(',');
        int baseTarget = int.Parse(fields[0]);
        int endgameZone = fields.Length > 1 && int.TryParse(fields[1], out int ez) ? ez : 68;
        return new ExpertStrategy(baseTarget, endgameZone);
    }
}
