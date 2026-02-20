namespace FocusBuddy.Models;

public sealed class AppUsageSummary
{
    public string ProcessName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
}
