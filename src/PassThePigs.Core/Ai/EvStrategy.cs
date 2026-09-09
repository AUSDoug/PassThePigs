namespace PassThePigs.Core.Ai;

/// <summary>
/// The pure expected-value heuristic from Gorman's paper, with nothing bolted on:
/// roll until the turn score reaches 23, then stop. Ignores the opponent entirely.
/// The baseline the Expert strategy builds on.
/// </summary>
public sealed class EvStrategy : IRollStrategy
{
    private const int Target = 23; // 0.21 * 23 > 4.7  ->  stop

    public string Name => "ev";

    public RollDecision Decide(in GameView v, Random rng)
    {
        if (v.MyTurn < 1)
            return RollDecision.DoRoll("first roll of the turn, nothing at stake");

        if (v.MyTotal + v.MyTurn >= v.WinScore)
            return RollDecision.Hold("this turn wins the game");

        return v.MyTurn >= Target
            ? RollDecision.Hold($"reached the stop-at-{Target} threshold")
            : RollDecision.DoRoll($"turn score {v.MyTurn} still below {Target}");
    }
}
