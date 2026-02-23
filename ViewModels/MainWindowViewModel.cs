using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocusBuddy.Models;
using FocusBuddy.Services;

namespace FocusBuddy.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string English = "en";
    private const string Korean = "ko";

    private readonly DashboardViewModel _dashboardViewModel;
    private readonly FocusModeService _focusModeService;
    private AppSettings _settings = new();

    [ObservableProperty]
    private bool _isFocusModeEnabled;

    [ObservableProperty]
    private bool _autoMinimizeDistractingApps;

    [ObservableProperty]
    private string _blacklistRaw = string.Empty;

    [ObservableProperty]
    private string _selectedLanguage = English;

    [ObservableProperty] private string _subtitleText = string.Empty;
    [ObservableProperty] private string _refreshDashboardText = string.Empty;
    [ObservableProperty] private string _languageText = string.Empty;
    [ObservableProperty] private string _focusModeTitleText = string.Empty;
    [ObservableProperty] private string _enableFocusModeText = string.Empty;
    [ObservableProperty] private string _autoMinimizeText = string.Empty;
    [ObservableProperty] private string _blacklistLabelText = string.Empty;
    [ObservableProperty] private string _saveFocusSettingsText = string.Empty;
    [ObservableProperty] private string _todayTotalText = string.Empty;
    [ObservableProperty] private string _todayByCategoryText = string.Empty;
    [ObservableProperty] private string _last7DaysText = string.Empty;
    [ObservableProperty] private string _topAppsTodayText = string.Empty;
    [ObservableProperty] private string _categoryRulesText = string.Empty;

    public IReadOnlyList<string> LanguageOptions { get; } = [English, Korean];

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
        _settings = await _focusModeService.GetSettingsAsync();
        BindSettings(_settings);
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        await Dashboard.RefreshAsync();
    }

    [RelayCommand]
    private async Task SaveFocusSettingsAsync()
    {
        _settings.FocusModeEnabled = IsFocusModeEnabled;
        _settings.AutoMinimizeDistractingApps = AutoMinimizeDistractingApps;
        _settings.FocusModeBlacklist = BlacklistRaw
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        await _focusModeService.SaveSettingsAsync(_settings);
    }

    private void BindSettings(AppSettings settings)
    {
        IsFocusModeEnabled = settings.FocusModeEnabled;
        AutoMinimizeDistractingApps = settings.AutoMinimizeDistractingApps;
        BlacklistRaw = string.Join(Environment.NewLine, settings.FocusModeBlacklist);
        SelectedLanguage = settings.UiLanguage is Korean ? Korean : English;
        ApplyLocalization(SelectedLanguage);
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        var normalizedLanguage = value is Korean ? Korean : English;
        if (value != normalizedLanguage)
        {
            SelectedLanguage = normalizedLanguage;
            return;
        }

        ApplyLocalization(normalizedLanguage);

        if (_settings.UiLanguage == normalizedLanguage)
        {
            return;
        }

        _settings.UiLanguage = normalizedLanguage;
        _ = _focusModeService.SaveSettingsAsync(_settings);
    }

    private void ApplyLocalization(string language)
    {
        if (language == Korean)
        {
            SubtitleText = "개인 사용 시간 분석기";
            RefreshDashboardText = "대시보드 새로고침";
            LanguageText = "언어";
            FocusModeTitleText = "집중 모드";
            EnableFocusModeText = "집중 모드 사용";
            AutoMinimizeText = "방해 앱 자동 최소화";
            BlacklistLabelText = "블랙리스트 (프로세스당 한 줄):";
            SaveFocusSettingsText = "집중 설정 저장";
            TodayTotalText = "오늘 총 사용 시간";
            TodayByCategoryText = "오늘 카테고리별";
            Last7DaysText = "최근 7일";
            TopAppsTodayText = "오늘 상위 5개 앱";
            CategoryRulesText = "카테고리 규칙";
            return;
        }

        SubtitleText = "Personal usage analyzer";
        RefreshDashboardText = "Refresh Dashboard";
        LanguageText = "Language";
        FocusModeTitleText = "Focus Mode";
        EnableFocusModeText = "Enable Focus Mode";
        AutoMinimizeText = "Auto-minimize distracting apps";
        BlacklistLabelText = "Blacklist (one process per line):";
        SaveFocusSettingsText = "Save Focus Settings";
        TodayTotalText = "Today Total";
        TodayByCategoryText = "Today by Category";
        Last7DaysText = "Last 7 Days";
        TopAppsTodayText = "Top 5 Apps Today";
        CategoryRulesText = "Category Rules";
    }
}
