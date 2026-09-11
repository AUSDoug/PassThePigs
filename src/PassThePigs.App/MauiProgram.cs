using Microsoft.Extensions.Logging;
using PassThePigs.App.Pages;
using PassThePigs.App.ViewModels;

namespace PassThePigs.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<SetupViewModel>();
        builder.Services.AddTransient<SetupPage>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<StatsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
