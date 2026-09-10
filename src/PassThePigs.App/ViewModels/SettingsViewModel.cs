using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassThePigs.App.Services;

namespace PassThePigs.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    // Index order matches AppTheme: Unspecified = 0, Light = 1, Dark = 2.
    public string[] Themes { get; } = { "System", "Light", "Dark" };
    public int[] TargetScores { get; } = { 50, 75, 100, 150, 200 };
    // Index order matches the FirstTurn enum: Human = 0, Ai = 1, Random = 2.
    public string[] FirstTurnOptions { get; } = { "Human", "AI", "Random" };

    [ObservableProperty] private string _selectedTheme;
    [ObservableProperty] private int _selectedTargetScore;
    [ObservableProperty] private string _selectedFirstTurn;
    [ObservableProperty] private bool _exactWin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AiDelayLabel))]
    private double _aiRollDelay;

    public string AiDelayLabel => Snap(AiRollDelay) <= 0
        ? "Instant - no pig graphics on the opponent's turn"
        : $"{Snap(AiRollDelay)} ms between the opponent's rolls";

    public SettingsViewModel()
    {
        _selectedTheme = Themes[Math.Clamp((int)GameSettings.Theme, 0, Themes.Length - 1)];
        _selectedTargetScore = GameSettings.WinScore;
        _selectedFirstTurn = FirstTurnOptions[Math.Clamp((int)GameSettings.FirstTurn, 0, FirstTurnOptions.Length - 1)];
        _exactWin = GameSettings.ExactWin;
        _aiRollDelay = GameSettings.AiRollDelayMs;
    }

    partial void OnSelectedThemeChanged(string value)
    {
        GameSettings.Theme = (AppTheme)Math.Max(0, Array.IndexOf(Themes, value));
        GameSettings.ApplyTheme();
    }

    partial void OnSelectedTargetScoreChanged(int value) => GameSettings.WinScore = value;

    partial void OnSelectedFirstTurnChanged(string value) =>
        GameSettings.FirstTurn = (FirstTurn)Math.Max(0, Array.IndexOf(FirstTurnOptions, value));

    partial void OnExactWinChanged(bool value) => GameSettings.ExactWin = value;

    partial void OnAiRollDelayChanged(double value) => GameSettings.AiRollDelayMs = Snap(value);

    // The slider is continuous; store and display it rounded to a tidy step.
    private static int Snap(double ms) => (int)(Math.Round(ms / 50.0) * 50);

    [RelayCommand]
    private static Task DoneAsync() => Shell.Current.GoToAsync("..");
}
