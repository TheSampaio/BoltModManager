using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>Visual weight of an <see cref="AppButton"/>.</summary>
internal enum ButtonVariant
{
    /// <summary>Filled with the accent colour. One per view, for the main action.</summary>
    Primary,

    /// <summary>Outlined surface. The default for secondary actions.</summary>
    Secondary,

    /// <summary>Outlined surface using the product accent, intended for dialog cancellation.</summary>
    AccentOutline,

    /// <summary>Transparent until hovered. For toolbars and icon-only actions.</summary>
    Ghost,

    /// <summary>Outlined, tinted red. For destructive actions.</summary>
    Danger
}

/// <summary>
/// Flat, rounded button with hover and pressed states, an optional vector icon and a focus ring.
/// </summary>
internal sealed class AppButton : Control
{
    private ButtonVariant _variant = ButtonVariant.Secondary;
    private IconKind _icon = IconKind.None;
    private int _cornerRadius = AppTheme.Radius.Small;
    private bool _isHovered;
    private bool _isPressed;

    public AppButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint
            | ControlStyles.SupportsTransparentBackColor,
            true);

        Font = AppTheme.Fonts.Body;
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Size = new Size(120, 34);
    }

    [DefaultValue(ButtonVariant.Secondary)]
    public ButtonVariant Variant
    {
        get => _variant;
        set => SetField(ref _variant, value);
    }

    [DefaultValue(IconKind.None)]
    public IconKind Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    /// <summary>Size of the icon in pixels. Defaults to a size proportional to the button.</summary>
    public int IconSize { get; set; } = 16;

    public int CornerRadius
    {
        get => _cornerRadius;
        set => SetField(ref _cornerRadius, value);
    }

    protected override Size DefaultSize => new(120, 34);

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Focus();
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Space or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            OnClick(EventArgs.Empty);
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        Invalidate();
        base.OnTextChanged(e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        Invalidate();
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        Invalidate();
        base.OnLostFocus(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.UseHighQuality();

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        var (background, border, foreground) = ResolveColors();

        if (background.A > 0)
            graphics.FillRoundedRectangle(background, bounds, CornerRadius);

        if (border.A > 0)
            graphics.DrawRoundedBorder(border, bounds, CornerRadius, 1.2f);

        if (Focused && Enabled)
            graphics.DrawRoundedBorder(AppTheme.Colors.AccentText, Rectangle.Inflate(bounds, -2, -2), Math.Max(CornerRadius - 2, 0), 1.2f);

        PaintContent(graphics, bounds, foreground);
    }

    private void PaintContent(Graphics graphics, Rectangle bounds, Color foreground)
    {
        var hasText = !string.IsNullOrEmpty(Text);
        var hasIcon = Icon != IconKind.None;

        if (!hasIcon)
        {
            if (hasText)
                DrawText(graphics, bounds, foreground);

            return;
        }

        if (!hasText)
        {
            Icons.Draw(graphics, Icon, ToSquare(bounds, IconSize), foreground);
            return;
        }

        const int gap = AppTheme.Spacing.Small;

        var textSize = TextRenderer.MeasureText(Text, Font);
        var contentWidth = IconSize + gap + textSize.Width;
        var left = bounds.X + ((bounds.Width - contentWidth) / 2f);

        Icons.Draw(
            graphics,
            Icon,
            new RectangleF(left, bounds.Y + ((bounds.Height - IconSize) / 2f), IconSize, IconSize),
            foreground);

        TextRenderer.DrawText(
            graphics,
            Text,
            Font,
            new Rectangle((int)(left + IconSize + gap), bounds.Y, textSize.Width, bounds.Height),
            foreground,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
    }

    private void DrawText(Graphics graphics, Rectangle bounds, Color foreground) =>
        TextRenderer.DrawText(
            graphics,
            Text,
            Font,
            bounds,
            foreground,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

    private static RectangleF ToSquare(Rectangle bounds, int size) => new(
        bounds.X + ((bounds.Width - size) / 2f),
        bounds.Y + ((bounds.Height - size) / 2f),
        size,
        size);

    private (Color Background, Color Border, Color Foreground) ResolveColors()
    {
        var colors = AppTheme.Colors;

        if (!Enabled)
        {
            // Disabled keeps the same silhouette as enabled: a Ghost button that grows a border
            // only while disabled reads as the interface losing its outlines when a game loads.
            return Variant switch
            {
                ButtonVariant.Primary => (colors.SurfaceActive, Color.Transparent, colors.TextMuted),
                ButtonVariant.Ghost => (Color.Transparent, Color.Transparent, colors.TextMuted),
                _ => (Color.Transparent, colors.BorderSubtle, colors.TextMuted)
            };
        }

        return Variant switch
        {
            ButtonVariant.Primary => (
                _isPressed ? colors.AccentPressed : _isHovered ? colors.AccentHover : colors.Accent,
                Color.Transparent,
                colors.OnAccent),

            ButtonVariant.AccentOutline => (
                _isPressed ? colors.SurfaceActive : _isHovered ? colors.SurfaceHover : colors.Surface,
                colors.Accent,
                colors.TextPrimary),

            ButtonVariant.Danger => (
                _isPressed ? colors.Danger : _isHovered ? colors.Danger.Blend(colors.Surface, 0.75f) : Color.Transparent,
                _isHovered || _isPressed ? colors.Danger : colors.Border,
                _isPressed ? colors.OnAccent : colors.Danger),

            ButtonVariant.Ghost => (
                _isPressed ? colors.SurfaceActive : _isHovered ? colors.SurfaceHover : Color.Transparent,
                Color.Transparent,
                _isHovered || _isPressed ? colors.TextPrimary : colors.TextSecondary),

            _ => (
                _isPressed ? colors.SurfaceActive : _isHovered ? colors.SurfaceHover : colors.Surface,
                colors.Border,
                colors.TextPrimary)
        };
    }

    private void SetField<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        Invalidate();
    }
}
