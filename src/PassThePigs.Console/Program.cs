using PassThePigs.Console;
using Serilog;

// Headless companion to the PassThePigs Android app: AI-vs-AI runs and the
// benchmark harness used to tune the rulesets.
//
//   PassThePigs.Console bench games=<n> seed=<n> p1=<spec> p2=<spec>
//   PassThePigs.Console play [p1=<id>] [p2=<id>] [games=<n>] [seed=<n>] [verbose]
//   PassThePigs.Console                  (reads Settings.ini next to the exe)

if (args.Length > 0 && args[0] == "bench")
{
    Benchmark.Run(args);           // no logging configured -> silent
    return;
}

string exeDir = AppContext.BaseDirectory;
GameLog.Configure(Path.Combine(exeDir, "logs"));
Log.Information("Session started {Time:u}", DateTime.Now);

var ini = IniSettings.Load(Path.Combine(exeDir, "Settings.ini"));

int p1Ai = ini.Cpu0Ai, p2Ai = ini.Cpu1Ai, games = ini.Games;
bool verbose = ini.Verbose;
int? seed = null;

foreach (string a in args)
{
    if (a == "play" || a == "verbose") { verbose |= a == "verbose"; continue; }
    int eq = a.IndexOf('=');
    if (eq < 0) continue;
    string k = a[..eq], v = a[(eq + 1)..];
    switch (k)
    {
        case "p1": p1Ai = int.Parse(v); break;
        case "p2": p2Ai = int.Parse(v); break;
        case "games": games = int.Parse(v); break;
        case "seed": seed = int.Parse(v); break;
        case "verbose": verbose = v is "1" or "true"; break;
    }
}

Log.Information("Settings — p1={P1}, p2={P2}, games={Games}, verbose={Verbose}", p1Ai, p2Ai, games, verbose);
ConsoleRunner.Run(p1Ai, p2Ai, games, verbose, seed);

Log.Information("Session ended {Time:u}", DateTime.Now);
GameLog.Shutdown();
