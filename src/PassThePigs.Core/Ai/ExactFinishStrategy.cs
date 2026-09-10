namespace PassThePigs.Core.Ai;

/// <summary>
/// Adapts any strategy for the "exact score to win" variant. Near the finish it
/// overrides the inner strategy: it keeps rolling while an exact finish is still
/// within reach, and stops the moment the projected total reaches or passes the
/// target - a turn that would overshoot can't win, so there is nothing to gain by
/// rolling on.
/// </summary>
public sealed class ExactFinishStrategy(IRollStrategy inner) : IRollStrategy
{
    /// <summary>Within this many points of an exact finish, chase it.</summary>
    private const int Reach = 13;

    public string Name => inner.Name;

    public RollDecision Decide(in GameView v, Random rng)
    {
        if (v.MyTurn >= 1)
        {
            int projected = v.MyTotal + v.MyTurn;
            if (projected >= v.WinScore)
                return RollDecision.Hold(projected == v.WinScore
                    ? "landed exactly on the target"
                    : "would overshoot the exact target");
            if (v.WinScore - projected <= Reach)
                return RollDecision.DoRoll("within reach of an exact finish");
        }

        return inner.Decide(v, rng);
    }
}
