using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassThePigs.App.Pages;
using PassThePigs.App.Services;
using PassThePigs.Core.Ai;

namespace PassThePigs.App.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    public string[] Opponents { get; } = Strategies.Names;

    [ObservableProperty] private int _selectedOpponentIndex = GameSettings.OpponentAi;
    [ObservableProperty] private bool _humanStarts = GameSettings.HumanStarts;

    [RelayCommand]
    private async Task PlayAsync()
    {
        GameSettings.OpponentAi = SelectedOpponentIndex;
        GameSettings.HumanStarts = HumanStarts;
        await Shell.Current.GoToAsync(nameof(GamePage));
    }
}
