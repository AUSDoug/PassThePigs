using PassThePigs.Core;
using PassThePigs.Core.Ai;

namespace PassThePigs.Console;

/// <summary>
/// Headless match runner for tuning the AI, driven from tools/hillclimb.py.
///
///   PassThePigs.Console bench games=&lt;n&gt; seed=&lt;n&gt; p1=&lt;spec&gt; p2=&lt;spec&gt;
///
/// &lt;spec&gt; is basic | random | aggressive | ev | expert | expert:&lt;n&gt; | expert:&lt;n&gt;,&lt;n&gt;.
/// The first turn alternates between the two sides so first-mover advantage is
/// split evenly. Prints one line:
///
///   RESULT p1=&lt;spec&gt; p2=&lt;spec&gt; games=&lt;n&gt; p1wins=&lt;n&gt; p2wins=&lt;n&gt; p1rate=&lt;f&gt; se=&lt;f&gt;
/// </summary>
internal static class Benchmark
{
    private const int TurnSafetyCap = 5000; // guards against two 'random' strategies stalling

    public static void Run(string[] args)
    {
        int games = 20000, seed = 1;
        string p1Spec = "expert", p2Spec = "expert";

        foreach (string a in args)
        {
            int eq = a.IndexOf('=');
            if (eq < 0) continue;
            string key = a[..eq], val = a[(eq + 1)..];
            switch (key)
            {
                case "games": games = int.Parse(val); break;
                case "seed": seed = int.Parse(val); break;
                case "p1": p1Spec = val; break;
                case "p2": p2Spec = val; break;
            }
        }

        IRollStrategy s1 = Strategies.Parse(p1Spec);
        IRollStrategy s2 = Strategies.Parse(p2Spec);
        var rng = new Random(seed);

        int p1Wins = 0;
        for (int g = 0; g < games; g++)
        {
            if (PlayGame(s1, s2, startWithP1: g % 2 == 0, rng)) p1Wins++;
        }

        int p2Wins = games - p1Wins;
        double rate = (double)p1Wins / games;
        double se = Math.Sqrt(rate * (1.0 - rate) / games);

        System.Console.WriteLine(
            $"RESULT p1={p1Spec} p2={p2Spec} games={games} " +
            $"p1wins={p1Wins} p2wins={p2Wins} p1rate={rate:F4} se={se:F4}");
    }

    private static bool PlayGame(IRollStrategy s1, IRollStrategy s2, bool startWithP1, Random rng)
    {
        var game = new PigGame("P1", "P2", winScore: 100, rng, startingPlayer: startWithP1 ? 0 : 1);

        for (int turn = 0; turn < TurnSafetyCap && !game.IsOver; turn++)
        {
            game.PlayTurn(game.ActiveIndex == 0 ? s1 : s2);
        }

        return game.WinnerIndex == 0;
    }
}
