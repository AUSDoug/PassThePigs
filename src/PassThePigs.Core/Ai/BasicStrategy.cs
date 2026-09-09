namespace PassThePigs.Core.Ai;

/// <summary>
/// Well-rounded and consistent. A pile of situational checks that falls back on
/// Gorman's "stop at 23" rule. The thresholds assume a 100-point game.
/// </summary>
public sealed class BasicStrategy : IRollStrategy
{
    public string Name => "basic";

    public RollDecision Decide(in GameView v, Random rng)
    {
        int myTotal = v.MyTotal, myTurn = v.MyTurn, oppTotal = v.OpponentTotal;

        if (myTurn < 1)
            return RollDecision.DoRoll("haven't rolled yet this turn");

        if (myTurn + myTotal > 90 && myTurn <= 30)
            return RollDecision.DoRoll("about to win and not pushing my luck");

        if (oppTotal >= 90 && myTotal + myTurn <= 90)
            return RollDecision.DoRoll("opponent is closing in on a win");

        if (myTurn > 0 && myTotal >= 90 && oppTotal <= 50)
            return RollDecision.Hold("not greedy; opponent far behind and I'm nearly home");

        if (myTurn >= 60)
            return RollDecision.Hold("not pushing my luck after 60+ this turn");

        if (myTurn > 29 && oppTotal < 76 && myTotal < 50)
            return RollDecision.Hold("had a good run, opponent isn't too far ahead");

        if (myTotal == 0 && myTurn > 14)
            return RollDecision.Hold("want to get off the mark");

        if (oppTotal - myTotal >= myTurn + 25)
            return RollDecision.DoRoll("passing now leaves opponent 25+ ahead");

        if (myTotal + myTurn > oppTotal && myTotal > 0 && oppTotal > 0 && myTurn > 0)
            return RollDecision.Hold("other reasons exhausted; passing leaves me ahead");

        return myTurn > 23
            ? RollDecision.Hold("other reasons exhausted; reached 23")
            : RollDecision.DoRoll("other reasons exhausted; not yet at 23");
    }
}
