namespace PassThePigs.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		Services.GameSettings.ApplyTheme();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}