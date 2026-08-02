using System.Drawing;
using System.Windows.Forms;

namespace Bolt.UI.Controls;

/// <summary>One-pixel frame shared by data grids throughout the application.</summary>
internal sealed class GridFrame : Panel
{
    public static Color BorderColor { get; } = Color.FromArgb(0xE8, 0xE9, 0xED);

    public GridFrame()
    {
        BackColor = BorderColor;
        Padding = new Padding(1);
    }
}
