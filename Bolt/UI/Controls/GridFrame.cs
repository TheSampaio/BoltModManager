using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>One-pixel frame shared by data grids throughout the application.</summary>
internal sealed class GridFrame : Panel
{
    /// <summary>
    /// Dark grids use the neutral frame requested for data-heavy surfaces; light mode keeps its
    /// existing subtle grey instead of borrowing a dark-only literal.
    /// </summary>
    public static Color BorderColor => AppTheme.Colors.IsDark
        ? Color.FromArgb(0x30, 0x33, 0x38)
        : Color.FromArgb(0xE8, 0xE9, 0xED);

    public GridFrame()
    {
        BackColor = BorderColor;
        Padding = new Padding(1);
    }
}
