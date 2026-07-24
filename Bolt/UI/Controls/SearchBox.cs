using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>Compact search input with a leading icon and a clear button.</summary>
internal sealed class SearchBox : Control
{
    private const int IconSize = 15;

    private readonly TextBox _input;
    private readonly AppButton _clear;

    public SearchBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

        BackColor = Color.Transparent;

        _input = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = AppTheme.Colors.SurfaceAlt,
            ForeColor = AppTheme.Colors.TextPrimary,
            Font = AppTheme.Fonts.Body,
            PlaceholderText = "Search modifications"
        };

        _clear = new AppButton
        {
            Variant = ButtonVariant.Ghost,
            Icon = IconKind.Close,
            IconSize = 12,
            Visible = false,
            TabStop = false
        };

        _input.TextChanged += OnInputTextChanged;
        _input.GotFocus += (_, _) => Invalidate();
        _input.LostFocus += (_, _) => Invalidate();
        _clear.Click += (_, _) => Clear();

        Controls.AddRange([_input, _clear]);

        Size = new Size(240, 32);
    }

    [Category("Behavior")]
    public event EventHandler? QueryChanged;

    /// <summary>Current search text, trimmed.</summary>
    public string Query => _input.Text.Trim();

    public string Placeholder
    {
        get => _input.PlaceholderText;
        set => _input.PlaceholderText = value ?? string.Empty;
    }

    protected override Size DefaultSize => new(240, 32);

    public void Clear() => _input.Clear();

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        var left = AppTheme.Spacing.Small + IconSize + AppTheme.Spacing.Small;
        var clearWidth = _clear.Visible ? Height : 0;

        _input.SetBounds(
            left,
            (Height - _input.PreferredHeight) / 2,
            Math.Max(Width - left - clearWidth - AppTheme.Spacing.Small, 10),
            _input.PreferredHeight);

        _clear.SetBounds(Width - Height, 0, Height, Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.UseHighQuality();

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        graphics.FillRoundedRectangle(AppTheme.Colors.SurfaceAlt, bounds, AppTheme.Radius.Small);
        graphics.DrawRoundedBorder(
            _input.Focused ? AppTheme.Colors.Accent : AppTheme.Colors.Border,
            bounds,
            AppTheme.Radius.Small,
            _input.Focused ? 1.6f : 1.2f);

        Icons.Draw(
            graphics,
            IconKind.Search,
            new RectangleF(AppTheme.Spacing.Small, (Height - IconSize) / 2f, IconSize, IconSize),
            _input.Focused ? AppTheme.Colors.TextSecondary : AppTheme.Colors.TextMuted,
            2.2f);

        base.OnPaint(e);
    }

    private void OnInputTextChanged(object? sender, EventArgs e)
    {
        var shouldShowClear = _input.TextLength > 0;

        if (_clear.Visible != shouldShowClear)
        {
            _clear.Visible = shouldShowClear;
            PerformLayout();
        }

        QueryChanged?.Invoke(this, EventArgs.Empty);
    }
}
