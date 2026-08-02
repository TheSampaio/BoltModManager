using System.Drawing;
using System.Windows.Forms;
using Bolt.Core.Models;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>Edits the relative deployment order of enabled modifications sharing game paths.</summary>
internal sealed class ConflictManagerForm : ThemedForm
{
    private const int LeftModificationColumn = 0;
    private const int LeftPositionColumn = 1;
    private const int FileColumn = 2;
    private const int RightPositionColumn = 3;
    private const int RightModificationColumn = 4;

    private readonly ToolTip _toolTip = new();
    private readonly Label _summaryLabel;
    private readonly DataGridView _conflictsGrid;
    private readonly ContextMenuStrip _positionMenu;

    private ModificationConflictOrder? _order;
    private int _hoveredPositionRow = -1;
    private int _hoveredPositionColumn = -1;
    private int _openPositionRow = -1;
    private int _openPositionColumn = -1;

    public ConflictManagerForm()
    {
        Text = "Manage Conflicts";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(980, 650);
        Padding = new Padding(
            AppTheme.Spacing.XLarge,
            AppTheme.Spacing.Large,
            AppTheme.Spacing.XLarge,
            AppTheme.Spacing.Large);

        _summaryLabel = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextMuted,
            TextAlign = ContentAlignment.TopLeft
        };
        _conflictsGrid = CreateConflictsGrid();
        _positionMenu = CreatePositionMenu();
        _conflictsGrid.CellClick += OnConflictCellClick;
        _conflictsGrid.CellMouseEnter += OnConflictCellMouseEnter;
        _conflictsGrid.CellMouseLeave += OnConflictCellMouseLeave;
        _conflictsGrid.CellPainting += OnConflictCellPainting;
        _conflictsGrid.KeyDown += OnConflictGridKeyDown;
        _positionMenu.Closed += OnPositionMenuClosed;

