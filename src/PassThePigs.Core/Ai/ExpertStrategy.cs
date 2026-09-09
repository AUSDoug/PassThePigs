namespace PassThePigs.Core.Ai;

/// <summary>
/// Gorman's "stop at 23" expected-value rule plus the endgame awareness he sketches:
/// each roll risks a ~21% pig out for a constant ~4.7 expected points, so rolling is
/// worth it while turn score is below ~23. Near the end of the game it instead plays
/// the probability of winning - pressing on when victory is in range, and gambling
/// for the win when the opponent is about to close the game out.
///
/// (An earlier version also slid the target up/down by relative score. Benchmarking
/// showed that layer was a net liability against the other rulesets, so it was removed;
/// the endgame overrides carried the whole benefit.)
/// </summary>
public sealed class ExpertStrategy(int baseTarget = 23) : IRollStrategy
{
    /// <summary>Accumulated-turn-points target at which to stop rolling (~23 is the EV break-even).</summary>
    public int BaseTarget { get; } = baseTarget;

    public string Name => "expert";

    public RollDecision Decide(in GameView v, Random rng)
    {
        int endgameZone = v.WinScore - BaseTarget; // ~one good turn from home

        if (v.MyTurn < 1)
            return RollDecision.DoRoll("first roll of the turn, nothing at stake");

        if (v.MyTotal + v.MyTurn >= v.WinScore)
            return RollDecision.Hold("this turn wins the game");

        if (v.MyTotal >= endgameZone)
            return RollDecision.DoRoll("within one turn of victory, rolling for the win");

        if (v.OpponentTotal >= endgameZone)
            return RollDecision.DoRoll("opponent is one turn from winning, gambling for the win");

        return v.MyTurn >= BaseTarget
            ? RollDecision.Hold($"reached the stop-at-{BaseTarget} threshold")
            : RollDecision.DoRoll($"turn score {v.MyTurn} below {BaseTarget}");
    }
}
