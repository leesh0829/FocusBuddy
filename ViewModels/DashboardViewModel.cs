using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocusBuddy.Data;

namespace FocusBuddy.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public sealed record WeeklyTotalItem(DateOnly Date, string DisplayText);

    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string _todayTotalTime = "00:00:00";

    [ObservableProperty]
    private IEnumerable<string> _categoryBreakdown = [];

    [ObservableProperty]
    private IEnumerable<WeeklyTotalItem> _weeklyTotals = [];

    [ObservableProperty]
    private IEnumerable<string> _topApps = [];

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    public bool IsViewingToday => SelectedDate == DateOnly.FromDateTime(DateTime.Now);
    public bool IsViewingHistoricalDate => !IsViewingToday;

    public DashboardViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task RefreshAsync()
    {
        var weekData = await _databaseService.GetLast7DaysByCategoryAsync();
        WeeklyTotals = weekData
            .GroupBy(x => x.Date)
            .OrderBy(x => x.Key)
            .Select(x => new WeeklyTotalItem(
                x.Key,
                $"{x.Key:yyyy-MM-dd}: {TimeSpan.FromSeconds(x.Sum(y => y.DurationSeconds)):hh\\:mm\\:ss}"))
            .ToList();

        if (!WeeklyTotals.Any(x => x.Date == SelectedDate))
        {
            SelectedDate = DateOnly.FromDateTime(DateTime.Now);
        }

        await RefreshForSelectedDateAsync();
    }

    [RelayCommand]
    private async Task SelectWeeklyDateAsync(WeeklyTotalItem? item)
    {
        if (item is null)
        {
            return;
        }

        SelectedDate = item.Date;
        await RefreshForSelectedDateAsync();
    }

    [RelayCommand]
    private async Task ReturnToTodayAsync()
    {
        SelectedDate = DateOnly.FromDateTime(DateTime.Now);
        await RefreshForSelectedDateAsync();
    }

    private async Task RefreshForSelectedDateAsync()
    {
        var totalSeconds = await _databaseService.GetTotalSecondsByDateAsync(SelectedDate);
        TodayTotalTime = TimeSpan.FromSeconds(totalSeconds).ToString("hh\\:mm\\:ss");

        var todayByCategory = await _databaseService.GetByCategoryAsync(SelectedDate);
        CategoryBreakdown = todayByCategory
            .OrderByDescending(x => x.DurationSeconds)
            .Select(x => $"{x.Category}: {TimeSpan.FromSeconds(x.DurationSeconds):hh\\:mm\\:ss}")
            .ToList();

        var topApps = await _databaseService.GetTopAppsByDateAsync(SelectedDate);
        TopApps = topApps.Select(x => $"{x.ProcessName} - {TimeSpan.FromSeconds(x.DurationSeconds):hh\\:mm\\:ss}").ToList();

        OnPropertyChanged(nameof(IsViewingToday));
        OnPropertyChanged(nameof(IsViewingHistoricalDate));
    }
}
