using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using FocusBuddy.Models;
using FocusBuddy.Services;

namespace FocusBuddy.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string English = "en";
    private const string Korean = "ko";
    private const string SelfProcessName = "focusbuddy.exe";

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
    private ProgramDisplayItem? _selectedRunningProgramForBlacklist;

    [ObservableProperty]
    private ProgramDisplayItem? _selectedBlacklistProgram;

    [ObservableProperty]
    private ProgramDisplayItem? _selectedRunningProgramForRule;

    [ObservableProperty]
    private ProgramDisplayItem? _selectedRuleProgram;

    [ObservableProperty] private string _subtitleText = string.Empty;
    [ObservableProperty] private string _refreshDashboardText = string.Empty;
    [ObservableProperty] private string _languageText = string.Empty;
    [ObservableProperty] private string _focusModeTitleText = string.Empty;
    [ObservableProperty] private string _enableFocusModeText = string.Empty;
    [ObservableProperty] private string _enableFocusModeTooltipText = string.Empty;
    [ObservableProperty] private string _autoMinimizeText = string.Empty;
    [ObservableProperty] private string _autoMinimizeTooltipText = string.Empty;
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
    public ObservableCollection<ProgramDisplayItem> RunningPrograms { get; } = [];
    public ObservableCollection<ProgramDisplayItem> FocusModeBlacklistPrograms { get; } = [];
    public ObservableCollection<ProgramDisplayItem> SelectedCategoryRulePrograms { get; } = [];

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
        var iconByProcess = new Dictionary<string, BitmapSource?>(StringComparer.OrdinalIgnoreCase);

        var processes = Process.GetProcesses()
            .Where(x => !string.IsNullOrWhiteSpace(x.ProcessName))
            .Select(x =>
            {
                var processName = x.ProcessName.ToLowerInvariant() + ".exe";
                iconByProcess.TryAdd(processName, TryGetProcessIcon(x));
                return processName;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        RunningPrograms.Clear();
        foreach (var process in processes)
        {
            RunningPrograms.Add(new ProgramDisplayItem
            {
                ProcessName = process,
                Icon = iconByProcess.TryGetValue(process, out var icon) ? icon : null
            });
        }
    }

    [RelayCommand]
    private void AddBlacklistProgram()
    {
        if (SelectedRunningProgramForBlacklist is null)
        {
            return;
        }

        if (string.Equals(SelectedRunningProgramForBlacklist.ProcessName, SelfProcessName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AddUnique(FocusModeBlacklistPrograms, SelectedRunningProgramForBlacklist.ProcessName);
    }

    [RelayCommand]
    private void RemoveBlacklistProgram()
    {
        if (SelectedBlacklistProgram is null)
        {
            return;
        }

        FocusModeBlacklistPrograms.Remove(SelectedBlacklistProgram);
    }

    [RelayCommand]
    private void AddProgramToSelectedCategoryRule()
    {
        if (SelectedCategoryRule is null || SelectedRunningProgramForRule is null)
        {
            return;
        }

        var programs = SplitCsv(SelectedCategoryRule.ProcessNames);
        AddUnique(programs, SelectedRunningProgramForRule.ProcessName);
        SelectedCategoryRule.ProcessNames = string.Join(", ", programs);
        RefreshSelectedRulePrograms();
    }

    [RelayCommand]
    private void RemoveProgramFromSelectedCategoryRule()
    {
        if (SelectedCategoryRule is null || SelectedRuleProgram is null)
        {
            return;
        }

        var programs = SplitCsv(SelectedCategoryRule.ProcessNames);
        var removed = programs.RemoveAll(x => string.Equals(x, SelectedRuleProgram.ProcessName, StringComparison.OrdinalIgnoreCase));
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

        var filteredBlacklist = FocusModeBlacklistPrograms
            .Select(x => x.ProcessName)
            .Where(x => !string.Equals(x, SelfProcessName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        FocusModeBlacklistPrograms.Clear();
        foreach (var processName in filteredBlacklist)
        {
            AddUnique(FocusModeBlacklistPrograms, processName);
        }

        _settings.FocusModeBlacklist = filteredBlacklist;

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
            AddProgramIfMissing(SelectedCategoryRulePrograms, program);
        }
    }

    private void BindSettings(AppSettings settings)
    {
        IsFocusModeEnabled = settings.FocusModeEnabled;
        AutoMinimizeDistractingApps = settings.AutoMinimizeDistractingApps;
        FocusModeBlacklistPrograms.Clear();
        foreach (var processName in settings.FocusModeBlacklist)
        {
            if (string.Equals(processName, SelfProcessName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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
            EnableFocusModeTooltipText = "집중 모드를 켜면 블랙리스트 앱 실행 시 알림을 보여줍니다.";
            AutoMinimizeText = "방해 앱 자동 최소화";
            AutoMinimizeTooltipText = "집중 모드가 켜져 있을 때 블랙리스트 앱 창을 자동으로 최소화합니다.";
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
        EnableFocusModeTooltipText = "When enabled, FocusBuddy shows reminders if a blacklisted app becomes active.";
        AutoMinimizeText = "Auto-minimize distracting apps";
        AutoMinimizeTooltipText = "When Focus Mode is on, blacklisted app windows are minimized automatically.";
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

    private void AddUnique(ICollection<ProgramDisplayItem> items, string value)
    {
        AddProgramIfMissing(items, value);
    }

    private static void AddUnique(ICollection<string> items, string value)
    {
        if (!items.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(value);
        }
    }

    private void AddProgramIfMissing(ICollection<ProgramDisplayItem> items, string processName)
    {
        if (items.Any(x => string.Equals(x.ProcessName, processName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var runningProgram = RunningPrograms.FirstOrDefault(x => string.Equals(x.ProcessName, processName, StringComparison.OrdinalIgnoreCase));
        items.Add(new ProgramDisplayItem
        {
            ProcessName = processName,
            Icon = runningProgram?.Icon
        });
    }

    private static BitmapSource? TryGetProcessIcon(Process process)
    {
        try
        {
            var filePath = process.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            using var icon = Icon.ExtractAssociatedIcon(filePath);
            if (icon is null)
            {
                return null;
            }

            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(16, 16));
        }
        catch
        {
            return null;
        }
    }
}
