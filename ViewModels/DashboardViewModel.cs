using CommunityToolkit.Mvvm.ComponentModel;
using FocusBuddy.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace FocusBuddy.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string _todayTotalTime = "00:00:00";

    [ObservableProperty]
    private IEnumerable<ISeries> _categorySeries = [];

    [ObservableProperty]
    private IEnumerable<ISeries> _weeklySeries = [];

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
        CategorySeries = todayByCategory.Select(x => new PieSeries<int>
        {
            Name = x.Category,
            Values = [x.DurationSeconds]
        }).ToList();

        var weekData = await _databaseService.GetLast7DaysByCategoryAsync();
        WeeklySeries = weekData
            .GroupBy(x => x.Date)
            .Select(x => new ColumnSeries<int>
            {
                Name = x.Key.ToString(),
                Values = [x.Sum(y => y.DurationSeconds)]
            }).ToList();

        var topApps = await _databaseService.GetTopAppsTodayAsync();
        TopApps = topApps.Select(x => $"{x.ProcessName} - {TimeSpan.FromSeconds(x.DurationSeconds):hh\\:mm\\:ss}").ToList();
    }
}
