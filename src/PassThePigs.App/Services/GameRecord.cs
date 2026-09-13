namespace PassThePigs.App.Services;

/// <summary>One finished game, logged for the Statistics page. Plain mutable class so
/// System.Text.Json can round-trip it without a custom constructor.</summary>
public sealed class GameRecord
{
    /// <summary>Links to the <see cref="DecisionRecord"/>s logged during this game.</summary>
    public Guid GameId { get; set; }

    public DateTime PlayedAtUtc { get; set; }

    /// <summary>The opponent actually played (the drawn AI, even under "Random").</summary>
    public int OpponentId { get; set; }

    public bool Won { get; set; }
    public int YourScore { get; set; }
    public int OpponentScore { get; set; }
    public int TargetScore { get; set; }
    public bool ExactWin { get; set; }

    /// <summary>Rounds the game took (both players get a turn per round).</summary>
    public int Rounds { get; set; }
}
