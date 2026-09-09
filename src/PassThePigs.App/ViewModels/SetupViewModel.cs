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
    [ObservableProperty] private bool _humanStarts = GameSettings.HumanStarts;

    [RelayCommand]
    private async Task PlayAsync()
    {
        int id = Math.Max(0, Array.IndexOf(Opponents, SelectedOpponent));
        GameSettings.OpponentAi = id;
        GameSettings.HumanStarts = HumanStarts;
        await Shell.Current.GoToAsync(nameof(GamePage));
    }

    private static string NameFor(int id) =>
        Strategies.Names[Math.Clamp(id, 0, Strategies.Names.Length - 1)];
}
