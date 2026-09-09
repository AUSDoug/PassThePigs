namespace PassThePigs.Core.Ai;

/// <summary>Loves to roll. Banks at 50 in a turn unless the opponent is winning big.</summary>
public sealed class AggressiveStrategy : IRollStrategy
{
    public string Name => "aggressive";

    public RollDecision Decide(in GameView v, Random rng)
    {
        if (v.MyTotal + v.MyTurn >= v.WinScore)
            return RollDecision.Hold("already won");

        if (v.MyTurn >= 50 && v.OpponentTotal <= 85)
            return RollDecision.Hold("50 points is enough in one turn");

        return RollDecision.DoRoll("I'm aggressive");
    }
}
