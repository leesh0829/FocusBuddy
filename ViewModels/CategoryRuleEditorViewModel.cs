using CommunityToolkit.Mvvm.ComponentModel;

namespace FocusBuddy.ViewModels;

public partial class CategoryRuleEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _processNames = string.Empty;

    [ObservableProperty]
    private string _windowTitleKeywords = string.Empty;
}
