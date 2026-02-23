using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace FocusBuddy.Services;

public sealed class TrayService : IDisposable
{
    private readonly FocusModeService _focusModeService;
    private NotifyIcon? _notifyIcon;

    public TrayService(FocusModeService focusModeService)
    {
        _focusModeService = focusModeService;
    }

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "FocusBuddy",
            Icon = SystemIcons.Application,
            Visible = true
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Dashboard", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Toggle Focus Mode", null, async (_, _) => await _focusModeService.SetEnabledAsync(!_focusModeService.IsEnabled));
        menu.Items.Add("Exit", null, (_, _) => ExitApp());
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    public void Dispose()
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _notifyIcon = null;
    }

    private static void ShowMainWindow()
    {
        var window = WpfApplication.Current.MainWindow;
        if (window is null)
        {
            return;
        }

        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private static void ExitApp()
    {
        WpfApplication.Current.Shutdown();
    }
}
