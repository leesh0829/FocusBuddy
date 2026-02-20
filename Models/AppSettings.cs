namespace FocusBuddy.Models;

public sealed class AppSettings
{
    public bool RunOnStartup { get; set; }
    public bool AutoMinimizeDistractingApps { get; set; }
    public bool FocusModeEnabled { get; set; }
    public List<string> FocusModeBlacklist { get; set; } = ["youtube.exe", "steam.exe", "discord.exe"];
}
