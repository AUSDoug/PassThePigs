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
public sealed class ExpertStrategy(int baseTarget = 23, int endgameZone = 68) : IRollStrategy
{
    /// <summary>Accumulated-turn-points target at which to stop rolling (~23 is the EV break-even).</summary>
    public int BaseTarget { get; } = baseTarget;

    /// <summary>
    /// The "go for the win" / "gamble" threshold, as points-from-target in a 100-point
    /// game (the scale it was tuned at - see <see cref="Decide"/>). It used to be derived
    /// as WinScore-BaseTarget (=77 for BaseTarget 23); decoupling it and sweeping
    /// independently via headless self-play (PassThePigsConsole, 300k games) found a real
    /// ~0.7pp win-rate gain around 68, flat across roughly 58-74 and falling off outside
    /// that. BaseTarget was re-checked against the new value and 23 is still best - no
    /// interaction between the two. See tools/hillclimb.py.
    /// </summary>
    public int EndgameZone { get; } = endgameZone;

    public string Name => "expert";

    public RollDecision Decide(in GameView v, Random rng)
    {
        // EndgameZone was tuned against a 100-point game (the only size the console
        // harness plays); this app lets the target score vary, so scale it the same
        // way the old derived threshold implicitly did.
        int endgameZone = (int)Math.Round(EndgameZone * v.WinScore / 100.0);

        if (v.MyTurn < 1)
            return RollDecision.DoRoll("first roll of the turn, nothing at stake");

        if (v.MyTotal + v.MyTurn >= v.WinScore)
            return RollDecision.Hold("this turn wins the game");

        if (v.MyTotal >= endgameZone)
            return RollDecision.DoRoll("within reach of victory, rolling for the win");

        if (v.OpponentTotal >= endgameZone)
            return RollDecision.DoRoll("opponent is closing in, gambling for the win");

        return v.MyTurn >= BaseTarget
            ? RollDecision.Hold($"reached the stop-at-{BaseTarget} threshold")
            : RollDecision.DoRoll($"turn score {v.MyTurn} below {BaseTarget}");
    }
}