        Controls.Add(BuildLayout());
    }

    public IReadOnlyList<Guid> OrderedModificationIds =>
        _order?.ModificationIds ?? [];

    public DialogResult ShowManager(
        IWin32Window owner,
        Profile profile,
        IReadOnlyList<ModificationConflict> conflicts)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(conflicts);

        _order = new ModificationConflictOrder(profile.Modifications.Select(modification => modification.Id));
        PopulateConflicts(conflicts);

        var fileCount = conflicts.Sum(conflict => conflict.Files.Count);
        var fileLabel = fileCount == 1 ? "1 conflicting file" : $"{fileCount} conflicting files";
        var pairLabel = conflicts.Count == 1
            ? "1 modification pair"
            : $"{conflicts.Count} modification pairs";
        _summaryLabel.Text = $"{fileLabel} across {pairLabel}.";

        return ShowDialog(owner);
    }

    private TableLayoutPanel BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 4
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));

        layout.Controls.Add(CreateIntroduction(), 0, 0);

        var frame = new GridFrame { Dock = DockStyle.Fill };
        frame.Controls.Add(_conflictsGrid);
        layout.Controls.Add(frame, 0, 1);
        layout.Controls.Add(BuildLegend(), 0, 2);
        layout.Controls.Add(BuildActions(), 0, 3);

        foreach (Control child in layout.Controls)
            child.Margin = Padding.Empty;

        return layout;
    }

    private static Label CreateIntroduction() => new()
    {
        BackColor = Color.Transparent,
        Dock = DockStyle.Fill,
        Font = AppTheme.Fonts.Caption,
        ForeColor = AppTheme.Colors.TextMuted,
        Text = "Choose which modification loads first for every shared file. Changing either rule automatically updates the opposite side and every other row affected by the new order.",
        TextAlign = ContentAlignment.TopLeft
    };

    private Panel BuildLegend()
    {
        var explanation = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextSecondary,
            Text = "Before loads first and loses the shared file.  After loads last and wins it.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var summary = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Right,
            Width = 310
        };
        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.TextAlign = ContentAlignment.MiddleRight;
        summary.Controls.Add(_summaryLabel);

        var panel = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, AppTheme.Spacing.Small, 0, 0)
        };
        panel.Controls.AddRange([explanation, summary]);
        return panel;
    }

    private Panel BuildActions()
    {
        var apply = new AppButton
        {
            Dock = DockStyle.Right,
            Text = "Apply rules",
            TabIndex = 1,
            Variant = ButtonVariant.Primary,
            Width = 120
        };
        var cancel = new AppButton
        {
            Dock = DockStyle.Right,
            Margin = new Padding(0, 0, AppTheme.Spacing.Small, 0),
            Text = "Cancel",
            TabIndex = 0,
            Variant = ButtonVariant.AccentOutline,
            Width = 100
        };

        _toolTip.SetToolTip(apply, "Save this precedence and redeploy the active profile");
        _toolTip.SetToolTip(cancel, "Close without changing modification order");

        apply.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
        cancel.Click += (_, _) => Close();

        var panel = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, AppTheme.Spacing.Small, 0, 0)
        };
        panel.Controls.AddRange([cancel, apply]);
        return panel;
    }

    private void PopulateConflicts(IEnumerable<ModificationConflict> conflicts)
    {
        _conflictsGrid.Rows.Clear();

        foreach (var conflict in conflicts)
        {
            foreach (var file in conflict.Files)
            {
                var rowIndex = _conflictsGrid.Rows.Add(
                    conflict.LeftModificationName,
                    GetPosition(conflict.LeftModificationId, conflict.RightModificationId),
                    file,
                    GetPosition(conflict.RightModificationId, conflict.LeftModificationId),
                    conflict.RightModificationName);
                var row = _conflictsGrid.Rows[rowIndex];
                row.Tag = new ConflictGridRow(conflict);
                row.Cells[FileColumn].ToolTipText = file;
                row.Cells[LeftModificationColumn].ToolTipText = conflict.LeftModificationName;
                row.Cells[RightModificationColumn].ToolTipText = conflict.RightModificationName;
            }
        }

        _conflictsGrid.ClearSelection();
    }

    private ConflictPosition GetPosition(Guid modificationId, Guid relativeToId) =>
        _order?.GetPosition(modificationId, relativeToId)
        ?? throw new InvalidOperationException("The conflict manager has not been initialized.");

    private void ApplyPosition(int rowIndex, int columnIndex, ConflictPosition position)
    {
        if (_order is null
            || rowIndex < 0
            || !IsPositionColumn(columnIndex)
            || _conflictsGrid.Rows[rowIndex].Tag is not ConflictGridRow row)
        {
            return;
        }

        var conflict = row.Conflict;

        if (columnIndex == LeftPositionColumn)
        {
            _order.SetPosition(conflict.LeftModificationId, conflict.RightModificationId, position);
        }
        else
        {
            _order.SetPosition(conflict.RightModificationId, conflict.LeftModificationId, position);
        }

        RefreshRuleCells();
    }

    private void RefreshRuleCells()
    {
        foreach (DataGridViewRow gridRow in _conflictsGrid.Rows)
        {
            if (gridRow.Tag is not ConflictGridRow row)
                continue;

            var conflict = row.Conflict;
            gridRow.Cells[LeftPositionColumn].Value = GetPosition(
                conflict.LeftModificationId,
                conflict.RightModificationId);
            gridRow.Cells[RightPositionColumn].Value = GetPosition(
                conflict.RightModificationId,
                conflict.LeftModificationId);
        }

        _conflictsGrid.Invalidate();
    }

    private void OnConflictCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || !IsPositionColumn(e.ColumnIndex))
            return;

        _conflictsGrid.CurrentCell = _conflictsGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
        OpenPositionMenu(e.RowIndex, e.ColumnIndex);
    }

    private void OpenPositionMenu(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0
            || !IsPositionColumn(columnIndex)
            || _conflictsGrid.Rows[rowIndex].Cells[columnIndex].Value is not ConflictPosition selected)
        {
            return;
        }

        _positionMenu.Items.Clear();

        foreach (var position in Enum.GetValues<ConflictPosition>())
        {
            var item = new ToolStripMenuItem(position.ToString())
            {
                Checked = position == selected,
                Tag = position
            };
            item.Click += (_, _) => ApplyPosition(rowIndex, columnIndex, position);
            _positionMenu.Items.Add(item);
        }

        var cellBounds = _conflictsGrid.GetCellDisplayRectangle(columnIndex, rowIndex, cutOverflow: true);
        _positionMenu.MinimumSize = new Size(Math.Max(cellBounds.Width - 12, 80), 0);
        _openPositionRow = rowIndex;
        _openPositionColumn = columnIndex;
        _conflictsGrid.InvalidateCell(columnIndex, rowIndex);
        _positionMenu.Show(
            _conflictsGrid,
            new Point(cellBounds.Left + 6, cellBounds.Bottom - 2));
    }

    private void OnPositionMenuClosed(object? sender, ToolStripDropDownClosedEventArgs e)
    {
        var row = _openPositionRow;
        var column = _openPositionColumn;
        _openPositionRow = -1;
        _openPositionColumn = -1;

        if (row >= 0 && column >= 0)
            _conflictsGrid.InvalidateCell(column, row);
    }

    private void OnConflictCellMouseEnter(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || !IsPositionColumn(e.ColumnIndex))
            return;

        _hoveredPositionRow = e.RowIndex;
        _hoveredPositionColumn = e.ColumnIndex;
        _conflictsGrid.Cursor = Cursors.Hand;
        _conflictsGrid.InvalidateCell(e.ColumnIndex, e.RowIndex);
    }

    private void OnConflictCellMouseLeave(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex != _hoveredPositionRow || e.ColumnIndex != _hoveredPositionColumn)
            return;

        _hoveredPositionRow = -1;
        _hoveredPositionColumn = -1;
        _conflictsGrid.Cursor = Cursors.Default;

        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            _conflictsGrid.InvalidateCell(e.ColumnIndex, e.RowIndex);
    }

    private void OnConflictCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || !IsPositionColumn(e.ColumnIndex))
            return;

        e.PaintBackground(e.CellBounds, true);

        var colors = AppTheme.Colors;
        var hovered = e.RowIndex == _hoveredPositionRow && e.ColumnIndex == _hoveredPositionColumn;
        var open = e.RowIndex == _openPositionRow && e.ColumnIndex == _openPositionColumn;
        var bounds = Rectangle.Inflate(e.CellBounds, -6, -4);
        bounds.Width = Math.Max(bounds.Width - 1, 1);
        bounds.Height = Math.Max(bounds.Height - 1, 1);
        var graphics = e.Graphics!;

        graphics.UseHighQuality();
        graphics.FillRoundedRectangle(
            open || hovered ? colors.SurfaceHover : colors.SurfaceAlt,
            bounds,
            AppTheme.Radius.Small);
        graphics.DrawRoundedBorder(
            open ? colors.Accent : hovered ? colors.TextMuted : colors.Border,
            bounds,
            AppTheme.Radius.Small,
            open ? 1.6f : 1.2f);

        var text = e.FormattedValue?.ToString() ?? string.Empty;
        var chevronSize = 12;
        var textBounds = new Rectangle(
            bounds.Left + AppTheme.Spacing.Small,
            bounds.Top,
            Math.Max(bounds.Width - chevronSize - (AppTheme.Spacing.Small * 3), 10),
            bounds.Height);
        TextRenderer.DrawText(
            graphics,
            text,
            AppTheme.Fonts.Body,
            textBounds,
            colors.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        Icons.Draw(
            graphics,
            IconKind.ChevronDown,
            new RectangleF(
                bounds.Right - chevronSize - AppTheme.Spacing.Small,
                bounds.Top + ((bounds.Height - chevronSize) / 2f),
                chevronSize,
                chevronSize),
            colors.TextSecondary,
            2.2f);

        e.Handled = true;
    }

    private void OnConflictGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is not (Keys.Space or Keys.Enter)
            || _conflictsGrid.CurrentCell is not { RowIndex: >= 0 } cell
            || !IsPositionColumn(cell.ColumnIndex))
        {
            return;
        }

        OpenPositionMenu(cell.RowIndex, cell.ColumnIndex);
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private static bool IsPositionColumn(int columnIndex) =>
        columnIndex is LeftPositionColumn or RightPositionColumn;

    private static ContextMenuStrip CreatePositionMenu() => new()
    {
        BackColor = AppTheme.Colors.Surface,
        Font = AppTheme.Fonts.Body,
        ForeColor = AppTheme.Colors.TextSecondary,
        Renderer = new ThemedToolStripRenderer(),
        ShowImageMargin = false
    };

    private static DataGridView CreateConflictsGrid()
    {
        var grid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            BackgroundColor = AppTheme.Colors.Surface,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ColumnHeadersHeight = 36,
            Dock = DockStyle.Fill,
            EditMode = DataGridViewEditMode.EditProgrammatically,
            EnableHeadersVisualStyles = false,
            GridColor = AppTheme.Colors.BorderSubtle,
            MultiSelect = false,
            RowHeadersVisible = false,
            RowTemplate = { Height = 36 },
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            TabIndex = 0
        };

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.Colors.SurfaceAlt,
            Font = AppTheme.Fonts.Overline,
            ForeColor = AppTheme.Colors.TextMuted,
            SelectionBackColor = AppTheme.Colors.SurfaceAlt,
            SelectionForeColor = AppTheme.Colors.TextMuted
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.Colors.Surface,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Padding = new Padding(AppTheme.Spacing.Small, 0, AppTheme.Spacing.Small, 0),
            SelectionBackColor = AppTheme.Colors.Selection,
            SelectionForeColor = AppTheme.Colors.TextPrimary
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.Colors.SurfaceAlt,
            ForeColor = AppTheme.Colors.TextSecondary,
            SelectionBackColor = AppTheme.Colors.Selection,
            SelectionForeColor = AppTheme.Colors.TextPrimary
        };

        grid.Columns.Add(CreateTextColumn("Modification", 24f));
        grid.Columns.Add(CreatePositionColumn("Rule"));
        grid.Columns.Add(CreateTextColumn("Conflicting file", 52f));
        grid.Columns.Add(CreatePositionColumn("Rule"));
        grid.Columns.Add(CreateTextColumn("Modification", 24f));

        return grid;
    }

    private static DataGridViewTextBoxColumn CreateTextColumn(string header, float fillWeight) => new()
    {
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        FillWeight = fillWeight,
        HeaderText = header,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private static DataGridViewTextBoxColumn CreatePositionColumn(string header) => new()
    {
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        HeaderText = header,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable,
        ValueType = typeof(ConflictPosition),
        Width = 112
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
            _positionMenu.Dispose();
        }

        base.Dispose(disposing);
    }

    private sealed record ConflictGridRow(ModificationConflict Conflict);
}
