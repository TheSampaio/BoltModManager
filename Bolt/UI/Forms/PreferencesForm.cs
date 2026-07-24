using System.Drawing;
using System.Windows.Forms;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>Application settings: where games are created and which colour scheme is used.</summary>
internal sealed class PreferencesForm : ThemedForm
{
    private readonly IUserPreferencesService _preferences;
    private readonly IDialogService _dialogs;

    private readonly AppTextField _gamesRootField;
    private readonly AppDropdown _themeSelector;

    public PreferencesForm(IUserPreferencesService preferences, IDialogService dialogs)
    {
        _preferences = preferences;
        _dialogs = dialogs;

        Text = "Preferences";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(520, 260);
        Padding = new Padding(AppTheme.Spacing.XLarge, AppTheme.Spacing.Large, AppTheme.Spacing.XLarge, AppTheme.Spacing.Large);

        _gamesRootField = new AppTextField
        {
            ActionIcon = IconKind.Folder,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.Medium),
            Placeholder = "Where Bolt stores the data of new games",
            ShowAction = true,
            Text = "Games folder",
            Value = preferences.Current.GamesRoot
        };

        _gamesRootField.ActionClick += OnBrowseGamesRoot;

        _themeSelector = new AppDropdown { Dock = DockStyle.Top, Height = 34 };
        _themeSelector.SetItems(
            [ThemeMode.Dark, ThemeMode.Light, ThemeMode.System],
            preferences.Current.Theme);

        var save = new AppButton
        {
            Dock = DockStyle.Right,
            Text = "Save",
            Variant = ButtonVariant.Primary,
            Width = 110
        };

        var cancel = new AppButton
        {
            Dock = DockStyle.Right,
            Margin = new Padding(0, 0, AppTheme.Spacing.Small, 0),
            Text = "Cancel",
            Width = 100
        };

        save.Click += OnSaveClicked;
        cancel.Click += (_, _) => Close();

        var buttons = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(0, AppTheme.Spacing.Small, 0, 0)
        };

        buttons.Controls.AddRange([cancel, save]);

        var themeSection = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 60
        };

        themeSection.Controls.Add(_themeSelector);
        themeSection.Controls.Add(CreateLabel("Appearance", AppTheme.Fonts.Body, AppTheme.Colors.TextSecondary, 22));

        var hint = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Font = AppTheme.Fonts.Caption,
            ForeColor = AppTheme.Colors.TextMuted,
            Height = 34,
            Text = "Changing the appearance takes effect the next time Bolt starts. Existing games keep their own folders.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Docked children stack in reverse order of addition.
        Controls.AddRange([buttons, hint, themeSection, _gamesRootField]);
    }

    private static Label CreateLabel(string text, Font font, Color color, int height) => new()
    {
        BackColor = Color.Transparent,
        Dock = DockStyle.Top,
        Font = font,
        ForeColor = color,
        Height = height,
        Text = text,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private void OnBrowseGamesRoot(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the folder where Bolt creates new games",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_gamesRootField.Value) ? _gamesRootField.Value : string.Empty
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _gamesRootField.Value = dialog.SelectedPath;
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        var gamesRoot = _gamesRootField.Value.Trim();

        if (gamesRoot.Length == 0)
        {
            _dialogs.Warning("Select a folder for your games.", "Preferences");
            return;
        }

        try
        {
            Directory.CreateDirectory(gamesRoot);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            _dialogs.Error($"\"{gamesRoot}\" cannot be used:\n{ex.Message}", "Preferences");
            return;
        }

        _preferences.Current.GamesRoot = gamesRoot;

        if (_themeSelector.SelectedItem is ThemeMode theme)
            _preferences.Current.Theme = theme;

        _preferences.Save();

        DialogResult = DialogResult.OK;
        Close();
    }
}
