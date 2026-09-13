namespace PassThePigs.App.Services;

/// <summary>
/// One human roll-or-pass decision, logged for the Statistics page's "play style"
/// section. State is captured immediately before the decision - the same shape
/// <see cref="PassThePigs.Core.Ai.GameView"/> gives a strategy's Decide() call.
/// </summary>
public sealed class DecisionRecord
{
    /// <summary>Links back to the <see cref="GameRecord"/> this decision belongs to.</summary>
    public Guid GameId { get; set; }

    public int MyTotal { get; set; }
    public int MyTurn { get; set; }
    public int OpponentTotal { get; set; }

    /// <summary>True = rolled, false = passed.</summary>
    public bool Rolled { get; set; }

    /// <summary>Only meaningful when <see cref="Rolled"/> is true.</summary>
    public bool PigOut { get; set; }
}
