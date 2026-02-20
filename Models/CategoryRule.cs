namespace FocusBuddy.Models;

public sealed class CategoryRule
{
    public string Category { get; set; } = "Other";
    public List<string> ProcessNames { get; set; } = [];
    public List<string> WindowTitleKeywords { get; set; } = [];
}
