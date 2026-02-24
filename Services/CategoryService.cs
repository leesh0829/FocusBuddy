using System.Text.Json;
using System.IO;
using FocusBuddy.Models;

namespace FocusBuddy.Services;

public sealed class CategoryService
{
    private readonly List<CategoryRule> _rules;
    private readonly string _rulesPath;
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public CategoryService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusBuddy");
        Directory.CreateDirectory(appDataPath);

        _rulesPath = Path.Combine(appDataPath, "category-rules.json");
        var bundledConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "category-rules.json");

        if (!File.Exists(_rulesPath) && File.Exists(bundledConfigPath))
        {
            File.Copy(bundledConfigPath, _rulesPath, overwrite: false);
        }

        if (!File.Exists(_rulesPath))
        {
            _rules = [];
            return;
        }

        var raw = File.ReadAllText(_rulesPath);
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

    public async Task SaveRulesAsync(IEnumerable<CategoryRule> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules);

        var payload = JsonSerializer.Serialize(_rules, SerializerOptions);
        await File.WriteAllTextAsync(_rulesPath, payload);
    }
}
