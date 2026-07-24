using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>
/// Rounded surface used to group related content.
/// </summary>
internal sealed class Card : Panel
{
    public Card()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        BackColor = Color.Transparent;
        Padding = new Padding(AppTheme.Spacing.Large);
    }

    public int CornerRadius { get; set; } = AppTheme.Radius.Large;

    /// <summary>Surface colour. Defaults to the standard card surface of the palette.</summary>
    public Color? SurfaceColor { get; set; }

    public bool ShowBorder { get; set; } = true;

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.UseHighQuality();

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        graphics.FillRoundedRectangle(SurfaceColor ?? AppTheme.Colors.Surface, bounds, CornerRadius);

        if (ShowBorder)
            graphics.DrawRoundedBorder(AppTheme.Colors.Border, bounds, CornerRadius, 1.2f);

        base.OnPaint(e);
    }
}
