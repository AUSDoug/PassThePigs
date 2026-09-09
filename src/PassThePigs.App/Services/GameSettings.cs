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

    public static bool HumanStarts
    {
        get => Preferences.Get(nameof(HumanStarts), true);
        set => Preferences.Set(nameof(HumanStarts), value);
    }
}
