using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassThePigs.App.Pages;
using PassThePigs.App.Services;
using PassThePigs.Core.Ai;

namespace PassThePigs.App.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    public string[] Opponents { get; } = Strategies.Names;

    // Bind the Picker's SelectedItem (string), not SelectedIndex - the index binding
    // has a first-load race with ItemsSource that can select the wrong row.
    [ObservableProperty] private string _selectedOpponent = NameFor(GameSettings.OpponentAi);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOpponentPicker))]
    private bool _randomOpponent = GameSettings.RandomOpponent;

    /// <summary>The picker is hidden while "Random" is on - the opponent is a surprise.</summary>
    public bool ShowOpponentPicker => !RandomOpponent;

    partial void OnRandomOpponentChanged(bool value) => GameSettings.RandomOpponent = value;

    [RelayCommand]
    private async Task PlayAsync()
    {
        if (!RandomOpponent)
        {
            int id = Math.Max(0, Array.IndexOf(Opponents, SelectedOpponent));
            GameSettings.OpponentAi = id;
        }
        await Shell.Current.GoToAsync(nameof(GamePage));
    }

    [RelayCommand]
    private static Task OpenSettingsAsync() => Shell.Current.GoToAsync(nameof(SettingsPage));

    [RelayCommand]
    private static Task OpenStatsAsync() => Shell.Current.GoToAsync(nameof(StatsPage));

    private static string NameFor(int id) =>
        Strategies.Names[Math.Clamp(id, 0, Strategies.Names.Length - 1)];
}
