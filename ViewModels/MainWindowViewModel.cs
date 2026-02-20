using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocusBuddy.Models;
using FocusBuddy.Services;

namespace FocusBuddy.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly FocusModeService _focusModeService;

    [ObservableProperty]
    private bool _isFocusModeEnabled;

    [ObservableProperty]
    private bool _autoMinimizeDistractingApps;

    [ObservableProperty]
    private string _blacklistRaw = string.Empty;

    public DashboardViewModel Dashboard => _dashboardViewModel;
    public IReadOnlyList<Models.CategoryRule> CategoryRules { get; }

    public MainWindowViewModel(DashboardViewModel dashboardViewModel, FocusModeService focusModeService, CategoryService categoryService)
    {
        _dashboardViewModel = dashboardViewModel;
        _focusModeService = focusModeService;
        CategoryRules = categoryService.GetRules();
    }

    public async Task InitializeAsync()
    {
        await Dashboard.RefreshAsync();
        var settings = await _focusModeService.GetSettingsAsync();
        BindSettings(settings);
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        await Dashboard.RefreshAsync();
    }

    [RelayCommand]
    private async Task SaveFocusSettingsAsync()
    {
        var settings = new AppSettings
        {
            FocusModeEnabled = IsFocusModeEnabled,
            AutoMinimizeDistractingApps = AutoMinimizeDistractingApps,
            FocusModeBlacklist = BlacklistRaw
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
        };

        await _focusModeService.SaveSettingsAsync(settings);
    }

    private void BindSettings(AppSettings settings)
    {
        IsFocusModeEnabled = settings.FocusModeEnabled;
        AutoMinimizeDistractingApps = settings.AutoMinimizeDistractingApps;
        BlacklistRaw = string.Join(Environment.NewLine, settings.FocusModeBlacklist);
    }
}
