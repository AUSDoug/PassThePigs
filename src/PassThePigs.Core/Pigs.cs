namespace PassThePigs.Core;

/// <summary>
/// Tosses the pigs and scores the result. The landing probabilities and the
/// (order-sensitive) scoring table are taken from the physical Hasbro game, as
/// reproduced in Gorman's "Analytics, Pedagogy and the Pass the Pigs Game".
/// </summary>
public static class Pigs
{
    // Relative weight of each PigPosition, in enum order, out of 10000.
    private static readonly int[] Weights = { 3490, 3020, 2240, 880, 300, 70 };
    private const int WeightTotal = 10000;

    // Score for [first pig, second pig]. Order matters: Razorback+Snouter is 10
    // but Snouter+Razorback is 15 - a quirk carried over from the source table.
    // A "mixed sider" (one dot, one no-dot) is a pig out (0).
    private static readonly int[,] Scores =
    {
        //            SND  SD  RZ  TR  SN  JW
        /* SND */ {   1,   0,  5,  5, 10, 15 },
        /* SD  */ {   0,   1,  5,  5, 10, 15 },
        /* RZ  */ {   5,   5, 20, 10, 10, 20 },
        /* TR  */ {   5,   5, 10, 20, 15, 20 },
        /* SN  */ {  10,  10, 15, 15, 40, 25 },
        /* JW  */ {  15,  15, 20, 20, 25, 60 },
    };

    /// <summary>Score for a specific pair of pig positions (0 = pig out).</summary>
    public static int Score(PigPosition one, PigPosition two) => Scores[(int)one, (int)two];

    /// <summary>Samples one pig position from the landing distribution.</summary>
    public static PigPosition SamplePosition(Random rng)
    {
        int r = rng.Next(WeightTotal);
        int acc = 0;
        for (int i = 0; i < Weights.Length; i++)
        {
            acc += Weights[i];
            if (r < acc) return (PigPosition)i;
        }
        return PigPosition.LeaningJowler;
    }

    /// <summary>Tosses both pigs and returns the scored outcome.</summary>
    public static PigRoll Roll(Random rng)
    {
        PigPosition one = SamplePosition(rng);
        PigPosition two = SamplePosition(rng);
        return new PigRoll(one, two, Score(one, two));
    }
}
