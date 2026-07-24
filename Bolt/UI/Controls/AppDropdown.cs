using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>
/// Drop-down list drawn entirely by the application.
/// </summary>
/// <remarks>
/// A native <see cref="ComboBox"/> keeps painting its frame and its button with system colours and
/// gives no hover feedback in a custom theme, so the control is rendered from scratch and the list
/// is shown as a themed drop-down.
/// </remarks>
internal sealed class AppDropdown : Control
{
    private const int ChevronSize = 14;

    private readonly List<object> _items = [];

    /// <summary>
    /// Reused for the whole life of the control. Creating a menu per click and disposing it from
    /// its own <c>Closed</c> event tore the strip down while it was still delivering the click,
    /// which threw <see cref="ObjectDisposedException"/>.
    /// </summary>
    private readonly ContextMenuStrip _menu;

    private object? _selectedItem;
    private bool _isHovered;
    private bool _isOpen;

    public AppDropdown()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint
            | ControlStyles.SupportsTransparentBackColor,
            true);

        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Font = AppTheme.Fonts.Body;
        Size = new Size(200, 34);

        _menu = new ContextMenuStrip
        {
            BackColor = AppTheme.Colors.Surface,
            Font = Font,
            ForeColor = AppTheme.Colors.TextSecondary,
            Renderer = new ThemedToolStripRenderer(),
            ShowImageMargin = false
        };

        _menu.Closed += (_, _) =>
        {
            _isOpen = false;
            Invalidate();
        };
    }

    /// <summary>Raised when the selection changes, from the user or from <see cref="SetItems"/>.</summary>
    [Category("Behavior")]
    public event EventHandler? SelectedItemChanged;

    /// <summary>Converts an item into the text shown in the control and in the list.</summary>
    public Func<object, string> DisplayText { get; set; } = item => item?.ToString() ?? string.Empty;

    public string Placeholder { get; set; } = string.Empty;

    public IReadOnlyList<object> Items => _items;

    public object? SelectedItem
    {
        get => _selectedItem;
        set => Select(value, notify: true);
    }

    protected override Size DefaultSize => new(200, 34);

    /// <summary>Replaces the list and the selection without raising a change notification.</summary>
    public void SetItems(IEnumerable<object> items, object? selected)
    {
        _items.Clear();
        _items.AddRange(items);

        Select(selected, notify: false);
    }

    public void Clear() => SetItems([], null);

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
            return;

        Focus();
        Open();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Space or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Space or Keys.Enter:
                Open();
                e.Handled = true;
                break;

            case Keys.Up or Keys.Down when _items.Count > 0:
                var step = e.KeyCode == Keys.Down ? 1 : -1;
                var index = Math.Clamp(_items.IndexOf(_selectedItem!) + step, 0, _items.Count - 1);
                SelectedItem = _items[index];
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
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

        var colors = AppTheme.Colors;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        var background = !Enabled
            ? colors.Background
            : _isOpen || _isHovered ? colors.SurfaceHover : colors.SurfaceAlt;

        var border = !Enabled
            ? colors.BorderSubtle
            : _isOpen || Focused ? colors.Accent
            : _isHovered ? colors.TextMuted
            : colors.Border;

        graphics.FillRoundedRectangle(background, bounds, AppTheme.Radius.Small);
        graphics.DrawRoundedBorder(border, bounds, AppTheme.Radius.Small, _isOpen || Focused ? 1.6f : 1.2f);

        var hasSelection = _selectedItem is not null;
        var text = hasSelection ? DisplayText(_selectedItem!) : Placeholder;

        var textBounds = new Rectangle(
            AppTheme.Spacing.Medium,
            0,
            Math.Max(Width - ChevronSize - (AppTheme.Spacing.Medium * 2) - AppTheme.Spacing.Small, 10),
            Height);

        TextRenderer.DrawText(
            graphics,
            text,
            Font,
            textBounds,
            !Enabled ? colors.TextMuted : hasSelection ? colors.TextPrimary : colors.TextMuted,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

        Icons.Draw(
            graphics,
            IconKind.ChevronDown,
            new RectangleF(Width - ChevronSize - AppTheme.Spacing.Medium, (Height - ChevronSize) / 2f, ChevronSize, ChevronSize),
            Enabled ? colors.TextSecondary : colors.TextMuted,
            2.4f);
    }

    private void Select(object? item, bool notify)
    {
        if (Equals(_selectedItem, item))
        {
            Invalidate();
            return;
        }

        _selectedItem = item;
        Invalidate();

        if (notify)
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Open()
    {
        if (_isOpen || !Enabled || _items.Count == 0)
            return;

        _menu.Items.Clear();
        _menu.MinimumSize = new Size(Width, 0);

        foreach (var item in _items)
        {
            var entry = new ToolStripMenuItem(DisplayText(item))
            {
                Checked = Equals(item, _selectedItem),
                Tag = item
            };

            entry.Click += OnEntryClicked;

            _menu.Items.Add(entry);
        }

        _isOpen = true;
        Invalidate();

        _menu.Show(this, new Point(0, Height + 2));
    }

    private void OnEntryClicked(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem { Tag: { } value })
            SelectedItem = value;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _menu.Dispose();

        base.Dispose(disposing);
    }
}
