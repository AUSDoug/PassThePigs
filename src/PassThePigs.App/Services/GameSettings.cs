using PassThePigs.Core.Ai;

namespace PassThePigs.App.Services;

/// <summary>Persistent game options, backed by MAUI <see cref="Preferences"/>.</summary>
public static class GameSettings
{
    public static int OpponentAi
    {
        get => Preferences.Get(nameof(OpponentAi), Strategies.Basic);
        set => Preferences.Set(nameof(OpponentAi), value);
    }

    public static int WinScore
    {
        get => Preferences.Get(nameof(WinScore), 100);
        set => Preferences.Set(nameof(WinScore), value);
    }

    /// <summary>Who opens a new game.</summary>
    public static FirstTurn FirstTurn
    {
        get => (FirstTurn)Preferences.Get(nameof(FirstTurn), (int)Services.FirstTurn.Random);
        set => Preferences.Set(nameof(FirstTurn), (int)value);
    }

    /// <summary>Require the target score to be hit exactly (an overshooting turn is forfeited).</summary>
    public static bool ExactWin
    {
        get => Preferences.Get(nameof(ExactWin), false);
        set => Preferences.Set(nameof(ExactWin), value);
    }

    public const int DefaultAiRollDelayMs = 700;

    /// <summary>Pause between the opponent's rolls, in ms. 0 also skips the 3D render on its turn.</summary>
    public static int AiRollDelayMs
    {
        get => Preferences.Get(nameof(AiRollDelayMs), DefaultAiRollDelayMs);
        set => Preferences.Set(nameof(AiRollDelayMs), value);
    }

    /// <summary>
    /// UI theme override. <see cref="AppTheme.Unspecified"/> means "follow the phone".
    /// </summary>
    public static AppTheme Theme
    {
        get => (AppTheme)Preferences.Get(nameof(Theme), (int)AppTheme.Unspecified);
        set => Preferences.Set(nameof(Theme), (int)value);
    }

    /// <summary>Push the saved <see cref="Theme"/> onto the running app.</summary>
    public static void ApplyTheme()
    {
        if (Application.Current is { } app)
            app.UserAppTheme = Theme;
    }
}
