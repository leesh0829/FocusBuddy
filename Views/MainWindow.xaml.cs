using System.ComponentModel;
using System.Windows;
using FocusBuddy.Services;
using FocusBuddy.ViewModels;

namespace FocusBuddy.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel, WindowTrackingService trackingService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.InitializeAsync();
        trackingService.UsageUpdated += async (_, _) => await Dispatcher.InvokeAsync(async () => await _viewModel.Dashboard.RefreshAsync());
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (Application.Current.ShutdownMode == ShutdownMode.OnExplicitShutdown)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
