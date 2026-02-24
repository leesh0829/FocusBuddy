using System.Windows.Media;

namespace FocusBuddy.Models;

public sealed class ProgramDisplayItem
{
    public required string ProcessName { get; init; }

    public ImageSource? Icon { get; init; }
}
