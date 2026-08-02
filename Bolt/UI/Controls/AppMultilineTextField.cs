using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>Labelled multiline text input matching the standard Bolt text field.</summary>
internal sealed class AppMultilineTextField : Control
{
    private const int LabelHeight = 18;

    private readonly Label _label;
    private readonly TextBox _input;
    private bool _useSectionLabelStyle;

    public AppMultilineTextField()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.SupportsTransparentBackColor,
            true);

        BackColor = Color.Transparent;
        TabStop = false;

        _label = new Label
        {
            BackColor = Color.Transparent,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Height = LabelHeight,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _input = new TextBox
        {
            AcceptsReturn = true,
            BackColor = AppTheme.Colors.SurfaceAlt,
            BorderStyle = BorderStyle.None,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextPrimary,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        _input.TextChanged += (_, e) => ValueChanged?.Invoke(this, e);
        _input.GotFocus += (_, _) => Invalidate();
        _input.LostFocus += (_, _) => Invalidate();

        Controls.AddRange([_label, _input]);
    }

    [Category("Behavior")]
    public event EventHandler? ValueChanged;

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _label.Text;
        set => _label.Text = FormatLabel(value);
    }

    /// <summary>Uses the uppercase overline treatment shared by section labels.</summary>
    public bool UseSectionLabelStyle
    {
        get => _useSectionLabelStyle;
        set
        {
            if (_useSectionLabelStyle == value)
                return;

            var label = _label.Text;
            _useSectionLabelStyle = value;
            _label.Font = value ? AppTheme.Fonts.Overline : AppTheme.Fonts.Body;
            _label.ForeColor = value ? AppTheme.Colors.TextMuted : AppTheme.Colors.TextSecondary;
            _label.TextAlign = value ? ContentAlignment.BottomLeft : ContentAlignment.MiddleLeft;
            _label.Text = FormatLabel(label);
        }
    }

    /// <summary>Content of the multiline input.</summary>
    public string Value
    {
        get => _input.Text;
        set => _input.Text = value ?? string.Empty;
    }

    public string Placeholder
    {
        get => _input.PlaceholderText;
        set => _input.PlaceholderText = value ?? string.Empty;
    }

    protected override Size DefaultSize => new(320, 88);

    public new void Focus() => _input.Focus();

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        _label.SetBounds(0, 0, Width, LabelHeight);

        var fieldTop = LabelHeight + AppTheme.Spacing.Tiny;
        _input.SetBounds(
            AppTheme.Spacing.Medium,
            fieldTop + AppTheme.Spacing.Small,
            Math.Max(Width - (AppTheme.Spacing.Medium * 2), 10),
            Math.Max(Height - fieldTop - (AppTheme.Spacing.Small * 2), 10));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.UseHighQuality();

        var fieldTop = LabelHeight + AppTheme.Spacing.Tiny;
        var bounds = new Rectangle(0, fieldTop, Width - 1, Height - fieldTop - 1);

        graphics.FillRoundedRectangle(AppTheme.Colors.SurfaceAlt, bounds, AppTheme.Radius.Small);
        graphics.DrawRoundedBorder(
            _input.Focused ? AppTheme.Colors.Accent : AppTheme.Colors.Border,
            bounds,
            AppTheme.Radius.Small,
            _input.Focused ? 1.6f : 1.2f);

        base.OnPaint(e);
    }

    private string FormatLabel(string? value) =>
        _useSectionLabelStyle ? (value ?? string.Empty).ToUpperInvariant() : value ?? string.Empty;
}
