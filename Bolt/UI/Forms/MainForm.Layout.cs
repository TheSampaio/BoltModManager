using System.Drawing;
using System.Windows.Forms;
using Bolt.Core.Models;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>
/// Construction of the main window layout.
/// </summary>
/// <remarks>
/// The interface is built in code rather than by the designer: the custom controls are themed at
/// runtime and the layout is fully fluid, neither of which the designer serialiser handles well.
/// Behaviour lives in <c>MainForm.cs</c>; this file only describes the visual tree.
/// </remarks>
internal sealed partial class MainForm
{
    private const int SidebarWidth = 292;
    private const int ToolbarHeight = 44;
    private const int StatusBarHeight = 34;
    private const int ProgressRowHeight = 34;
    private const int SectionLabelHeight = 22;
    private const int ShortcutButtonHeight = 32;
    private const int ShortcutGap = AppTheme.Spacing.Medium;

    private readonly ToolTip _toolTip = new();

    private Panel _content = null!;
    private MenuStrip _menu = null!;
    private ToolStripMenuItem _recentMenuItem = null!;
    private ToolStripMenuItem _closeGameMenuItem = null!;
    private ToolStripMenuItem _restoreGameMenuItem = null!;

    private PictureBox _gameIcon = null!;
    private Label _gameName = null!;
    private Label _gameTarget = null!;
    private AppButton _playButton = null!;

    private AppDropdown _profileSelector = null!;
    private AppButton _addProfileButton = null!;
    private AppButton _removeProfileButton = null!;

    private Label _totalValue = null!;
    private Label _enabledValue = null!;
    private Label _conflictsValue = null!;

    private AppButton _openGameFolderButton = null!;
    private AppButton _openModsFolderButton = null!;

    private SearchBox _search = null!;
    private AppButton _importButton = null!;
    private AppButton _syncButton = null!;
    private AppButton _editButton = null!;
    private AppButton _enableButton = null!;
    private AppButton _disableButton = null!;
    private AppButton _deleteButton = null!;

    private TableLayoutPanel _workspace = null!;
    private Panel _progressPanel = null!;
    private AppProgressBar _progressBar = null!;
    private Label _progressLabel = null!;

    private ModificationListView _list = null!;
    private ContextMenuStrip _listMenu = null!;
    private ToolStripMenuItem _enableMenuItem = null!;
    private ToolStripMenuItem _disableMenuItem = null!;
    private ToolStripMenuItem _editMenuItem = null!;
    private ToolStripMenuItem _deleteMenuItem = null!;

    private Label _statusLabel = null!;
    private Label _versionLabel = null!;

    private void BuildLayout()
    {
        SuspendLayout();

        Text = "Bolt Mod Manager";
        MinimumSize = new Size(1040, 660);
        ClientSize = new Size(1180, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppTheme.Colors.Background;

        _content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.Background,
            Padding = new Padding(AppTheme.Spacing.Large)
        };

        _content.Controls.Add(BuildWorkspace());
        _content.Controls.Add(BuildSidebar());

        // Docked children are laid out from the last to the first, so the filling panel is added
        // first and the menu last to keep it pinned to the top edge.
        Controls.AddRange([_content, BuildStatusBar(), BuildMenu()]);

        MainMenuStrip = _menu;

        ResumeLayout(performLayout: true);
    }

    private MenuStrip BuildMenu()
    {
        _recentMenuItem = new ToolStripMenuItem("Recent");
        _closeGameMenuItem = CreateMenuItem("Close Game", OnCloseGameClicked, Keys.Control | Keys.W);

        var file = new ToolStripMenuItem("File");
        file.DropDownItems.AddRange([
            CreateMenuItem("New Game…", OnNewGameClicked, Keys.Control | Keys.N),
            CreateMenuItem("Open Game…", OnOpenGameClicked, Keys.Control | Keys.O),
            _recentMenuItem,
            new ToolStripSeparator(),
            _closeGameMenuItem,
            new ToolStripSeparator(),
            CreateMenuItem("Exit", (_, _) => Close(), Keys.Control | Keys.Q)
        ]);

        _restoreGameMenuItem = CreateMenuItem("Restore Game Defaults…", OnRestoreGameClicked);
        _restoreGameMenuItem.Enabled = false;

        var edit = new ToolStripMenuItem("Edit");
        edit.DropDownItems.AddRange([
            _restoreGameMenuItem,
            new ToolStripSeparator(),
            CreateMenuItem("Preferences…", OnPreferencesClicked, Keys.Control | Keys.P)
        ]);

        var help = new ToolStripMenuItem("Help");
        help.DropDownItems.Add(CreateMenuItem("About Bolt", OnAboutClicked));

        _menu = new MenuStrip
        {
            Dock = DockStyle.Top,
            BackColor = AppTheme.Colors.Background,
            ForeColor = AppTheme.Colors.TextSecondary,
            Font = AppTheme.Fonts.Body,
            Padding = new Padding(AppTheme.Spacing.Small, AppTheme.Spacing.Tiny, 0, AppTheme.Spacing.Tiny),
            Renderer = new ThemedToolStripRenderer()
        };

        _menu.Items.AddRange([file, edit, help]);

        return _menu;
    }

