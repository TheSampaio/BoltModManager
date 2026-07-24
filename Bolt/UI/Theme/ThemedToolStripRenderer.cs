using System.Drawing;
using System.Windows.Forms;

namespace Bolt.UI.Theme;

/// <summary>
/// Renders menus and context menus with the application palette instead of the system colours.
/// </summary>
internal sealed class ThemedToolStripRenderer() : ToolStripProfessionalRenderer(new ThemedColorTable())
{
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item switch
        {
            { Enabled: false } => AppTheme.Colors.TextMuted,
            // The entry currently chosen in a drop-down stays highlighted even when not hovered.
            ToolStripMenuItem { Checked: true } => AppTheme.Colors.AccentText,
            { Selected: true } or { Pressed: true } => AppTheme.Colors.TextPrimary,
            _ => AppTheme.Colors.TextSecondary
        };

        base.OnRenderItemText(e);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = e.Item?.Enabled == true ? AppTheme.Colors.TextSecondary : AppTheme.Colors.TextMuted;
        base.OnRenderArrow(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var bounds = e.Item!.ContentRectangle;
        var middle = bounds.Top + (bounds.Height / 2);

        using var pen = new Pen(AppTheme.Colors.Border);
        e.Graphics.DrawLine(pen, bounds.Left + AppTheme.Spacing.Small, middle, bounds.Right - AppTheme.Spacing.Small, middle);
    }

    /// <summary>Maps the palette onto the colours used by the professional renderer.</summary>
    private sealed class ThemedColorTable : ProfessionalColorTable
    {
        public ThemedColorTable() => UseSystemColors = false;

        public override Color MenuStripGradientBegin => AppTheme.Colors.Background;

        public override Color MenuStripGradientEnd => AppTheme.Colors.Background;

        public override Color MenuBorder => AppTheme.Colors.Border;

        public override Color MenuItemBorder => Color.Transparent;

        public override Color MenuItemSelected => AppTheme.Colors.SurfaceHover;

        public override Color MenuItemSelectedGradientBegin => AppTheme.Colors.SurfaceHover;

        public override Color MenuItemSelectedGradientEnd => AppTheme.Colors.SurfaceHover;

        public override Color MenuItemPressedGradientBegin => AppTheme.Colors.Surface;

        public override Color MenuItemPressedGradientMiddle => AppTheme.Colors.Surface;

        public override Color MenuItemPressedGradientEnd => AppTheme.Colors.Surface;

        public override Color ToolStripDropDownBackground => AppTheme.Colors.Surface;

        public override Color ImageMarginGradientBegin => AppTheme.Colors.Surface;

        public override Color ImageMarginGradientMiddle => AppTheme.Colors.Surface;

        public override Color ImageMarginGradientEnd => AppTheme.Colors.Surface;

        public override Color SeparatorDark => AppTheme.Colors.Border;

        public override Color SeparatorLight => AppTheme.Colors.Border;

        public override Color CheckBackground => AppTheme.Colors.Selection;

        public override Color CheckSelectedBackground => AppTheme.Colors.Selection;
    }
}
