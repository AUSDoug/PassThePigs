namespace PassThePigs.Core;

/// <summary>A player's running score. <see cref="TurnScore"/> is at risk until banked.</summary>
public sealed class PlayerState(string name)
{
    public string Name { get; } = name;
    public int TotalScore { get; internal set; }
    public int TurnScore { get; internal set; }

    /// <summary>What the player would have if they passed right now.</summary>
    public int ProjectedTotal => TotalScore + TurnScore;

    internal void Reset()
    {
        TotalScore = 0;
        TurnScore = 0;
    }
}
