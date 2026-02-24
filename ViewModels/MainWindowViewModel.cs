using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FocusBuddy.Models;
using FocusBuddy.Services;

namespace FocusBuddy.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string English = "en";
    private const string Korean = "ko";

    private readonly DashboardViewModel _dashboardViewModel;
    private readonly FocusModeService _focusModeService;
    private readonly CategoryService _categoryService;
    private AppSettings _settings = new();

    [ObservableProperty]
    private bool _isFocusModeEnabled;

    [ObservableProperty]
    private bool _autoMinimizeDistractingApps;

    [ObservableProperty]
    private string _selectedLanguage = English;

    [ObservableProperty]
    private CategoryRuleEditorViewModel? _selectedCategoryRule;

    [ObservableProperty]
    private string? _selectedRunningProgramForBlacklist;

    [ObservableProperty]
    private string? _selectedBlacklistProgram;

    [ObservableProperty]
    private string? _selectedRunningProgramForRule;

    [ObservableProperty]
    private string? _selectedRuleProgram;

    [ObservableProperty] private string _subtitleText = string.Empty;
    [ObservableProperty] private string _refreshDashboardText = string.Empty;
    [ObservableProperty] private string _languageText = string.Empty;
    [ObservableProperty] private string _focusModeTitleText = string.Empty;
    [ObservableProperty] private string _enableFocusModeText = string.Empty;
    [ObservableProperty] private string _autoMinimizeText = string.Empty;
    [ObservableProperty] private string _blacklistLabelText = string.Empty;
    [ObservableProperty] private string _blacklistAvailableProgramsText = string.Empty;
    [ObservableProperty] private string _blacklistSelectedProgramsText = string.Empty;
    [ObservableProperty] private string _ruleSelectedProgramsText = string.Empty;
    [ObservableProperty] private string _refreshProgramListText = string.Empty;
    [ObservableProperty] private string _addProgramText = string.Empty;
    [ObservableProperty] private string _removeProgramText = string.Empty;
    [ObservableProperty] private string _saveFocusSettingsText = string.Empty;
    [ObservableProperty] private string _todayTotalText = string.Empty;
    [ObservableProperty] private string _todayByCategoryText = string.Empty;
    [ObservableProperty] private string _last7DaysText = string.Empty;
    [ObservableProperty] private string _topAppsTodayText = string.Empty;
    [ObservableProperty] private string _categoryRulesText = string.Empty;
    [ObservableProperty] private string _categoryRulesHeaderText = string.Empty;
    [ObservableProperty] private string _addCategoryRuleText = string.Empty;
    [ObservableProperty] private string _removeCategoryRuleText = string.Empty;
    [ObservableProperty] private string _saveCategoryRulesText = string.Empty;
    [ObservableProperty] private string _categoryColumnText = string.Empty;
    [ObservableProperty] private string _processColumnText = string.Empty;
    [ObservableProperty] private string _keywordsColumnText = string.Empty;

    public IReadOnlyList<string> LanguageOptions { get; } = [English, Korean];

    public DashboardViewModel Dashboard => _dashboardViewModel;
    public ObservableCollection<CategoryRuleEditorViewModel> CategoryRules { get; } = [];
    public ObservableCollection<string> RunningPrograms { get; } = [];
    public ObservableCollection<string> FocusModeBlacklistPrograms { get; } = [];
    public ObservableCollection<string> SelectedCategoryRulePrograms { get; } = [];

    public MainWindowViewModel(DashboardViewModel dashboardViewModel, FocusModeService focusModeService, CategoryService categoryService)
    {
        _dashboardViewModel = dashboardViewModel;
        _focusModeService = focusModeService;
        _categoryService = categoryService;

        foreach (var rule in _categoryService.GetRules())
        {
            CategoryRules.Add(ToEditorRule(rule));
        }

        RefreshRunningPrograms();
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
    private void RefreshRunningPrograms()
    {
        var processes = Process.GetProcesses()
            .Where(x => !string.IsNullOrWhiteSpace(x.ProcessName))
            .Select(x => x.ProcessName.ToLowerInvariant() + ".exe")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        RunningPrograms.Clear();
        foreach (var process in processes)
        {
            RunningPrograms.Add(process);
        }
    }

    [RelayCommand]
    private void AddBlacklistProgram()
    {
        if (string.IsNullOrWhiteSpace(SelectedRunningProgramForBlacklist))
        {
            return;
        }

        AddUnique(FocusModeBlacklistPrograms, SelectedRunningProgramForBlacklist);
    }

    [RelayCommand]
    private void RemoveBlacklistProgram()
    {
        if (string.IsNullOrWhiteSpace(SelectedBlacklistProgram))
        {
            return;
        }

        FocusModeBlacklistPrograms.Remove(SelectedBlacklistProgram);
    }

    [RelayCommand]
    private void AddProgramToSelectedCategoryRule()
    {
        if (SelectedCategoryRule is null || string.IsNullOrWhiteSpace(SelectedRunningProgramForRule))
        {
            return;
        }

        var programs = SplitCsv(SelectedCategoryRule.ProcessNames);
        AddUnique(programs, SelectedRunningProgramForRule);
        SelectedCategoryRule.ProcessNames = string.Join(", ", programs);
        RefreshSelectedRulePrograms();
    }

    [RelayCommand]
    private void RemoveProgramFromSelectedCategoryRule()
    {
        if (SelectedCategoryRule is null || string.IsNullOrWhiteSpace(SelectedRuleProgram))
        {
            return;
        }

        var programs = SplitCsv(SelectedCategoryRule.ProcessNames);
        var removed = programs.RemoveAll(x => string.Equals(x, SelectedRuleProgram, StringComparison.OrdinalIgnoreCase));
        if (removed == 0)
        {
            return;
        }

        SelectedCategoryRule.ProcessNames = string.Join(", ", programs);
        RefreshSelectedRulePrograms();
    }

    [RelayCommand]
    private async Task SaveFocusSettingsAsync()
    {
        _settings.FocusModeEnabled = IsFocusModeEnabled;
        _settings.AutoMinimizeDistractingApps = AutoMinimizeDistractingApps;
        _settings.FocusModeBlacklist = FocusModeBlacklistPrograms.ToList();

        await _focusModeService.SaveSettingsAsync(_settings);
    }

    [RelayCommand]
    private void AddCategoryRule()
    {
        var newRule = new CategoryRuleEditorViewModel
        {
            Category = "New Category"
        };

        CategoryRules.Add(newRule);
        SelectedCategoryRule = newRule;
    }

    [RelayCommand]
    private void RemoveCategoryRule()
    {
        if (SelectedCategoryRule is null)
        {
            return;
        }

        CategoryRules.Remove(SelectedCategoryRule);
        SelectedCategoryRule = null;
        SelectedCategoryRulePrograms.Clear();
    }

    [RelayCommand]
    private async Task SaveCategoryRulesAsync()
    {
        var rules = CategoryRules
            .Where(x => !string.IsNullOrWhiteSpace(x.Category))
            .Select(x => new CategoryRule
            {
                Category = x.Category.Trim(),
                ProcessNames = SplitCsv(x.ProcessNames),
                WindowTitleKeywords = SplitCsv(x.WindowTitleKeywords)
            })
            .ToList();

        await _categoryService.SaveRulesAsync(rules);
        await Dashboard.RefreshAsync();
    }

    partial void OnSelectedCategoryRuleChanged(CategoryRuleEditorViewModel? value)
    {
        RefreshSelectedRulePrograms();
    }

    private void RefreshSelectedRulePrograms()
    {
        SelectedRuleProgram = null;
        SelectedCategoryRulePrograms.Clear();
        if (SelectedCategoryRule is null)
        {
            return;
        }

        foreach (var program in SplitCsv(SelectedCategoryRule.ProcessNames))
        {
            SelectedCategoryRulePrograms.Add(program);
        }
    }

    private void BindSettings(AppSettings settings)
    {
        IsFocusModeEnabled = settings.FocusModeEnabled;
        AutoMinimizeDistractingApps = settings.AutoMinimizeDistractingApps;
        FocusModeBlacklistPrograms.Clear();
        foreach (var processName in settings.FocusModeBlacklist)
        {
            AddUnique(FocusModeBlacklistPrograms, processName);
        }

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
            BlacklistLabelText = "블랙리스트 프로그램";
            BlacklistAvailableProgramsText = "현재 실행 중 프로그램";
            BlacklistSelectedProgramsText = "선택된 블랙리스트";
            RuleSelectedProgramsText = "선택 규칙의 프로그램";
            RefreshProgramListText = "프로그램 목록 새로고침";
            AddProgramText = "추가";
            RemoveProgramText = "제거";
            SaveFocusSettingsText = "집중 설정 저장";
            TodayTotalText = "오늘 총 사용 시간";
            TodayByCategoryText = "오늘 카테고리별";
            Last7DaysText = "최근 7일";
            TopAppsTodayText = "오늘 상위 5개 앱";
            CategoryRulesText = "카테고리 규칙";
            CategoryRulesHeaderText = "앱/창 이름을 카테고리에 매핑해 대시보드 분류 기준을 설정하는 표입니다.";
            AddCategoryRuleText = "규칙 추가";
            RemoveCategoryRuleText = "선택 규칙 삭제";
            SaveCategoryRulesText = "카테고리 규칙 저장";
            CategoryColumnText = "카테고리";
            ProcessColumnText = "프로세스(선택 목록)";
            KeywordsColumnText = "제목 키워드(쉼표 구분)";
            return;
        }

        SubtitleText = "Personal usage analyzer";
        RefreshDashboardText = "Refresh Dashboard";
        LanguageText = "Language";
        FocusModeTitleText = "Focus Mode";
        EnableFocusModeText = "Enable Focus Mode";
        AutoMinimizeText = "Auto-minimize distracting apps";
        BlacklistLabelText = "Blacklist Programs";
        BlacklistAvailableProgramsText = "Currently Running Programs";
        BlacklistSelectedProgramsText = "Selected Blacklist";
        RuleSelectedProgramsText = "Programs in Selected Rule";
        RefreshProgramListText = "Refresh Program List";
        AddProgramText = "Add";
        RemoveProgramText = "Remove";
        SaveFocusSettingsText = "Save Focus Settings";
        TodayTotalText = "Today Total";
        TodayByCategoryText = "Today by Category";
        Last7DaysText = "Last 7 Days";
        TopAppsTodayText = "Top 5 Apps Today";
        CategoryRulesText = "Category Rules";
        CategoryRulesHeaderText = "Use this table to map apps/window titles to categories used in dashboard summaries.";
        AddCategoryRuleText = "Add Rule";
        RemoveCategoryRuleText = "Remove Selected Rule";
        SaveCategoryRulesText = "Save Category Rules";
        CategoryColumnText = "Category";
        ProcessColumnText = "Processes (selected list)";
        KeywordsColumnText = "Window Title Keywords (comma-separated)";
    }

    private static CategoryRuleEditorViewModel ToEditorRule(CategoryRule rule)
    {
        return new CategoryRuleEditorViewModel
        {
            Category = rule.Category,
            ProcessNames = string.Join(", ", rule.ProcessNames),
            WindowTitleKeywords = string.Join(", ", rule.WindowTitleKeywords)
        };
    }

    private static List<string> SplitCsv(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddUnique(ICollection<string> items, string value)
    {
        if (!items.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(value);
        }
    }
}
