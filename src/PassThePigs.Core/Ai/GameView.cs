namespace PassThePigs.Core.Ai;

/// <summary>
/// Everything a roll strategy is allowed to see: the deciding player's totals and
/// the opponent's total. (Gorman's point: past and future rolls are irrelevant -
/// only the accumulated points matter.)
/// </summary>
/// <param name="MyTotal">The deciding player's banked score.</param>
/// <param name="MyTurn">Points accumulated this turn, at risk.</param>
/// <param name="OpponentTotal">The opponent's banked score.</param>
/// <param name="WinScore">Total needed to win.</param>
public readonly record struct GameView(int MyTotal, int MyTurn, int OpponentTotal, int WinScore);
