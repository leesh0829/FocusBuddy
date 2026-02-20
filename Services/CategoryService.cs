using System.Text.Json;
using FocusBuddy.Models;

namespace FocusBuddy.Services;

public sealed class CategoryService
{
    private readonly List<CategoryRule> _rules;

    public CategoryService()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "Config", "category-rules.json");
        if (!File.Exists(configPath))
        {
            _rules = [];
            return;
        }

        var raw = File.ReadAllText(configPath);
        _rules = JsonSerializer.Deserialize<List<CategoryRule>>(raw) ?? [];
    }

    public string ResolveCategory(string processName, string title)
    {
        foreach (var rule in _rules)
        {
            if (rule.ProcessNames.Any(p => p.Equals(processName, StringComparison.OrdinalIgnoreCase)))
            {
                return rule.Category;
            }

            if (rule.WindowTitleKeywords.Any(k => title.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return rule.Category;
            }
        }

        return "Other";
    }

    public IReadOnlyList<CategoryRule> GetRules() => _rules;
}
