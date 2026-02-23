using System.IO;
using System.Windows;
using FocusBuddy.Data;
using FocusBuddy.Services;
using FocusBuddy.ViewModels;
using FocusBuddy.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FocusBuddy;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ConfigureLogging();
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        await databaseService.InitializeAsync();

        var trackingService = _serviceProvider.GetRequiredService<WindowTrackingService>();
        var trayService = _serviceProvider.GetRequiredService<TrayService>();

        await trackingService.StartAsync();
        trayService.Initialize();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is not null)
        {
            var trackingService = _serviceProvider.GetRequiredService<WindowTrackingService>();
            var trayService = _serviceProvider.GetRequiredService<TrayService>();

            await trackingService.StopAsync();
            trayService.Dispose();

            await _serviceProvider.DisposeAsync();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static void ConfigureLogging()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusBuddy");
        Directory.CreateDirectory(appData);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(appData, "focusbuddy.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SettingsService>();
        services.AddSingleton<CategoryService>();
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<WindowTrackingService>();
        services.AddSingleton<FocusModeService>();
        services.AddSingleton<TrayService>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }
}
