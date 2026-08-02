using System.Drawing;
using System.Windows.Forms;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Storage;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>
/// Creates a new game: picks the installation folder, the executable and where Bolt stores its data.
/// </summary>
internal sealed class NewGameForm : ThemedForm
{
    private readonly IGameSessionService _session;
    private readonly IGameRepository _repository;
    private readonly IUserPreferencesService _preferences;
    private readonly IDialogService _dialogs;

    private readonly AppTextField _targetField;
    private readonly AppTextField _nameField;
    private readonly AppTextField _executableField;
    private readonly AppTextField _locationField;

    private bool _nameEditedByUser;

    public NewGameForm(
        IGameSessionService session,
        IGameRepository repository,
        IUserPreferencesService preferences,
        IDialogService dialogs)
    {
        _session = session;
        _repository = repository;
        _preferences = preferences;
        _dialogs = dialogs;

        Text = "New Game";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(520, 396);
        Padding = new Padding(AppTheme.Spacing.XLarge, AppTheme.Spacing.Large, AppTheme.Spacing.XLarge, AppTheme.Spacing.Large);

        _targetField = CreateField("Game folder", "The folder where the game is installed", IconKind.Folder, OnBrowseTarget);
        _nameField = CreateField("Name", "How the game appears in Bolt");
        _executableField = CreateField("Executable", "The file used to launch the game", IconKind.Folder, OnBrowseExecutable);
        _locationField = CreateField("Bolt data folder", "Created automatically");
        _locationField.ReadOnly = true;

        _targetField.TabIndex = 0;
        _nameField.TabIndex = 1;
        _executableField.TabIndex = 2;
        _locationField.TabIndex = 3;

        _targetField.ValueChanged += OnTargetChanged;
        _nameField.ValueChanged += OnNameChanged;

        var create = new AppButton
        {
            Dock = DockStyle.Right,
            Text = "Create game",
            TabIndex = 1,
            Variant = ButtonVariant.Primary,
            Width = 130
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

        create.Click += OnCreateClicked;
        cancel.Click += (_, _) => Close();

        var buttons = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(0, AppTheme.Spacing.Small, 0, 0),
            TabIndex = 4
        };

        buttons.Controls.AddRange([cancel, create]);

        var header = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextMuted,
            Height = 34,
            Text = "Bolt keeps modifications outside the game folder and links them in, so the original files are never lost.",
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false
        };

        // Docked children stack in reverse order of addition.
        Controls.AddRange([buttons, _locationField, _executableField, _nameField, _targetField, header]);

        foreach (var field in new[] { _targetField, _nameField, _executableField, _locationField })
            field.Margin = new Padding(0, 0, 0, AppTheme.Spacing.Medium);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UpdateLocation();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _targetField.Focus();
    }

    private static AppTextField CreateField(string label, string placeholder, IconKind actionIcon = IconKind.None, EventHandler? onAction = null)
    {
        var field = new AppTextField
        {
            Dock = DockStyle.Top,
            Placeholder = placeholder,
            Text = label
        };

        if (onAction is null)
            return field;

        field.ShowAction = true;
        field.ActionIcon = actionIcon;
        field.ActionClick += onAction;

        return field;
    }

    private void OnBrowseTarget(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the folder where the game is installed",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _targetField.Value = dialog.SelectedPath;
    }

    private void OnBrowseExecutable(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select the game executable",
            Filter = "Executable (*.exe)|*.exe",
            InitialDirectory = Directory.Exists(_targetField.Value) ? _targetField.Value : string.Empty
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _executableField.Value = dialog.FileName;
    }

    private void OnTargetChanged(object? sender, EventArgs e)
    {
        // The name only follows the folder until the user types one of their own.
        if (!_nameEditedByUser && _targetField.Value.Length > 0)
        {
            _nameField.ValueChanged -= OnNameChanged;
            _nameField.Value = Path.GetFileName(Path.TrimEndingDirectorySeparator(_targetField.Value));
            _nameField.ValueChanged += OnNameChanged;
        }

        UpdateLocation();
    }

    private void OnNameChanged(object? sender, EventArgs e)
    {
        _nameEditedByUser = true;
        UpdateLocation();
    }

    private void UpdateLocation()
    {
        var name = _nameField.Value.Trim();

        _locationField.Value = name.Length == 0
            ? _preferences.Current.GamesRoot
            : Path.Combine(_preferences.Current.GamesRoot, PathUtility.ToSafeFolderName(name));
    }

    private void OnCreateClicked(object? sender, EventArgs e)
    {
        if (!TryValidate(out var error))
        {
            _dialogs.Warning(error, "New Game");
            return;
        }

        var game = new Game
        {
            Name = _nameField.Value.Trim(),
            ExecutablePath = _executableField.Value.Trim(),
            TargetPath = Path.TrimEndingDirectorySeparator(_targetField.Value.Trim()),
            Profiles = [new Profile { Name = "Main" }]
        };

        GameSession created;

        try
        {
            created = _repository.Create(game, _locationField.Value);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _dialogs.Error($"The game could not be created:\n{ex.Message}", "New Game");
            return;
        }

        var result = _session.Load(created.FilePath);

        if (result.Failed)
        {
            _dialogs.Error(result.Error!, "New Game");
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private bool TryValidate(out string error)
    {
        error = string.Empty;

        if (_nameField.Value.Trim().Length == 0)
            error = "Enter a name for the game.";
        else if (!Directory.Exists(_targetField.Value))
            error = "Select an existing game folder.";
        else if (!File.Exists(_executableField.Value))
            error = "Select an existing executable file.";
        else if (Directory.Exists(_locationField.Value) && Directory.EnumerateFileSystemEntries(_locationField.Value).Any())
            error = $"\"{_locationField.Value}\" already exists and is not empty. Choose a different name.";

        return error.Length == 0;
    }
}
