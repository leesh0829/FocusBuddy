namespace FocusBuddy.Models;

public sealed class AppSettings
{
    public bool RunOnStartup { get; set; }
    public bool AutoMinimizeDistractingApps { get; set; }
    public bool FocusModeEnabled { get; set; }
    public string UiLanguage { get; set; } = "en";
    public List<string> FocusModeBlacklist { get; set; } = ["youtube.exe", "steam.exe", "discord.exe"];
}
