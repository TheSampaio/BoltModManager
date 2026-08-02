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
            Text = "Description"
        };

        _filesLabel = CreateFilesLabel();
        _filesGrid = CreateFilesGrid();
        _fileMenu = CreateFileMenu();
        _filesGrid.ContextMenuStrip = _fileMenu;

        _filesGrid.CellMouseClick += OnFileCellMouseClick;
        _filesGrid.CellMouseDown += OnFileCellMouseDown;
        _filesGrid.ColumnHeaderMouseClick += OnFileColumnHeaderMouseClick;
        _filesGrid.KeyDown += OnFileGridKeyDown;
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
        layout.Controls.Add(_filesGrid, 0, 3);
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
            Variant = ButtonVariant.Primary,
            Width = 132
        };

        var cancel = new AppButton
        {
            Dock = DockStyle.Right,
            Margin = new Padding(0, 0, AppTheme.Spacing.Small, 0),
            Text = "Cancel",
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
        var initialDirectory = ResolveInitialDirectory(gameRoot, files[0]);

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select or create the destination inside the game folder",
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

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file.DestinationPath);
            file.DestinationPath = relativeDirectory.Length == 0
                ? fileName
                : Path.Combine(relativeDirectory, fileName);
        }

        RefreshFileTree();
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
            || e.ColumnIndex != 1
            || _filesGrid.Rows[e.RowIndex].Tag is not FileTreeRow { FolderPath: not null } node
            || e.X > GetTreeGlyphRight(node.Depth))
        {
            return;
        }

        ToggleFolder(node.FolderPath);
    }

    private void ToggleFolder(string folderPath)
    {
        if (!_collapsedFolders.Add(folderPath))
            _collapsedFolders.Remove(folderPath);

        RefreshFileTree();
    }

    private static int GetTreeGlyphRight(int depth) => AppTheme.Spacing.Small + (depth * 18) + 28;

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

        RefreshFileTree();
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

    private TreeFolder BuildFileTree()
    {
        var root = new TreeFolder(string.Empty, string.Empty);

        foreach (var file in _files)
        {
            var directory = Path.GetDirectoryName(file.DestinationPath) ?? string.Empty;
            var current = root;
            var currentPath = string.Empty;

            foreach (var segment in directory.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries))
            {
                currentPath = currentPath.Length == 0 ? segment : Path.Combine(currentPath, segment);

                if (!current.Folders.TryGetValue(segment, out var child))
                {
                    child = new TreeFolder(segment, currentPath);
                    current.Folders.Add(segment, child);
                }

                current = child;
            }

            current.Files.Add(file);
        }

        return root;
    }

    private void AddFolderChildren(TreeFolder folder, int depth)
    {
        var folders = Order(folder.Folders.Values, child => child.Name);

        foreach (var child in folders)
        {
            var collapsed = _collapsedFolders.Contains(child.Path);
            var marker = collapsed ? "▸" : "▾";
            var displayPath = $"{Indent(depth)}{marker}  {child.Name}";
            var index = _filesGrid.Rows.Add(string.Empty, displayPath);
            var row = _filesGrid.Rows[index];

            row.Tag = new FileTreeRow(child.Path, depth, null);
            row.DefaultCellStyle.Font = AppTheme.Fonts.BodyStrong;
            row.DefaultCellStyle.ForeColor = AppTheme.Colors.TextPrimary;
            row.Cells[1].ToolTipText = child.Path;

            if (!collapsed)
                AddFolderChildren(child, depth + 1);
        }

        var files = Order(
            folder.Files,
            file => _sortColumnIndex == 0 ? file.SourcePath : Path.GetFileName(file.DestinationPath));

        foreach (var file in files)
        {
            var displayPath = $"{Indent(depth)}    {Path.GetFileName(file.DestinationPath)}";
            var index = _filesGrid.Rows.Add(file.SourcePath, displayPath);
            var row = _filesGrid.Rows[index];

            row.Tag = new FileTreeRow(null, depth, file);
            row.Cells[0].ToolTipText = file.SourcePath;
            row.Cells[1].ToolTipText = file.DestinationPath;
        }
    }

    private IEnumerable<T> Order<T>(IEnumerable<T> values, Func<T, string> keySelector)
    {
        var ordered = values.OrderBy(keySelector, StringComparer.OrdinalIgnoreCase);
        return _sortDirection == ListSortDirection.Ascending ? ordered : ordered.Reverse();
    }

    private static string Indent(int depth) => new(' ', depth * 4);

    private void UpdateFileCount() => _filesLabel.Text = $"Files ({_files.Count})";

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

            if (node.FolderPath is not null)
            {
                foreach (var file in _files.Where(file => IsInFolder(file.DestinationPath, node.FolderPath)))
                    selected.Add(file);
            }
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

    private static bool IsInFolder(string filePath, string folderPath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        return directory.Equals(folderPath, StringComparison.OrdinalIgnoreCase)
            || directory.StartsWith(folderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveInitialDirectory(string gameRoot, EditableFile file)
    {
        var relativeDirectory = Path.GetDirectoryName(file.DestinationPath);

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
        Text = label
    };

    private static Label CreateFilesLabel() => new()
    {
        BackColor = Color.Transparent,
        Dock = DockStyle.Fill,
        Font = AppTheme.Fonts.Heading,
        ForeColor = AppTheme.Colors.TextPrimary,
        Text = "Files",
        TextAlign = ContentAlignment.MiddleLeft
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
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
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
            HeaderText = "CURRENT PATH",
            Name = "SourcePath",
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.Programmatic,
            Width = 330
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            HeaderText = "MOD LAYOUT",
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

    private sealed class TreeFolder(string name, string path)
    {
        public string Name { get; } = name;

        public string Path { get; } = path;

        public Dictionary<string, TreeFolder> Folders { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<EditableFile> Files { get; } = [];
    }

    private sealed record FileTreeRow(string? FolderPath, int Depth, EditableFile? File);
}
