using PassThePigs.App.ViewModels;

namespace PassThePigs.App.Pages;

public partial class HowToPlayPage : ContentPage
{
    public HowToPlayPage(HowToPlayViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
