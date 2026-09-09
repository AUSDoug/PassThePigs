using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;

namespace PassThePigs.Console;

/// <summary>
/// Serilog setup for the console head. Two channels:
///  - default: game flow, to the console and logs/passthepigs-*.log
///  - "AI" channel (<see cref="Ai"/>): the strategies' reasoning, to
///    logs/ai-commentary-*.log always, and the console only when verbose.
/// </summary>
internal static class GameLog
{
    private const string ChannelProperty = "Channel";
    private const string AiChannel = "AI";
    private const string ConsoleTemplate = "{Message:lj}{NewLine}{Exception}";
    private const string FileTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    private static readonly LoggingLevelSwitch MainLevel = new(LogEventLevel.Information);

    public static ILogger Ai { get; private set; } = Logger.None;

    public static void Configure(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        try { System.Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* no console */ }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Logger(flow => flow
                .MinimumLevel.ControlledBy(MainLevel)
                .Filter.ByExcluding(Matching.WithProperty<string>(ChannelProperty, c => c == AiChannel))
                .WriteTo.Console(outputTemplate: ConsoleTemplate)
                .WriteTo.File(Path.Combine(logDirectory, "passthepigs-.log"),
                    rollingInterval: RollingInterval.Day, outputTemplate: FileTemplate))
            .WriteTo.Logger(reasoning => reasoning
                .Filter.ByIncludingOnly(Matching.WithProperty<string>(ChannelProperty, c => c == AiChannel))
                .WriteTo.File(Path.Combine(logDirectory, "ai-commentary-.log"),
                    rollingInterval: RollingInterval.Day, outputTemplate: FileTemplate)
                .WriteTo.Console(outputTemplate: ConsoleTemplate, levelSwitch: MainLevel))
            .CreateLogger();

        Ai = Log.ForContext(ChannelProperty, AiChannel);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown();
    }

    public static void SetVerbose(bool verbose) =>
        MainLevel.MinimumLevel = verbose ? LogEventLevel.Debug : LogEventLevel.Information;

    public static void Shutdown() => Log.CloseAndFlush();
}
