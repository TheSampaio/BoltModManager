using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Bolt.Core.Models;
using Bolt.Infrastructure.Native;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>
/// Owner drawn list of modifications with a custom toggle column, hover feedback and an empty state.
/// </summary>
/// <remarks>
/// The native check boxes are replaced by a toggle drawn in the first column. The old
/// <c>ItemCheck</c> based flow ran the whole deployment inside an event that fires <em>before</em>
/// the state changes, which forced the handler to undo the check on failure; here a click simply
/// asks the form to perform the change and the list is refreshed afterwards.
/// </remarks>
internal sealed class ModificationListView : ListView
{
    private const int ToggleColumnWidth = 44;
    private const int ToggleSize = 18;
    private const int RowHeight = 34;

    private int _hoveredIndex = -1;
    private int _sortColumnIndex = 5;
    private ListSortDirection _sortDirection = ListSortDirection.Descending;

    public ModificationListView()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

        View = View.Details;
        FullRowSelect = true;
        HeaderStyle = ColumnHeaderStyle.Clickable;
        BorderStyle = BorderStyle.None;
        MultiSelect = true;
        OwnerDraw = true;
        UseCompatibleStateImageBehavior = false;
        BackColor = AppTheme.Colors.Surface;
        ForeColor = AppTheme.Colors.TextPrimary;
        Font = AppTheme.Fonts.Body;

        // A transparent image list is the supported way of forcing a taller row in Details view.
        SmallImageList = new ImageList { ImageSize = new Size(1, RowHeight) };

