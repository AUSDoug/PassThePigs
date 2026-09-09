namespace PassThePigs.Core.Ai;

/// <summary>Coin flip each roll. Checks the obvious "already won" case first.</summary>
public sealed class RandomStrategy : IRollStrategy
{
    public string Name => "random";

    public RollDecision Decide(in GameView v, Random rng)
    {
        if (v.MyTotal + v.MyTurn >= v.WinScore)
            return RollDecision.Hold("already won");

        int x = rng.Next(0, 1000);
        return x % 2 != 0
            ? RollDecision.Hold($"random draw {x} is odd")
            : RollDecision.DoRoll($"random draw {x} is even");
    }
}
