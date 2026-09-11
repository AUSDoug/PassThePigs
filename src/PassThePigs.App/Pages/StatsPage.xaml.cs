using PassThePigs.App.ViewModels;

namespace PassThePigs.App.Pages;

public partial class StatsPage : ContentPage
{
    public StatsPage(StatsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
