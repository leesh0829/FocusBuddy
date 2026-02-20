namespace FocusBuddy.Models;

public sealed class UsageSession
{
    public long Id { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationSeconds { get; set; }
}