        Columns.AddRange([
            new ColumnHeader { Text = string.Empty, Width = ToggleColumnWidth },
            new ColumnHeader { Text = "Name", Width = 260 },
            new ColumnHeader { Text = "Version", Width = 90 },
            new ColumnHeader { Text = "Category", Width = 120 },
            new ColumnHeader { Text = "Files", Width = 70, TextAlign = HorizontalAlignment.Right },
            new ColumnHeader { Text = "Imported", Width = 140 }
        ]);
    }

    /// <summary>Raised when the user clicks the toggle of a row.</summary>
    [Category("Behavior")]
    public event Action<Modification>? ToggleRequested;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // Rows and headers are owner drawn, but the scroll bars belong to the native control and
        // only follow the theme through uxtheme.
        NativeMethods.SetWindowTheme(
            Handle,
            AppTheme.Colors.IsDark ? "DarkMode_Explorer" : "Explorer",
            null);

        // Owner drawn rows over a non buffered native list flicker heavily while columns are being
        // resized; the control has its own double buffering for exactly this case.
        NativeMethods.SendMessage(
            Handle,
            NativeMethods.ListViewSetExtendedStyle,
            NativeMethods.ListViewExtendedStyleDoubleBuffer,
            NativeMethods.ListViewExtendedStyleDoubleBuffer);

        StretchLastColumn();
    }

    protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
    {
        base.OnColumnWidthChanged(e);

        // Only once the drag has finished. Reacting while it is in progress fights the user.
        if (e.ColumnIndex != Columns.Count - 1)
            StretchLastColumn();
    }

    protected override void OnColumnClick(ColumnClickEventArgs e)
    {
        base.OnColumnClick(e);

        if (e.Column == _sortColumnIndex)
        {
            _sortDirection = _sortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            _sortColumnIndex = e.Column;
            _sortDirection = ListSortDirection.Ascending;
        }

        var modifications = Items
            .Cast<ListViewItem>()
            .Select(item => (Modification)item.Tag!)
            .ToList();

        SetItems(modifications);
    }

    /// <summary>Message shown when the list holds no rows.</summary>
    public string EmptyMessage { get; set; } = "No modifications imported yet.";

    public string EmptyHint { get; set; } = "Use Import to add a package to this profile.";

    /// <summary>Modifications backing the currently selected rows, in display order.</summary>
    public IReadOnlyList<Modification> SelectedModifications =>
        [.. SelectedItems.Cast<ListViewItem>().Select(item => (Modification)item.Tag!)];

    /// <summary>Replaces the content of the list, preserving the selection when possible.</summary>
    public void SetItems(IEnumerable<Modification> modifications)
    {
        var selectedIds = SelectedModifications.Select(m => m.Id).ToHashSet();

        BeginUpdate();

        try
        {
            Items.Clear();

            foreach (var modification in SortModifications(modifications))
            {
                var item = new ListViewItem([
                    string.Empty,
                    modification.Name,
                    string.IsNullOrWhiteSpace(modification.Version) ? "—" : modification.Version,
                    string.IsNullOrWhiteSpace(modification.Category) ? "—" : modification.Category,
                    modification.Content.Count.ToString(CultureInfo.CurrentCulture),
                    modification.InstalledAt.ToString("g", CultureInfo.CurrentCulture)
                ])
                {
                    Tag = modification,
                    Selected = selectedIds.Contains(modification.Id)
                };

                Items.Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }

        Invalidate();
    }

    protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
    {
        var graphics = e.Graphics;

        using (var brush = new SolidBrush(AppTheme.Colors.SurfaceAlt))
            graphics.FillRectangle(brush, e.Bounds);

        using (var pen = new Pen(AppTheme.Colors.Border))
            graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

        if (string.IsNullOrEmpty(e.Header?.Text))
            return;

        var alignment = e.Header.TextAlign == HorizontalAlignment.Right
            ? TextFormatFlags.Right
            : TextFormatFlags.Left;

        var headerText = e.Header.Text.ToUpperInvariant();

        if (e.ColumnIndex == _sortColumnIndex)
            headerText += _sortDirection == ListSortDirection.Ascending ? "  ▲" : "  ▼";

        TextRenderer.DrawText(
            graphics,
            headerText,
            AppTheme.Fonts.Overline,
            Rectangle.Inflate(e.Bounds, -AppTheme.Spacing.Small, 0),
            AppTheme.Colors.TextMuted,
            alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private IEnumerable<Modification> SortModifications(IEnumerable<Modification> modifications) =>
        _sortColumnIndex switch
        {
            0 => OrderBy(modifications, modification => modification.IsEnabled),
            1 => OrderBy(modifications, modification => modification.Name, StringComparer.OrdinalIgnoreCase),
            2 => OrderBy(modifications, modification => modification.Version, StringComparer.OrdinalIgnoreCase),
            3 => OrderBy(modifications, modification => modification.Category, StringComparer.OrdinalIgnoreCase),
            4 => OrderBy(modifications, modification => modification.Content.Count),
            _ => OrderBy(modifications, modification => modification.InstalledAt)
        };

    private IEnumerable<Modification> OrderBy<TKey>(
        IEnumerable<Modification> modifications,
        Func<Modification, TKey> keySelector,
        IComparer<TKey>? comparer = null) =>
        _sortDirection == ListSortDirection.Ascending
            ? modifications.OrderBy(keySelector, comparer)
            : modifications.OrderByDescending(keySelector, comparer);

    protected override void OnDrawItem(DrawListViewItemEventArgs e)
    {
        using (var brush = new SolidBrush(GetRowBackground(e.ItemIndex, e.Item is { Selected: true })))
            e.Graphics.FillRectangle(brush, e.Bounds);

        e.DrawDefault = false;
    }

    protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
    {
        if (e.Item?.Tag is not Modification modification)
            return;

        // The visual style repaints the themed selection behind every sub item, so each cell has to
        // reclaim its background; otherwise the system accent shows through the row colour.
        using (var brush = new SolidBrush(GetRowBackground(e.ItemIndex, e.Item.Selected)))
            e.Graphics.FillRectangle(brush, e.Bounds);

        if (e.ColumnIndex == 0)
        {
            if (e.Item.Selected)
            {
                using var accent = new SolidBrush(AppTheme.Colors.Accent);
                e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Top, 3, e.Bounds.Height);
            }

            DrawToggle(e.Graphics, e.Bounds, modification.IsEnabled);
            return;
        }

        var isNameColumn = e.ColumnIndex == 1;

        var color = modification.IsEnabled
            ? isNameColumn ? AppTheme.Colors.TextPrimary : AppTheme.Colors.TextSecondary
            : AppTheme.Colors.TextMuted;

        var alignment = Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right
            ? TextFormatFlags.Right
            : TextFormatFlags.Left;

        TextRenderer.DrawText(
            e.Graphics,
            e.SubItem?.Text ?? string.Empty,
            isNameColumn ? AppTheme.Fonts.BodyStrong : AppTheme.Fonts.Body,
            Rectangle.Inflate(e.Bounds, -AppTheme.Spacing.Small, 0),
            color,
            alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    /// <summary>Background of a row, honouring selection, hover and the zebra striping.</summary>
    private Color GetRowBackground(int index, bool isSelected)
    {
        if (isSelected)
            return AppTheme.Colors.Selection;

        if (index == _hoveredIndex)
            return AppTheme.Colors.SurfaceHover;

        return index % 2 == 0 ? AppTheme.Colors.Surface : AppTheme.Colors.SurfaceAlt;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var index = GetItemAt(e.X, e.Y)?.Index ?? -1;

        if (index != _hoveredIndex)
        {
            _hoveredIndex = index;
            Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
            Invalidate();
        }

        base.OnMouseLeave(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (e.Button != MouseButtons.Left || e.X > ToggleColumnWidth)
            return;

        if (GetItemAt(e.X, e.Y)?.Tag is Modification modification)
            ToggleRequested?.Invoke(modification);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        StretchLastColumn();
    }

    /// <summary>
    /// Grows the last column to the full client width so the header has no leftover strip, which
    /// the native control would paint with the system colours instead of the theme.
    /// </summary>
    /// <remarks>
    /// Never called from painting: assigning a column width invalidates the control, and doing it
    /// from <c>WM_PAINT</c> turned every header drag into an endless repaint loop.
    /// </remarks>
    private void StretchLastColumn()
    {
        if (Columns.Count == 0 || !IsHandleCreated)
            return;

        var used = 0;

        for (var i = 0; i < Columns.Count - 1; i++)
            used += Columns[i].Width;

        var target = Math.Max(ClientSize.Width - used, 120);

        if (Columns[^1].Width != target)
            Columns[^1].Width = target;
    }

    protected override void WndProc(ref Message m)
    {
        const int WmPaint = 0x000F;

        base.WndProc(ref m);

        // A ListView is a native control: it never raises OnPaint, so everything the owner drawing
        // does not reach is finished here, once the control has painted itself.
        if (m.Msg != WmPaint)
            return;

        using var graphics = CreateGraphics();

        FillAreaBelowItems(graphics);

        if (Items.Count == 0)
            DrawEmptyState(graphics);
    }

    /// <summary>
    /// Repaints the space under the last row. Owner drawing only covers the rows themselves, so
    /// the visual style keeps drawing its column separators across the empty remainder.
    /// </summary>
    private void FillAreaBelowItems(Graphics graphics)
    {
        var top = Items.Count > 0
            ? Items[^1].Bounds.Bottom
            : GetHeaderHeight();

        if (top >= ClientSize.Height)
            return;

        using var brush = new SolidBrush(AppTheme.Colors.Surface);
        graphics.FillRectangle(brush, 0, top, ClientSize.Width, ClientSize.Height - top);
    }

    private int GetHeaderHeight() =>
        Items.Count > 0 ? Items[0].Bounds.Top : TextRenderer.MeasureText("Ag", AppTheme.Fonts.Overline).Height + 12;

    private void DrawEmptyState(Graphics graphics)
    {
        var area = ClientRectangle with { Y = ClientRectangle.Y + RowHeight };

        Icons.Draw(
            graphics,
            IconKind.Package,
            new RectangleF(area.X + ((area.Width - 44) / 2f), area.Y + (area.Height / 2f) - 60, 44, 44),
            AppTheme.Colors.TextMuted,
            1.6f);

        TextRenderer.DrawText(
            graphics,
            EmptyMessage,
            AppTheme.Fonts.Subtitle,
            area with { Y = area.Y + (area.Height / 2) - 4, Height = 26 },
            AppTheme.Colors.TextSecondary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

        TextRenderer.DrawText(
            graphics,
            EmptyHint,
            AppTheme.Fonts.Body,
            area with { Y = area.Y + (area.Height / 2) + 24, Height = 22 },
            AppTheme.Colors.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
    }

    private static void DrawToggle(Graphics graphics, Rectangle bounds, bool isEnabled)
    {
        graphics.UseHighQuality();

        var box = new Rectangle(
            bounds.X + ((bounds.Width - ToggleSize) / 2),
            bounds.Y + ((bounds.Height - ToggleSize) / 2),
            ToggleSize,
            ToggleSize);

        if (isEnabled)
        {
            graphics.FillRoundedRectangle(AppTheme.Colors.Accent, box, AppTheme.Radius.Small - 1);
            Icons.Draw(graphics, IconKind.Check, Rectangle.Inflate(box, -3, -3), AppTheme.Colors.OnAccent, 3f);
        }
        else
        {
            // An empty box drawn with the divider colour was practically invisible against the row,
            // so it gets its own fill plus a border with real contrast.
            graphics.FillRoundedRectangle(AppTheme.Colors.SurfaceActive, box, AppTheme.Radius.Small - 1);
            graphics.DrawRoundedBorder(AppTheme.Colors.TextMuted, box, AppTheme.Radius.Small - 1, 1.5f);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            SmallImageList?.Dispose();

        base.Dispose(disposing);
    }
}