    private TableLayoutPanel BuildSidebar()
    {
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = SidebarWidth,
            BackColor = AppTheme.Colors.Background,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(0, 0, AppTheme.Spacing.Large, 0)
        };

        sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 144f));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 68f));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 128f));
        sidebar.RowStyles.Add(new RowStyle(
            SizeType.Absolute,
            SectionLabelHeight + (ShortcutButtonHeight * 2) + ShortcutGap));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        sidebar.Controls.Add(BuildGameCard(), 0, 0);
        sidebar.Controls.Add(BuildProfileSection(), 0, 1);
        sidebar.Controls.Add(BuildStatsCard(), 0, 2);
        sidebar.Controls.Add(BuildShortcutsSection(), 0, 3);

        return sidebar;
    }

    private Card BuildGameCard()
    {
        _gameIcon = new PictureBox
        {
            BackColor = Color.Transparent,
            Location = new Point(AppTheme.Spacing.Large, AppTheme.Spacing.Large),
            Size = new Size(44, 44),
            SizeMode = PictureBoxSizeMode.Zoom
        };

        _gameName = new Label
        {
            AutoEllipsis = true,
            BackColor = Color.Transparent,
            Font = AppTheme.Fonts.Subtitle,
            ForeColor = AppTheme.Colors.TextPrimary,
            Location = new Point(72, AppTheme.Spacing.Large),
            Size = new Size(180, 24),
            Text = "No game loaded"
        };

        _gameTarget = new Label
        {
            AutoEllipsis = true,
            BackColor = Color.Transparent,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextMuted,
            Location = new Point(72, 42),
            Size = new Size(180, 18),
            Text = "Open or create a game to start"
        };

        _playButton = new AppButton
        {
            Dock = DockStyle.Bottom,
            Enabled = false,
            Height = 38,
            Icon = IconKind.Play,
            IconSize = 14,
            Text = "Play",
            Variant = ButtonVariant.Primary
        };

        _playButton.Click += OnPlayClicked;
        _toolTip.SetToolTip(_playButton, "Launch the configured game");

        var card = new Card
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.Medium),
            Padding = new Padding(AppTheme.Spacing.Large)
        };

        card.Controls.AddRange([_playButton, _gameIcon, _gameName, _gameTarget]);

        return card;
    }

    private Panel BuildProfileSection()
    {
        _profileSelector = new AppDropdown
        {
            Dock = DockStyle.Fill,
            DisplayText = item => ((Profile)item).Name,
            Enabled = false,
            Placeholder = "No profile"
        };

        _profileSelector.SelectedItemChanged += OnProfileSelected;

        _addProfileButton = CreateIconButton(IconKind.Plus, "Create a new profile", OnAddProfileClicked);
        _removeProfileButton = CreateIconButton(IconKind.Trash, "Delete the selected profile", OnRemoveProfileClicked);

        _addProfileButton.Dock = DockStyle.Right;
        _removeProfileButton.Dock = DockStyle.Right;

        var row = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 34
        };

        // Controls docked to the same edge stack in reverse order of addition, so adding Remove
        // last puts it on the far right of the row.
        row.Controls.AddRange([_profileSelector, _addProfileButton, _removeProfileButton]);

        var container = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.Medium)
        };

        container.Controls.Add(row);
        container.Controls.Add(CreateSectionLabel("Profile"));

        return container;
    }

    private Card BuildStatsCard()
    {
        var card = new Card
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.Medium),
            Padding = new Padding(AppTheme.Spacing.Large, AppTheme.Spacing.Medium, AppTheme.Spacing.Large, AppTheme.Spacing.Medium)
        };

        card.Controls.Add(CreateStatRow("Conflicts", out _conflictsValue));
        card.Controls.Add(CreateStatRow("Enabled", out _enabledValue));
        card.Controls.Add(CreateStatRow("Modifications", out _totalValue));

        return card;
    }

    private TableLayoutPanel BuildShortcutsSection()
    {
        _openGameFolderButton = CreateShortcutButton(IconKind.Folder, "Game folder", OnOpenGameFolderClicked);
        _openModsFolderButton = CreateShortcutButton(IconKind.Package, "Modifications folder", OnOpenModsFolderClicked);

        // Rows instead of margins: the default layout engine ignores the margin of a docked child,
        // so stacking the two buttons with Dock.Top left them 2px apart no matter what was asked.
        var container = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            RowCount = 4
        };

        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, SectionLabelHeight));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, ShortcutButtonHeight));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, ShortcutGap));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, ShortcutButtonHeight));

        container.Controls.Add(CreateSectionLabel("Shortcuts"), 0, 0);
        container.Controls.Add(_openGameFolderButton, 0, 1);
        container.Controls.Add(_openModsFolderButton, 0, 3);

        foreach (Control child in container.Controls)
        {
            child.Dock = DockStyle.Fill;
            child.Margin = Padding.Empty;
        }

        return container;
    }

    private TableLayoutPanel BuildWorkspace()
    {
        _workspace = new TableLayoutPanel
        {
            BackColor = AppTheme.Colors.Background,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3
        };

        _workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        _workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, ToolbarHeight));
        _workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 0f));
        _workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _workspace.Controls.Add(BuildToolbar(), 0, 0);
        _workspace.Controls.Add(BuildProgressPanel(), 0, 1);
        _workspace.Controls.Add(BuildList(), 0, 2);

        // TableLayoutPanel gives every child a 3px margin by default, which ate into the fixed row
        // heights and clipped the toolbar buttons. Spacing is expressed with padding instead.
        foreach (Control child in _workspace.Controls)
            child.Margin = Padding.Empty;

        return _workspace;
    }

    private Panel BuildToolbar()
    {
        _search = new SearchBox
        {
            Dock = DockStyle.Left,
            Enabled = false,
            Width = 260
        };

        _search.QueryChanged += OnSearchQueryChanged;

        _importButton = new AppButton
        {
            Enabled = false,
            Height = 32,
            Icon = IconKind.Download,
            IconSize = 15,
            Text = "Import",
            Variant = ButtonVariant.Primary,
            Width = 108
        };

        _syncButton = new AppButton
        {
            Enabled = false,
            Height = 32,
            Icon = IconKind.Refresh,
            IconSize = 15,
            Text = "Deploy",
            Variant = ButtonVariant.Secondary,
            Width = 108
        };

        _editButton = new AppButton
        {
            Enabled = false,
            Height = 32,
            Icon = IconKind.Sliders,
            IconSize = 15,
            Text = "Edit",
            Variant = ButtonVariant.Secondary,
            Width = 88
        };

        _enableButton = CreateIconButton(IconKind.Check, "Enable the selected modifications", OnEnableSelectedClicked);
        _disableButton = CreateIconButton(IconKind.Ban, "Disable the selected modifications", OnDisableSelectedClicked);
        _deleteButton = CreateIconButton(IconKind.Trash, "Delete the selected modifications", OnDeleteSelectedClicked);
        _enableButton.IconColor = AppTheme.Colors.AccentText;
        _disableButton.IconColor = AppTheme.Colors.AccentText;
        _deleteButton.Variant = ButtonVariant.Danger;

        _importButton.Click += OnImportClicked;
        _toolTip.SetToolTip(_importButton, "Import modification archives");
        _syncButton.Click += OnSyncClicked;
        _toolTip.SetToolTip(_syncButton, "Deploy the active profile to the game folder");
        _editButton.Click += OnEditSelectedClicked;

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        foreach (var button in new Control[] { _enableButton, _disableButton, _deleteButton, _editButton, _syncButton, _importButton })
        {
            button.Margin = new Padding(AppTheme.Spacing.Small, 0, 0, 0);
            actions.Controls.Add(button);
        }

        var toolbar = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, AppTheme.Spacing.Medium)
        };

        toolbar.Controls.Add(actions);
        toolbar.Controls.Add(_search);

        return toolbar;
    }

    private Panel BuildProgressPanel()
    {
        _progressBar = new AppProgressBar { Dock = DockStyle.Top, Height = 6 };

        _progressLabel = new Label
        {
            AutoEllipsis = true,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _progressPanel = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.Small),
            Visible = false
        };

        _progressPanel.Controls.AddRange([_progressLabel, _progressBar]);

        return _progressPanel;
    }

    private GridFrame BuildList()
    {
        _editMenuItem = CreateMenuItem("Edit…", OnEditSelectedClicked);
        _enableMenuItem = CreateMenuItem("Enable", OnEnableSelectedClicked);
        _disableMenuItem = CreateMenuItem("Disable", OnDisableSelectedClicked);
        _deleteMenuItem = CreateMenuItem("Delete", OnDeleteSelectedClicked);

        // Shown as a hint only: the key itself is handled by the list so that pressing Delete while
        // typing in the search box cannot wipe a modification.
        _deleteMenuItem.ShortcutKeyDisplayString = "Del";

        _listMenu = new ContextMenuStrip
        {
            BackColor = AppTheme.Colors.Surface,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Renderer = new ThemedToolStripRenderer(),
            ShowImageMargin = false
        };

        _listMenu.Items.AddRange([
            _editMenuItem,
            new ToolStripSeparator(),
            _enableMenuItem,
            _disableMenuItem,
            new ToolStripSeparator(),
            _deleteMenuItem
        ]);

        _list = new ModificationListView
        {
            ContextMenuStrip = _listMenu,
            Dock = DockStyle.Fill
        };

        var frame = new GridFrame
        {
            Dock = DockStyle.Fill
        };

        frame.Controls.Add(_list);

        return frame;
    }

    private Panel BuildStatusBar()
    {
        _statusLabel = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextSecondary,
            Padding = new Padding(AppTheme.Spacing.Large, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _versionLabel = new Label
        {
            Dock = DockStyle.Right,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextMuted,
            Padding = new Padding(0, 0, AppTheme.Spacing.Large, 0),
            TextAlign = ContentAlignment.MiddleRight,
            Width = 260
        };

        var statusBar = new Panel
        {
            BackColor = AppTheme.Colors.Surface,
            Dock = DockStyle.Bottom,
            Height = StatusBarHeight
        };

        statusBar.Paint += (sender, e) =>
        {
            using var pen = new Pen(AppTheme.Colors.Border);
            e.Graphics.DrawLine(pen, 0, 0, ((Control)sender!).Width, 0);
        };

        statusBar.Controls.AddRange([_statusLabel, _versionLabel]);

        return statusBar;
    }

    /// <summary>Shows or hides the import progress row without leaving a gap behind.</summary>
    private void SetProgressVisible(bool visible)
    {
        _progressPanel.Visible = visible;
        _workspace.RowStyles[1].Height = visible ? ProgressRowHeight : 0f;
    }

    private static Label CreateSectionLabel(string text) => new()
    {
        BackColor = Color.Transparent,
        Dock = DockStyle.Top,
        Font = AppTheme.Fonts.Overline,
        ForeColor = AppTheme.Colors.TextMuted,
        Height = SectionLabelHeight,
        Text = text.ToUpperInvariant(),
        TextAlign = ContentAlignment.BottomLeft
    };

    private static Panel CreateStatRow(string text, out Label value)
    {
        value = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Right,
            Font = AppTheme.Fonts.BodyStrong,
            ForeColor = AppTheme.Colors.TextPrimary,
            Text = "0",
            TextAlign = ContentAlignment.MiddleRight,
            Width = 60
        };

        var caption = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var row = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 28
        };

        row.Controls.AddRange([caption, value]);

        return row;
    }

    private static AppButton CreateShortcutButton(IconKind icon, string text, EventHandler onClick)
    {
        var button = new AppButton
        {
            Dock = DockStyle.Top,
            Enabled = false,
            Height = ShortcutButtonHeight,
            Icon = icon,
            IconSize = 15,
            Text = text,
            // Outlined rather than Ghost so the two shortcuts keep their frame in every state.
            Variant = ButtonVariant.Secondary
        };

        button.Click += onClick;

        return button;
    }

    private AppButton CreateIconButton(IconKind icon, string tooltip, EventHandler onClick)
    {
        var button = new AppButton
        {
            Enabled = false,
            Height = 32,
            Icon = icon,
            IconSize = 16,
            Variant = ButtonVariant.Ghost,
            Width = 34
        };

        button.Click += onClick;
        _toolTip.SetToolTip(button, tooltip);

        return button;
    }

    private static ToolStripMenuItem CreateMenuItem(string text, EventHandler onClick, Keys shortcut = Keys.None)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeys = shortcut };
        item.Click += onClick;

        return item;
    }
}
