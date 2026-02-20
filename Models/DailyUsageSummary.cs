namespace FocusBuddy.Models;

public sealed class DailyUsageSummary
{
    public DateOnly Date { get; set; }
    public string Category { get; set; } = "Other";
    public int DurationSeconds { get; set; }
}
