using PassThePigs.App.ViewModels;

namespace PassThePigs.App.Pages;

public partial class GamePage : ContentPage
{
    private readonly GameViewModel _vm;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Start();
    }
}
