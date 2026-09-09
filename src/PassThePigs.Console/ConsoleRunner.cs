using PassThePigs.Core;
using PassThePigs.Core.Ai;
using Serilog;

namespace PassThePigs.Console;

/// <summary>Plays AI vs AI games to the log (Serilog), the way the old console app did.</summary>
internal static class ConsoleRunner
{
    public static void Run(int p1Ai, int p2Ai, int games, bool verbose, int? seed)
    {
        GameLog.SetVerbose(verbose);

        IRollStrategy s1 = Strategies.ById(p1Ai);
        IRollStrategy s2 = Strategies.ById(p2Ai);
        var rng = seed is { } sd ? new Random(sd) : new Random();

        Log.Information("AI vs AI — {P1} vs {P2}, {Games} game(s)", s1.Name, s2.Name, games);

        int p1Wins = 0, p2Wins = 0;
        for (int i = 1; i <= games; i++)
        {
            var game = new PigGame($"{s1.Name} (P1)", $"{s2.Name} (P2)", winScore: 100, rng);
            Wire(game);
            Log.Information("=== Game {N} ===", i);

            while (!game.IsOver)
            {
                IRollStrategy s = game.ActiveIndex == 0 ? s1 : s2;
                var actor = game.Active;
                game.PlayTurn(s, d => GameLog.Ai.Debug(
                    "{Player} {Decision} — {Reason}", actor.Name, d.Roll ? "rolls" : "holds", d.Reason));
            }

            if (game.WinnerIndex == 0) p1Wins++; else p2Wins++;
            Log.Information("Game {N} over — {Winner} wins ({T} turns; {P1} {P1S}, {P2} {P2S})",
                i, game.Winner!.Name, game.TurnNumber,
                game.Players[0].Name, game.Players[0].TotalScore,
                game.Players[1].Name, game.Players[1].TotalScore);
        }

        Log.Information("Session complete — {P1} {P1W}, {P2} {P2W}", s1.Name, p1Wins, s2.Name, p2Wins);
    }

    private static void Wire(PigGame game)
    {
        game.Rolled += (p, r) => Log.Debug("{Player} rolls {A} + {B} = {Score}", p.Name, r.One, r.Two, r.Score);
        game.PiggedOut += p => Log.Information("{Player} pigs out", p.Name);
        game.Passed += p => Log.Information("{Player} holds on {TurnScore}", p.Name, p.TurnScore);
    }
}
