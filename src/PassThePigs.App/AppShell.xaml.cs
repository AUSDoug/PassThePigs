using PassThePigs.App.Pages;

namespace PassThePigs.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
    }
}
