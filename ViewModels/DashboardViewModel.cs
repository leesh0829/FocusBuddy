using CommunityToolkit.Mvvm.ComponentModel;
using FocusBuddy.Data;

namespace FocusBuddy.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string _todayTotalTime = "00:00:00";

    [ObservableProperty]
    private IEnumerable<string> _categoryBreakdown = [];

    [ObservableProperty]
    private IEnumerable<string> _weeklyTotals = [];

    [ObservableProperty]
    private IEnumerable<string> _topApps = [];

    public DashboardViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task RefreshAsync()
    {
        var totalSeconds = await _databaseService.GetTodayTotalSecondsAsync();
        TodayTotalTime = TimeSpan.FromSeconds(totalSeconds).ToString("hh\\:mm\\:ss");

        var todayByCategory = await _databaseService.GetTodayByCategoryAsync();
        CategoryBreakdown = todayByCategory
            .OrderByDescending(x => x.DurationSeconds)
            .Select(x => $"{x.Category}: {TimeSpan.FromSeconds(x.DurationSeconds):hh\\:mm\\:ss}")
            .ToList();

        var weekData = await _databaseService.GetLast7DaysByCategoryAsync();
        WeeklyTotals = weekData
            .GroupBy(x => x.Date)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key:yyyy-MM-dd}: {TimeSpan.FromSeconds(x.Sum(y => y.DurationSeconds)):hh\\:mm\\:ss}")
            .ToList();

        var topApps = await _databaseService.GetTopAppsTodayAsync();
        TopApps = topApps.Select(x => $"{x.ProcessName} - {TimeSpan.FromSeconds(x.DurationSeconds):hh\\:mm\\:ss}").ToList();
    }
}
