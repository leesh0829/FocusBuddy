using System.Diagnostics;
using FocusBuddy.Data;
using FocusBuddy.Helpers;
using FocusBuddy.Models;
using Serilog;

namespace FocusBuddy.Services;

public sealed class WindowTrackingService
{
    private const string LockScreenProcessName = "lockapp.exe";

    private readonly CategoryService _categoryService;
    private readonly DatabaseService _databaseService;
    private readonly FocusModeService _focusModeService;
    private CancellationTokenSource? _cts;
    private Task? _pollingTask;

    private IntPtr _currentWindowHandle;
    private string _currentProcess = string.Empty;
    private string _currentTitle = string.Empty;
    private string _currentCategory = "Other";
    private DateTime _sessionStartUtc;
    private DateTime? _lastTickUtc;

    private static readonly TimeSpan SleepGapThreshold = TimeSpan.FromSeconds(10);

    public event EventHandler? UsageUpdated;

    public WindowTrackingService(CategoryService categoryService, DatabaseService databaseService, FocusModeService focusModeService)
    {
        _categoryService = categoryService;
        _databaseService = databaseService;
        _focusModeService = focusModeService;
    }

    public Task StartAsync()
    {
        if (_pollingTask is not null)
        {
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();
        _lastTickUtc = null;
        _pollingTask = Task.Run(() => PollingLoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is null || _pollingTask is null)
        {
            return;
        }

        _cts.Cancel();
        await _pollingTask;
        await CloseCurrentSessionAsync(DateTime.UtcNow);
        _pollingTask = null;
        _lastTickUtc = null;
        _cts.Dispose();
        _cts = null;
    }

    private async Task PollingLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var nowUtc = DateTime.UtcNow;
                if (_lastTickUtc is not null)
                {
                    var gap = nowUtc - _lastTickUtc.Value;
                    if (gap > SleepGapThreshold)
                    {
                        HandleSleepGap(gap);
                    }
                }

                await TrackTickAsync();
                _lastTickUtc = nowUtc;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Tracking tick failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), token);
        }
    }

    private async Task TrackTickAsync()
    {
        var hwnd = Win32Interop.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        if (hwnd == _currentWindowHandle)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        await CloseCurrentSessionAsync(nowUtc);

        var processName = ResolveProcessName(hwnd);
        if (string.Equals(processName, LockScreenProcessName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _currentWindowHandle = hwnd;
        _currentTitle = Win32Interop.ReadWindowTitle(hwnd);
        _currentProcess = processName;
        _currentCategory = _categoryService.ResolveCategory(_currentProcess, _currentTitle);
        _sessionStartUtc = nowUtc;

        _focusModeService.HandleActiveWindow(hwnd, _currentProcess, _currentTitle);
    }

    private async Task CloseCurrentSessionAsync(DateTime endUtc)
    {
        if (_currentWindowHandle == IntPtr.Zero || string.IsNullOrWhiteSpace(_currentProcess))
        {
            return;
        }

        var duration = (int)(endUtc - _sessionStartUtc).TotalSeconds;
        if (duration <= 0)
        {
            return;
        }

        var session = new UsageSession
        {
            ProcessName = _currentProcess,
            WindowTitle = _currentTitle,
            Category = _currentCategory,
            StartTime = _sessionStartUtc.ToLocalTime(),
            EndTime = endUtc.ToLocalTime(),
            DurationSeconds = duration
        };

        await _databaseService.InsertUsageSessionAsync(session);
        UsageUpdated?.Invoke(this, EventArgs.Empty);

        _currentWindowHandle = IntPtr.Zero;
        _currentProcess = string.Empty;
        _currentTitle = string.Empty;
        _currentCategory = "Other";
    }

    private void HandleSleepGap(TimeSpan gap)
    {
        if (_currentWindowHandle == IntPtr.Zero || _sessionStartUtc == default)
        {
            return;
        }

        _sessionStartUtc = _sessionStartUtc.Add(gap);
    }

    private static string ResolveProcessName(IntPtr windowHandle)
    {
        try
        {
            _ = Win32Interop.GetWindowThreadProcessId(windowHandle, out var processId);
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName.ToLowerInvariant() + ".exe";
        }
        catch
        {
            return "unknown.exe";
        }
    }
}
