using System.Windows;
using WpfApplication = System.Windows.Application;
using FocusBuddy.Helpers;
using FocusBuddy.Models;

namespace FocusBuddy.Services;

public sealed class FocusModeService
{
    private readonly SettingsService _settingsService;
    private AppSettings _settings = new();

    public bool IsEnabled => _settings.FocusModeEnabled;

    public FocusModeService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _ = LoadSettingsAsync();
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        _settings.FocusModeEnabled = enabled;
        await _settingsService.SaveAsync(_settings);
    }

    public void HandleActiveWindow(IntPtr hwnd, string processName, string title)
    {
        if (!_settings.FocusModeEnabled)
        {
            return;
        }

        if (!_settings.FocusModeBlacklist.Contains(processName, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            System.Windows.MessageBox.Show(
                $"Focus reminder: {processName} is marked as distracting.\nWindow: {title}",
                "FocusBuddy Reminder",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        });

        if (_settings.AutoMinimizeDistractingApps)
        {
            _ = Win32Interop.ShowWindow(hwnd, Win32Interop.SW_MINIMIZE);
        }
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        _settings = await _settingsService.LoadAsync();
        return _settings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _settings = settings;
        await _settingsService.SaveAsync(_settings);
    }

    private async Task LoadSettingsAsync()
    {
        _settings = await _settingsService.LoadAsync();
    }
}
