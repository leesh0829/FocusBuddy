using System.Text.Json;
using System.IO;
using FocusBuddy.Models;

namespace FocusBuddy.Services;

public sealed class SettingsService
{
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusBuddy");
        Directory.CreateDirectory(appData);
        _settingsPath = Path.Combine(appData, "settings.json");
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = new AppSettings();
            await SaveAsync(defaults);
            return defaults;
        }

        var raw = await File.ReadAllTextAsync(_settingsPath);
        return JsonSerializer.Deserialize<AppSettings>(raw) ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var raw = JsonSerializer.Serialize(settings, _jsonOptions);
        await File.WriteAllTextAsync(_settingsPath, raw);
    }
}
