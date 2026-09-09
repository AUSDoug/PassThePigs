using PassThePigs.App.ViewModels;

namespace PassThePigs.App.Pages;

public partial class SetupPage : ContentPage
{
    public SetupPage(SetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
