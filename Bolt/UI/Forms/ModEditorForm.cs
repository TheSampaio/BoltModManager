using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Storage;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>Edits metadata and deployment paths for one imported modification.</summary>
internal sealed class ModEditorForm : ThemedForm
{
    private const int ToolbarButtonHeight = 32;

    private static readonly HashSet<string> TextFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".markdown", ".ini", ".cfg", ".conf", ".config", ".json", ".xml",
        ".yaml", ".yml", ".toml", ".log", ".csv", ".properties", ".bat", ".cmd", ".ps1"
    };

    private readonly IModEditorService _editor;
    private readonly IDialogService _dialogs;
    private readonly IUserPreferencesService _preferences;
    private readonly ToolTip _toolTip = new();
    private readonly List<EditableFile> _files = [];
    private readonly HashSet<string> _collapsedFolders = new(StringComparer.OrdinalIgnoreCase);

    private readonly AppTextField _nameField;
    private readonly AppTextField _versionField;
    private readonly AppTextField _categoryField;
    private readonly AppMultilineTextField _descriptionField;
    private readonly DataGridView _filesGrid;
    private readonly Label _filesLabel;
    private readonly ContextMenuStrip _fileMenu;

    private AppButton _moveButton = null!;
    private AppButton _openButton = null!;
    private AppButton _resetButton = null!;
    private AppButton _removeButton = null!;

    private GameSession? _session;
    private Modification? _modification;
    private int _sortColumnIndex = 1;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;

    public ModEditorForm(
        IModEditorService editor,
        IDialogService dialogs,
        IUserPreferencesService preferences)
    {
        _editor = editor;
        _dialogs = dialogs;
        _preferences = preferences;

        Text = "Edit Modification";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(900, 680);
        Padding = new Padding(
            AppTheme.Spacing.XLarge,
            AppTheme.Spacing.Large,
            AppTheme.Spacing.XLarge,
            AppTheme.Spacing.Large);

        _nameField = CreateField("Name", "Modification name");
        _versionField = CreateField("Version", "For example, 1.2.0");
        _categoryField = CreateField("Category", "For example, Visuals");
        _descriptionField = new AppMultilineTextField
        {
            Dock = DockStyle.Fill,
            Placeholder = "Optional notes about this modification",
            Text = "Description",
            UseSectionLabelStyle = true
        };

        _nameField.TabIndex = 0;
        _versionField.TabIndex = 1;
        _categoryField.TabIndex = 2;
        _descriptionField.TabIndex = 3;

        _filesLabel = CreateFilesLabel();
        _filesGrid = CreateFilesGrid();
        _fileMenu = CreateFileMenu();
        _filesGrid.ContextMenuStrip = _fileMenu;

        _filesGrid.CellMouseClick += OnFileCellMouseClick;
        _filesGrid.CellMouseDown += OnFileCellMouseDown;
        _filesGrid.ColumnHeaderMouseClick += OnFileColumnHeaderMouseClick;
        _filesGrid.CellPainting += OnFileCellPainting;
        _filesGrid.KeyDown += OnFileGridKeyDown;
        _filesGrid.Paint += OnFilesGridPaint;
        _filesGrid.RowPostPaint += OnFileRowPostPaint;
        _filesGrid.SelectionChanged += (_, _) => UpdateFileActions();
        _fileMenu.Opening += OnFileMenuOpening;

        Controls.Add(BuildLayout());
    }

    /// <summary>Loads <paramref name="modification"/> and opens the editor as a modal window.</summary>
    public DialogResult ShowEditor(IWin32Window owner, GameSession session, Modification modification)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(modification);

        _session = session;
        _modification = modification;

        _nameField.Value = modification.Name;
        _versionField.Value = modification.Version;
        _categoryField.Value = modification.Category;
        _descriptionField.Value = modification.Description;

        SetGridFiles(modification.Content);

        return ShowDialog(owner);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _nameField.Focus();
    }

    private TableLayoutPanel BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 5
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));

        layout.Controls.Add(CreateIntroduction(), 0, 0);
        layout.Controls.Add(BuildMetadata(), 0, 1);
        layout.Controls.Add(BuildFileToolbar(), 0, 2);
        var filesFrame = new GridFrame { Dock = DockStyle.Fill };
        filesFrame.Controls.Add(_filesGrid);

        layout.Controls.Add(filesFrame, 0, 3);
        layout.Controls.Add(BuildActions(), 0, 4);

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
        Text = "Choose destinations inside the game folder. Bolt mirrors those paths in the mod source so deployment matches the original game files.",
        TextAlign = ContentAlignment.TopLeft
    };

    private TableLayoutPanel BuildMetadata()
    {
        var metadata = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            RowCount = 2
        };

        metadata.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        metadata.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        metadata.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        metadata.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
        metadata.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        metadata.Controls.Add(_nameField, 0, 0);
        metadata.Controls.Add(_versionField, 1, 0);
        metadata.Controls.Add(_categoryField, 2, 0);
        metadata.Controls.Add(_descriptionField, 0, 1);
        metadata.SetColumnSpan(_descriptionField, 3);

        _nameField.Margin = new Padding(0, 0, AppTheme.Spacing.Medium, AppTheme.Spacing.Small);
        _versionField.Margin = new Padding(0, 0, AppTheme.Spacing.Medium, AppTheme.Spacing.Small);
        _categoryField.Margin = new Padding(0, 0, 0, AppTheme.Spacing.Small);
        _descriptionField.Margin = Padding.Empty;

        return metadata;
    }

    private Panel BuildFileToolbar()
    {
        _openButton = CreateToolbarButton(string.Empty, 34, IconKind.Document, OnOpenSelected, ButtonVariant.Ghost);
        _moveButton = CreateToolbarButton("Move", 94, IconKind.Folder, OnMoveSelected);
        _resetButton = CreateToolbarButton(string.Empty, 34, IconKind.Reset, OnResetSelected, ButtonVariant.Ghost);
        _removeButton = CreateToolbarButton(
            string.Empty,
            34,
            IconKind.Trash,
            OnRemoveSelected,
            ButtonVariant.Danger);

        _openButton.IconColor = AppTheme.Colors.AccentText;
        _resetButton.IconColor = AppTheme.Colors.AccentText;
        _resetButton.TabIndex = 0;
        _openButton.TabIndex = 1;
        _removeButton.TabIndex = 2;
        _moveButton.TabIndex = 3;

        _toolTip.SetToolTip(_openButton, "Open the selected text file in the configured editor");
        _toolTip.SetToolTip(_moveButton, "Choose a game folder for the selected files");
        _toolTip.SetToolTip(_resetButton, "Reset the selected files to their original paths");
        _toolTip.SetToolTip(_removeButton, "Exclude the selected files from this modification");

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        // The group is right-aligned. Left-to-right insertion produces the requested visual order
        // from the right edge: Move, Remove, Read, Reset.
        foreach (var button in new[] { _resetButton, _openButton, _removeButton, _moveButton })
        {
            button.Margin = new Padding(AppTheme.Spacing.Small, 0, 0, 0);
            actions.Controls.Add(button);
        }

        var panel = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, AppTheme.Spacing.Small, 0, AppTheme.Spacing.Tiny)
        };

        panel.Controls.Add(actions);
        panel.Controls.Add(_filesLabel);
        return panel;
    }

    private Panel BuildActions()
    {
        var save = new AppButton
        {
            Dock = DockStyle.Right,
            Text = "Save changes",
            TabIndex = 1,
            Variant = ButtonVariant.Primary,
            Width = 132
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

        _toolTip.SetToolTip(save, "Apply metadata and file-layout changes");
        _toolTip.SetToolTip(cancel, "Close without saving changes");

        save.Click += OnSaveClicked;
        cancel.Click += (_, _) => Close();

        var panel = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, AppTheme.Spacing.Small, 0, 0)
        };

        panel.Controls.AddRange([cancel, save]);
        return panel;
    }

    private ContextMenuStrip CreateFileMenu()
    {
        var open = new ToolStripMenuItem("Open in text editor");
        var move = new ToolStripMenuItem("Move…");
        var reset = new ToolStripMenuItem("Reset");
        var delete = new ToolStripMenuItem("Delete") { ShortcutKeyDisplayString = "Del" };

        open.Click += OnOpenSelected;
        move.Click += OnMoveSelected;
        reset.Click += OnResetSelected;
        delete.Click += OnRemoveSelected;

        var menu = new ContextMenuStrip
        {
            BackColor = AppTheme.Colors.Surface,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Renderer = new ThemedToolStripRenderer(),
            ShowImageMargin = false,
            Tag = new FileMenuItems(open, move, reset, delete)
        };

        menu.Items.AddRange([
            open,
            new ToolStripSeparator(),
            move,
            reset,
            new ToolStripSeparator(),
            delete
        ]);
        return menu;
    }

    private void OnFileMenuOpening(object? sender, CancelEventArgs e)
    {
        if (_fileMenu.Tag is not FileMenuItems items)
            return;

        var files = SelectedFiles();
        items.Open.Enabled = GetSingleSelectedTextFile() is not null;
        items.Move.Enabled = files.Count > 0;
        items.Reset.Enabled = files.Any(file =>
            !file.SourcePath.Equals(file.DestinationPath, StringComparison.Ordinal));
        items.Delete.Enabled = files.Count > 0;

        e.Cancel = files.Count == 0;
    }

    private void OnMoveSelected(object? sender, EventArgs e)
    {
        var rows = SelectedRows();
        var files = SelectedFiles();

        if (files.Count == 0)
        {
            _dialogs.Warning("Select one or more files first.", "Move Files");
            return;
        }

        if (_session is null || !Directory.Exists(_session.Game.TargetPath))
        {
            _dialogs.Warning("The game folder no longer exists.", "Move Files");
            return;
        }

        var gameRoot = Path.GetFullPath(_session.Game.TargetPath);
        var selectedFolder = rows
            .Select(row => row.Tag)
            .OfType<FileTreeRow>()
            .FirstOrDefault(node => node.Folder is not null)
            ?.Folder;
        var initialDirectory = ResolveInitialDirectory(gameRoot, files[0], selectedFolder);
        var hasFolderSelection = selectedFolder is not null;

        using var dialog = new FolderBrowserDialog
        {
            Description = hasFolderSelection
                ? "Select or create the parent destination for the selected folder(s)"
                : "Select or create the destination inside the game folder",
            InitialDirectory = initialDirectory,
            RootFolder = Environment.SpecialFolder.MyComputer,
            SelectedPath = initialDirectory,
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var selectedDirectory = Path.GetFullPath(dialog.SelectedPath);

        if (!PathUtility.IsInside(gameRoot, selectedDirectory))
        {
            _dialogs.Warning("Choose a destination inside the game folder.", "Move Files");
            return;
        }

        var relativeDirectory = Path.GetRelativePath(gameRoot, selectedDirectory);

        if (relativeDirectory == ".")
            relativeDirectory = string.Empty;

        MoveSelectedRows(rows, relativeDirectory);

        RefreshFileTree();
    }

    private static void MoveSelectedRows(IReadOnlyList<DataGridViewRow> rows, string destinationDirectory)
    {
        var moved = new HashSet<EditableFile>();
        var nodes = rows
            .Select(row => row.Tag)
            .OfType<FileTreeRow>()
            .ToList();

        foreach (var node in nodes.Where(node => node.Folder is not null).OrderBy(node => node.Depth))
        {
            var folder = node.Folder!;
            var useSourceTree = folder.SourcePath.Length > 0;
            var folderPath = useSourceTree ? folder.SourcePath : folder.DestinationPath;

            foreach (var file in folder.DescendantFiles)
            {
                if (!moved.Add(file))
                    continue;

                var filePath = useSourceTree ? file.SourcePath : file.DestinationPath;
                file.DestinationPath = PathUtility.RebaseFolderFile(
                    filePath,
                    folderPath,
                    destinationDirectory);
            }
        }

        foreach (var node in nodes.Where(node => node.File is not null))
        {
            var file = node.File!;

            if (!moved.Add(file))
                continue;

            var fileName = Path.GetFileName(file.DestinationPath);
            file.DestinationPath = destinationDirectory.Length == 0
                ? fileName
                : Path.Combine(destinationDirectory, fileName);
        }
    }

    private void OnResetSelected(object? sender, EventArgs e)
    {
        var files = SelectedFiles();

        foreach (var file in files)
            file.DestinationPath = file.SourcePath;

        RefreshFileTree();
    }

    private void OnRemoveSelected(object? sender, EventArgs e)
    {
        var files = SelectedFiles();

        if (files.Count == 0)
        {
            _dialogs.Warning("Select one or more files first.", "Remove Files");
            return;
        }

        var prompt = files.Count == 1
            ? $"Remove \"{files[0].SourcePath}\" from this modification?"
            : $"Remove {files.Count} files from this modification?";

        if (!_dialogs.Confirm(prompt, "Remove Files", destructive: true))
            return;

        foreach (var file in files)
            _files.Remove(file);

        RefreshFileTree();
    }

    private void OnOpenSelected(object? sender, EventArgs e)
    {
        var file = GetSingleSelectedTextFile();

        if (file is null || _session is null || _modification is null)
            return;

        var sourcePath = Path.Combine(_session.GetModificationPath(_modification), file.SourcePath);

        if (!File.Exists(sourcePath))
        {
            _dialogs.Warning("The selected file no longer exists in the modification source.", "Open Text File");
            return;
        }

        var editorPath = string.IsNullOrWhiteSpace(_preferences.Current.TextEditorPath)
            ? "notepad.exe"
            : _preferences.Current.TextEditorPath;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = editorPath,
                UseShellExecute = true
            };

            startInfo.ArgumentList.Add(sourcePath);
            Process.Start(startInfo)?.Dispose();
        }
        catch (Exception ex) when (ex is Win32Exception or IOException)
        {
            _dialogs.Error($"The text editor could not be opened:\n{ex.Message}", "Open Text File");
        }
    }

    private void OnFileCellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right || e.RowIndex < 0)
            return;

        var row = _filesGrid.Rows[e.RowIndex];

        if (!row.Selected)
        {
            _filesGrid.ClearSelection();
            row.Selected = true;
        }

        _filesGrid.CurrentCell = row.Cells[Math.Max(e.ColumnIndex, 0)];
    }

    private void OnFileCellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left
            || e.RowIndex < 0
            || e.ColumnIndex < 0
            || _filesGrid.Rows[e.RowIndex].Tag is not FileTreeRow { Folder: not null } node
            || GetFolderName(node.Folder, e.ColumnIndex).Length == 0
            || e.X > GetTreeGlyphRight(node.Depth))
        {
            return;
        }

        ToggleFolder(node.Folder.Key);
    }

    private void ToggleFolder(string folderPath)
    {
        if (!_collapsedFolders.Add(folderPath))
            _collapsedFolders.Remove(folderPath);

        RefreshFileTree();
    }

    private static int GetTreeGlyphRight(int depth) => AppTheme.Spacing.Small + (depth * 18) + 28;

    private static string GetFolderName(MappingFolder folder, int columnIndex) =>
        columnIndex == 0 ? folder.SourceName : folder.DestinationName;

    private void OnFileGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Delete)
            return;

        OnRemoveSelected(sender, EventArgs.Empty);
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private static void OnFileRowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
    {
        if (sender is not DataGridView grid || !grid.Rows[e.RowIndex].Selected)
            return;

        using var accent = new SolidBrush(AppTheme.Colors.Accent);
        e.Graphics.FillRectangle(accent, e.RowBounds.Left, e.RowBounds.Top, 3, e.RowBounds.Height);
    }

    private void OnFileCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0
            || e.ColumnIndex < 0
            || _filesGrid.Rows[e.RowIndex].Tag is not FileTreeRow node)
        {
            return;
        }

        e.PaintBackground(e.CellBounds, true);

        if (node.Folder is not null)
            PaintFolderCell(e, node);
        else if (node.File is not null)
            PaintFileCell(e, node);

        e.Handled = true;
    }

    private void PaintFolderCell(DataGridViewCellPaintingEventArgs e, FileTreeRow node)
    {
        var folder = node.Folder!;
        var name = GetFolderName(folder, e.ColumnIndex);

        if (name.Length == 0)
            return;

        var left = e.CellBounds.Left + AppTheme.Spacing.Small + (node.Depth * 18);
        var centerY = e.CellBounds.Top + (e.CellBounds.Height / 2f);
        var collapsed = _collapsedFolders.Contains(folder.Key);

        DrawChevron(e.Graphics!, new PointF(left + 5, centerY), collapsed);
        Icons.Draw(
            e.Graphics!,
            IconKind.Folder,
            new RectangleF(left + 16, centerY - 7, 14, 14),
            AppTheme.Colors.TextSecondary,
            1.7f);

        var textLeft = left + 36;
        var textBounds = new Rectangle(
            textLeft,
            e.CellBounds.Top,
            Math.Max(e.CellBounds.Right - textLeft - AppTheme.Spacing.Small, 0),
            e.CellBounds.Height);
        var nameSize = TextRenderer.MeasureText(
            name,
            AppTheme.Fonts.BodyStrong,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

        TextRenderer.DrawText(
            e.Graphics!,
            name,
            AppTheme.Fonts.BodyStrong,
            textBounds,
            AppTheme.Colors.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

        var countLeft = textLeft + nameSize.Width + AppTheme.Spacing.Small;

        if (countLeft < e.CellBounds.Right - 32)
        {
            var count = folder.DescendantFiles.Count == 1
                ? "1 file"
                : $"{folder.DescendantFiles.Count} files";

            TextRenderer.DrawText(
                e.Graphics!,
                count,
                AppTheme.Fonts.Caption,
                new Rectangle(countLeft, e.CellBounds.Top, e.CellBounds.Right - countLeft, e.CellBounds.Height),
                AppTheme.Colors.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }

    private static void PaintFileCell(DataGridViewCellPaintingEventArgs e, FileTreeRow node)
    {
        var file = node.File!;
        var name = e.ColumnIndex == 0
            ? Path.GetFileName(file.SourcePath)
            : Path.GetFileName(file.DestinationPath);
        var left = e.CellBounds.Left + AppTheme.Spacing.Small + (node.Depth * 18) + 16;
        var centerY = e.CellBounds.Top + (e.CellBounds.Height / 2f);
        var changed = e.ColumnIndex == 1
            && !file.SourcePath.Equals(file.DestinationPath, StringComparison.OrdinalIgnoreCase);

        Icons.Draw(
            e.Graphics!,
            IconKind.Document,
            new RectangleF(left, centerY - 7, 14, 14),
            changed ? AppTheme.Colors.AccentText : AppTheme.Colors.TextMuted,
            1.6f);

        TextRenderer.DrawText(
            e.Graphics!,
            name,
            AppTheme.Fonts.Body,
            new Rectangle(left + 21, e.CellBounds.Top, Math.Max(e.CellBounds.Right - left - 27, 0), e.CellBounds.Height),
            changed ? AppTheme.Colors.AccentText : AppTheme.Colors.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
    }

    private static void DrawChevron(Graphics graphics, PointF center, bool collapsed)
    {
        graphics.UseHighQuality();

        using var pen = new Pen(AppTheme.Colors.TextMuted, 1.5f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };

        if (collapsed)
        {
            graphics.DrawLines(pen, [
                new PointF(center.X - 2, center.Y - 4),
                new PointF(center.X + 2, center.Y),
                new PointF(center.X - 2, center.Y + 4)]);
        }
        else
        {
            graphics.DrawLines(pen, [
                new PointF(center.X - 4, center.Y - 2),
                new PointF(center.X, center.Y + 2),
                new PointF(center.X + 4, center.Y - 2)]);
        }
    }

    private static void OnFilesGridPaint(object? sender, PaintEventArgs e)
    {
        if (sender is not DataGridView { Columns.Count: > 1 } grid)
            return;

        var dividerX = grid.Columns[0].Width - grid.HorizontalScrollingOffset;
        using var divider = new Pen(GridFrame.BorderColor);
        e.Graphics.DrawLine(divider, dividerX, 0, dividerX, grid.ClientSize.Height);
    }

    private void OnFileColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex == _sortColumnIndex)
        {
            _sortDirection = _sortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            _sortColumnIndex = e.ColumnIndex;
            _sortDirection = ListSortDirection.Ascending;
        }

        RefreshFileTree();
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_session is null || _modification is null)
            return;

        var files = _files
            .Select(file => new ModFileEdit(file.SourcePath, file.DestinationPath))
            .ToList();

        var edit = new ModificationEdit(
            _nameField.Value,
            _descriptionField.Value,
            _versionField.Value,
            _categoryField.Value,
            files);

        var result = _editor.Apply(_session, _modification, edit);

        if (result.Failed)
        {
            _dialogs.Error(result.Error!, "Edit Modification");
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void SetGridFiles(IEnumerable<string> files)
    {
        _files.Clear();
        _collapsedFolders.Clear();

        foreach (var file in files)
            _files.Add(new EditableFile(file, file));

        CollapseAllFolders(BuildFileTree());
        RefreshFileTree();
    }

    private void CollapseAllFolders(MappingFolder folder)
    {
        foreach (var child in folder.Folders.Values)
        {
            _collapsedFolders.Add(child.Key);
            CollapseAllFolders(child);
        }
    }

    private void RefreshFileTree()
    {
        _filesGrid.SuspendLayout();
        _filesGrid.Rows.Clear();

        foreach (DataGridViewColumn column in _filesGrid.Columns)
            column.HeaderCell.SortGlyphDirection = SortOrder.None;

        var sortOrder = _sortDirection == ListSortDirection.Ascending
            ? SortOrder.Ascending
            : SortOrder.Descending;

        _filesGrid.Columns[_sortColumnIndex].HeaderCell.SortGlyphDirection = sortOrder;

        var root = BuildFileTree();
        AddFolderChildren(root, depth: 0);

        _filesGrid.ResumeLayout();
        UpdateFileCount();
        UpdateFileActions();
    }

    private MappingFolder BuildFileTree()
    {
        var root = new MappingFolder(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        foreach (var file in _files)
        {
            var sourceSegments = GetDirectorySegments(file.SourcePath);
            var destinationSegments = GetDirectorySegments(file.DestinationPath);
            var current = root;
            var sourcePath = string.Empty;
            var destinationPath = string.Empty;
            var depth = Math.Max(sourceSegments.Length, destinationSegments.Length);
            var sourceOffset = depth - sourceSegments.Length;
            var destinationOffset = depth - destinationSegments.Length;

            for (var index = 0; index < depth; index++)
            {
                var sourceIndex = index - sourceOffset;
                var destinationIndex = index - destinationOffset;
                var sourceName = sourceIndex >= 0 ? sourceSegments[sourceIndex] : string.Empty;
                var destinationName = destinationIndex >= 0 ? destinationSegments[destinationIndex] : string.Empty;
                sourcePath = AppendPath(sourcePath, sourceName);
                destinationPath = AppendPath(destinationPath, destinationName);
                var localKey = $"{sourceName}\u001F{destinationName}";

                if (!current.Folders.TryGetValue(localKey, out var child))
                {
                    child = new MappingFolder(
                        $"{current.Key}\u001E{localKey}",
                        sourceName,
                        destinationName,
                        sourcePath,
                        destinationPath);
                    current.Folders.Add(localKey, child);
                }

                current = child;
                current.DescendantFiles.Add(file);
            }

            current.DirectFiles.Add(file);
        }

        return root;
    }

    private void AddFolderChildren(MappingFolder folder, int depth)
    {
        var folders = Order(
            folder.Folders.Values,
            child => _sortColumnIndex == 0
                ? FirstNonEmpty(child.SourceName, child.DestinationName)
                : FirstNonEmpty(child.DestinationName, child.SourceName));

        foreach (var child in folders)
        {
            var collapsed = _collapsedFolders.Contains(child.Key);
            var index = _filesGrid.Rows.Add(child.SourceName, child.DestinationName);
            var row = _filesGrid.Rows[index];

            row.Tag = new FileTreeRow(child, depth, null);
            row.Cells[0].ToolTipText = child.SourcePath;
            row.Cells[1].ToolTipText = child.DestinationPath;

            if (!collapsed)
                AddFolderChildren(child, depth + 1);
        }

        var files = Order(
            folder.DirectFiles,
            file => _sortColumnIndex == 0
                ? Path.GetFileName(file.SourcePath)
                : Path.GetFileName(file.DestinationPath));

        foreach (var file in files)
        {
            var index = _filesGrid.Rows.Add(
                Path.GetFileName(file.SourcePath),
                Path.GetFileName(file.DestinationPath));
            var row = _filesGrid.Rows[index];

            row.Tag = new FileTreeRow(null, depth, file);
            row.Cells[0].ToolTipText = file.SourcePath;
            row.Cells[1].ToolTipText = file.DestinationPath;
        }
    }

    private static string[] GetDirectorySegments(string filePath) =>
        (Path.GetDirectoryName(filePath) ?? string.Empty).Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

    private static string AppendPath(string path, string segment) => segment.Length == 0
        ? path
        : path.Length == 0 ? segment : Path.Combine(path, segment);

    private static string FirstNonEmpty(string preferred, string fallback) =>
        preferred.Length > 0 ? preferred : fallback;

    private IEnumerable<T> Order<T>(IEnumerable<T> values, Func<T, string> keySelector)
    {
        var ordered = values.OrderBy(keySelector, StringComparer.OrdinalIgnoreCase);
        return _sortDirection == ListSortDirection.Ascending ? ordered : ordered.Reverse();
    }

    private void UpdateFileCount() => _filesLabel.Text = $"FILES ({_files.Count})";

    private void UpdateFileActions()
    {
        var files = SelectedFiles();
        _openButton.Enabled = GetSingleSelectedTextFile() is not null;
        _moveButton.Enabled = files.Count > 0;
        _resetButton.Enabled = files.Any(file =>
            !file.SourcePath.Equals(file.DestinationPath, StringComparison.Ordinal));
        _removeButton.Enabled = files.Count > 0;
    }

    private IReadOnlyList<DataGridViewRow> SelectedRows() =>
        [.. _filesGrid.SelectedRows.Cast<DataGridViewRow>()];

    private IReadOnlyList<EditableFile> SelectedFiles()
    {
        var selected = new HashSet<EditableFile>();

        foreach (var row in SelectedRows())
        {
            if (row.Tag is not FileTreeRow node)
                continue;

            if (node.File is not null)
            {
                selected.Add(node.File);
                continue;
            }

            if (node.Folder is not null)
                selected.UnionWith(node.Folder.DescendantFiles);
        }

        return [.. selected];
    }

    private EditableFile? GetSingleSelectedTextFile()
    {
        var rows = SelectedRows();

        if (rows.Count != 1 || rows[0].Tag is not FileTreeRow { File: not null } node)
            return null;

        return TextFileExtensions.Contains(Path.GetExtension(node.File.DestinationPath))
            ? node.File
            : null;
    }

    private static string ResolveInitialDirectory(
        string gameRoot,
        EditableFile file,
        MappingFolder? selectedFolder)
    {
        var relativeDirectory = selectedFolder is null
            ? Path.GetDirectoryName(file.DestinationPath)
            : Path.GetDirectoryName(selectedFolder.DestinationPath);

        if (string.IsNullOrEmpty(relativeDirectory))
            return gameRoot;

        var candidate = Path.GetFullPath(Path.Combine(gameRoot, relativeDirectory));

        return PathUtility.IsInside(gameRoot, candidate) && Directory.Exists(candidate)
            ? candidate
            : gameRoot;
    }

    private static AppTextField CreateField(string label, string placeholder) => new()
    {
        Dock = DockStyle.Fill,
        Placeholder = placeholder,
        Text = label,
        UseSectionLabelStyle = true
    };

    private static Label CreateFilesLabel() => new()
    {
        BackColor = Color.Transparent,
        Dock = DockStyle.Fill,
        Font = AppTheme.Fonts.Overline,
        ForeColor = AppTheme.Colors.TextMuted,
        Text = "FILES",
        TextAlign = ContentAlignment.BottomLeft
    };

    private static DataGridView CreateFilesGrid()
    {
        var grid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            BackgroundColor = AppTheme.Colors.Surface,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.None,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ColumnHeadersHeight = 34,
            Dock = DockStyle.Fill,
            EditMode = DataGridViewEditMode.EditProgrammatically,
            EnableHeadersVisualStyles = false,
            GridColor = AppTheme.Colors.BorderSubtle,
            MultiSelect = true,
            RowHeadersVisible = false,
            RowTemplate = { Height = 34 },
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

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 50f,
            HeaderText = "Current Path (source)",
            Name = "SourcePath",
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 50f,
            HeaderText = "Mod Layout (destination in game folder)",
            Name = "DestinationPath",
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });

        return grid;
    }

    private static AppButton CreateToolbarButton(
        string text,
        int width,
        IconKind icon,
        EventHandler onClick,
        ButtonVariant variant = ButtonVariant.Secondary)
    {
        var button = new AppButton
        {
            Height = ToolbarButtonHeight,
            Icon = icon,
            IconSize = 14,
            Text = text,
            Variant = variant,
            Width = width
        };

        button.Click += onClick;
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
            _fileMenu.Dispose();
        }

        base.Dispose(disposing);
    }

    private sealed record FileMenuItems(
        ToolStripMenuItem Open,
        ToolStripMenuItem Move,
        ToolStripMenuItem Reset,
        ToolStripMenuItem Delete);

    private sealed class EditableFile(string sourcePath, string destinationPath)
    {
        public string SourcePath { get; } = sourcePath;

        public string DestinationPath { get; set; } = destinationPath;
    }

    private sealed class MappingFolder(
        string key,
        string sourceName,
        string destinationName,
        string sourcePath,
        string destinationPath)
    {
        public string Key { get; } = key;

        public string SourceName { get; } = sourceName;

        public string DestinationName { get; } = destinationName;

        public string SourcePath { get; } = sourcePath;

        public string DestinationPath { get; } = destinationPath;

        public Dictionary<string, MappingFolder> Folders { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<EditableFile> DirectFiles { get; } = [];

        public List<EditableFile> DescendantFiles { get; } = [];
    }

    private sealed record FileTreeRow(MappingFolder? Folder, int Depth, EditableFile? File);
}
