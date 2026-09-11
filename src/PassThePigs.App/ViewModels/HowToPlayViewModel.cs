using CommunityToolkit.Mvvm.Input;

namespace PassThePigs.App.ViewModels;

/// <summary>Static rules content - no state, just the Done navigation.</summary>
public partial class HowToPlayViewModel
{
    [RelayCommand]
    private static Task DoneAsync() => Shell.Current.GoToAsync("..");
}
