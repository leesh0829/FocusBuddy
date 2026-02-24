using System.IO;
using System.Threading;
using System.Windows;
using FocusBuddy.Data;
using FocusBuddy.Services;
using FocusBuddy.ViewModels;
using FocusBuddy.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FocusBuddy;

public partial class App : Wpf.Application
{
    private const string SingleInstanceMutexName = "Global\\FocusBuddy.SingleInstance";

    private ServiceProvider? _serviceProvider;
    private Mutex? _singleInstanceMutex;

    protected override async void OnStartup(Wpf.StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;

            MessageBox.Show(
                "FocusBuddy is already running.",
                "FocusBuddy",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        try
        {
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
        catch (Exception ex)
        {
            Log.Error(ex, "Application startup failed");
            Wpf.MessageBox.Show($"앱 시작 중 오류가 발생했습니다.\n{ex.Message}", "FocusBuddy", Wpf.MessageBoxButton.OK, Wpf.MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(Wpf.ExitEventArgs e)
    {
        if (_serviceProvider is not null)
        {
            var trackingService = _serviceProvider.GetRequiredService<WindowTrackingService>();
            var trayService = _serviceProvider.GetRequiredService<TrayService>();

            await trackingService.StopAsync();
            trayService.Dispose();

            await _serviceProvider.DisposeAsync();
        }

        if (_singleInstanceMutex is not null)
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
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
