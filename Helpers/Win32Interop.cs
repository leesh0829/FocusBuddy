using System.Runtime.InteropServices;
using System.Text;

namespace FocusBuddy.Helpers;

public static class Win32Interop
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public const int SW_MINIMIZE = 6;

    public static string ReadWindowTitle(IntPtr windowHandle)
    {
        var builder = new StringBuilder(512);
        _ = GetWindowText(windowHandle, builder, builder.Capacity);
        return builder.ToString();
    }
}
