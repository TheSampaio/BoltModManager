using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>
/// Labelled text input with a placeholder, a focus ring and an optional trailing action button.
/// </summary>
/// <remarks>
/// Replaces the previous <c>TextEntry</c> control, whose <c>Value</c> setter raised
/// <c>ValueChanged</c> a second time on top of the one already raised by the inner text box.
/// </remarks>
internal sealed class AppTextField : Control
{
    private const int LabelHeight = 18;
    private const int FieldHeight = 36;
    private const int ActionWidth = 36;

    private readonly Label _label;
    private readonly TextBox _input;
    private readonly AppButton _action;

    private bool _showAction;
    private bool _useSectionLabelStyle;

    public AppTextField()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

        BackColor = Color.Transparent;
        TabStop = false;

        _label = new Label
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Height = LabelHeight,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _input = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = AppTheme.Colors.SurfaceAlt,
            ForeColor = AppTheme.Colors.TextPrimary,
            Font = AppTheme.Fonts.Body
        };

        _action = new AppButton
        {
            Variant = ButtonVariant.Ghost,
            Icon = IconKind.Ellipsis,
            Visible = false,
            TabStop = false
        };

        _input.TextChanged += (_, e) => OnValueChanged(e);
        _input.GotFocus += (_, _) => Invalidate();
        _input.LostFocus += (_, _) => Invalidate();
        _action.Click += (_, e) => ActionClick?.Invoke(this, e);

        Controls.AddRange([_label, _input, _action]);

        Height = LabelHeight + AppTheme.Spacing.Tiny + FieldHeight;
    }

    /// <summary>Raised whenever the text changes, from the user or from code.</summary>
    [Category("Behavior")]
    public event EventHandler? ValueChanged;

    /// <summary>Raised when the trailing action button is pressed.</summary>
    [Category("Behavior")]
    public event EventHandler? ActionClick;

    /// <summary>Caption displayed above the input.</summary>
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

    /// <summary>Content of the input.</summary>
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

    public bool ReadOnly
    {
        get => _input.ReadOnly;
        set
        {
            _input.ReadOnly = value;
            _input.TabStop = !value;
            _input.BackColor = value ? AppTheme.Colors.Background : AppTheme.Colors.SurfaceAlt;
            _input.ForeColor = value ? AppTheme.Colors.TextSecondary : AppTheme.Colors.TextPrimary;
        }
    }

    /// <summary>Shows a browse button at the right edge of the field.</summary>
    public bool ShowAction
    {
        get => _showAction;
        set
        {
            _showAction = value;
            _action.Visible = value;
            PerformLayout();
        }
    }

    public IconKind ActionIcon
    {
        get => _action.Icon;
        set => _action.Icon = value;
    }

    protected override Size DefaultSize => new(320, LabelHeight + AppTheme.Spacing.Tiny + FieldHeight);

    public new void Focus() => _input.Focus();

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        _label.SetBounds(0, 0, Width, LabelHeight);

        var fieldTop = LabelHeight + AppTheme.Spacing.Tiny;
        var fieldHeight = GetFieldHeight(fieldTop);
        var actionSpace = _showAction ? ActionWidth : 0;

        _input.SetBounds(
            AppTheme.Spacing.Medium,
            fieldTop + Math.Max((fieldHeight - _input.PreferredHeight) / 2, 0),
            Math.Max(Width - actionSpace - (AppTheme.Spacing.Medium * 2), 10),
            _input.PreferredHeight);

        _action.SetBounds(Width - ActionWidth, fieldTop, ActionWidth, fieldHeight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.UseHighQuality();

        var fieldTop = LabelHeight + AppTheme.Spacing.Tiny;
        var bounds = new Rectangle(0, fieldTop, Width - 1, Math.Max(GetFieldHeight(fieldTop) - 1, 1));

        graphics.FillRoundedRectangle(_input.BackColor, bounds, AppTheme.Radius.Small);
        graphics.DrawRoundedBorder(
            _input.Focused ? AppTheme.Colors.Accent : AppTheme.Colors.Border,
            bounds,
            AppTheme.Radius.Small,
            _input.Focused ? 1.6f : 1.2f);

        base.OnPaint(e);
    }

    private void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

    private string FormatLabel(string? value) =>
        _useSectionLabelStyle ? (value ?? string.Empty).ToUpperInvariant() : value ?? string.Empty;

    private int GetFieldHeight(int fieldTop) => Math.Max(Math.Min(FieldHeight, Height - fieldTop), 1);
}
