using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>
/// Main window: browses the modifications of the active profile and drives every operation on them.
/// </summary>
internal sealed partial class MainForm : ThemedForm
{
    private readonly IGameSessionService _session;
    private readonly IGameProcessService _process;
    private readonly IModImportService _import;
    private readonly IModDeploymentService _deployment;
    private readonly IUserPreferencesService _preferences;
    private readonly IGameRepository _repository;
    private readonly IDialogService _dialogs;
    private readonly Func<NewGameForm> _newGameFormFactory;
    private readonly Func<PreferencesForm> _preferencesFormFactory;
    private readonly string _version;

    private bool _isBusy;

    public MainForm(
        IGameSessionService session,
        IGameProcessService process,
        IModImportService import,
        IModDeploymentService deployment,
        IUserPreferencesService preferences,
        IGameRepository repository,
        IDialogService dialogs,
        Func<NewGameForm> newGameFormFactory,
        Func<PreferencesForm> preferencesFormFactory,
        AppSettings settings)
    {
        _session = session;
        _process = process;
        _import = import;
        _deployment = deployment;
        _preferences = preferences;
        _repository = repository;
        _dialogs = dialogs;
        _newGameFormFactory = newGameFormFactory;
        _preferencesFormFactory = preferencesFormFactory;
        _version = settings.Version;

        BuildLayout();
    }

    private GameSession? Current => _session.Current;

    protected override void InitializeEvents()
    {
        _session.GameLoaded += OnGameLoaded;
        _session.GameUnloaded += OnGameUnloaded;
        _session.GameChanged += OnGameChanged;

        _process.GameStarted += OnGameStarted;
        _process.GameExited += OnGameExited;

        _list.ToggleRequested += OnToggleRequested;
        _list.SelectedIndexChanged += OnListSelectionChanged;
        _list.KeyDown += OnListKeyDown;
        _listMenu.Opening += OnListMenuOpening;
    }

    protected override void TerminateEvents()
    {
        _session.GameLoaded -= OnGameLoaded;
        _session.GameUnloaded -= OnGameUnloaded;
        _session.GameChanged -= OnGameChanged;

        _process.GameStarted -= OnGameStarted;
        _process.GameExited -= OnGameExited;

        _list.ToggleRequested -= OnToggleRequested;
        _list.SelectedIndexChanged -= OnListSelectionChanged;
        _list.KeyDown -= OnListKeyDown;
        _listMenu.Opening -= OnListMenuOpening;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _versionLabel.Text = $"v{_version}";

        RefreshRecentMenu();
        ShowNoGameState();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // Forces the whole window to paint before any disk access. Reopening the last game blocks
        // the UI thread for a moment, and controls that have not painted yet stay at their default
        // system background — which is what made the interface flash white on startup.
        Update();

        RestoreLastGame();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _gameIcon.Image?.Dispose();
        _toolTip.Dispose();

        base.OnFormClosed(e);
    }

    // ---------------------------------------------------------------- session lifecycle

    private void RestoreLastGame()
    {
        var lastGame = _preferences.Current.RecentGames.FirstOrDefault();

        if (string.IsNullOrEmpty(lastGame) || !File.Exists(lastGame))
            return;

        var result = _session.Load(lastGame);

        if (result.Failed)
            SetStatus($"The last game could not be reopened: {result.Error}");
    }

    private async void OnGameLoaded(GameSession session)
    {
        RefreshRecentMenu();
        RefreshGameCard(session);
        RefreshProfiles(session);
        RefreshList();

        SetGameControlsEnabled(true);

        // Re-applies whatever the profile says. Nothing happens — and no elevation is requested —
        // when the game folder already matches, which is the usual case. It still inspects every
        // file of every modification, so it runs off the UI thread to keep the window painted.
        await SynchronizeAsync(session, $"Checking the modifications of {session.Game.Name}…").ConfigureAwait(true);
    }

    /// <summary>Runs a synchronisation in the background, reporting through the status bar.</summary>
    private async Task SynchronizeAsync(GameSession session, string message)
    {
        SetBusy(true);
        SetStatus(message);

        try
        {
            var result = await Task.Run(() => _deployment.Synchronize(session)).ConfigureAwait(true);

            if (IsDisposed)
                return;

            SetStatus(result.Succeeded
                ? BuildIdleStatus(session)
                : $"Some modifications are not applied: {result.Error}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            SetStatus($"The modifications could not be checked: {ex.Message}");
        }
        finally
        {
            if (!IsDisposed)
                SetBusy(false);
        }
    }

    private void OnGameUnloaded() => ShowNoGameState();

    private void OnGameChanged(GameSession session)
    {
        RefreshList();
        SetStatus(BuildIdleStatus(session));
    }

    private void ShowNoGameState()
    {
        _gameIcon.Image?.Dispose();
        _gameIcon.Image = null;

        _gameName.Text = "No game loaded";
        _gameTarget.Text = "Open or create a game to start";

        _profileSelector.Clear();

        _list.SetItems([]);
        _search.Clear();

        SetGameControlsEnabled(false);
        UpdateStatistics();

        SetStatus("Press Ctrl+O to open a game, or Ctrl+N to create one.");
    }

    private void RefreshGameCard(GameSession session)
    {
        _gameName.Text = session.Game.Name;
        _gameTarget.Text = session.Game.TargetPath;
        _toolTip.SetToolTip(_gameTarget, session.Game.TargetPath);

        _gameIcon.Image?.Dispose();
        _gameIcon.Image = TryExtractIcon(session.Game.ExecutablePath);
    }

    private static Bitmap? TryExtractIcon(string executablePath)
    {
        try
        {
            using var icon = Icon.ExtractAssociatedIcon(executablePath);
            return icon?.ToBitmap();
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            // A missing or unreadable executable only costs the icon; the game stays usable.
            return null;
        }
    }

    private void RefreshProfiles(GameSession session) =>
        _profileSelector.SetItems(session.Game.Profiles, session.ActiveProfile);

    private void RefreshList()
    {
        if (Current is null)
        {
            _list.SetItems([]);
            UpdateStatistics();
            return;
        }

        var query = _search.Query;

        var modifications = Current.ActiveProfile.Modifications
            .Where(m => query.Length == 0 || m.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        _list.EmptyMessage = query.Length == 0
            ? "No modifications in this profile"
            : $"Nothing matches “{query}”";

        _list.EmptyHint = query.Length == 0
            ? "Use Import to add a package to this profile."
            : "Try a different search term.";

        _list.SetItems(modifications);

        UpdateStatistics();
        OnListSelectionChanged(this, EventArgs.Empty);
    }

    private void UpdateStatistics()
    {
        if (Current is null)
        {
            _totalValue.Text = "0";
            _enabledValue.Text = "0";
            _conflictsValue.Text = "0";
            _conflictsValue.ForeColor = AppTheme.Colors.TextPrimary;
            return;
        }

        var modifications = Current.ActiveProfile.Modifications;
        var conflicts = _deployment.FindConflicts(Current).Count;

        _totalValue.Text = modifications.Count.ToString(CultureInfo.CurrentCulture);
        _enabledValue.Text = modifications.Count(m => m.IsEnabled).ToString(CultureInfo.CurrentCulture);
        _conflictsValue.Text = conflicts.ToString(CultureInfo.CurrentCulture);
        _conflictsValue.ForeColor = conflicts > 0 ? AppTheme.Colors.Warning : AppTheme.Colors.TextPrimary;
    }

    private static string BuildIdleStatus(GameSession session) =>
        $"{session.Game.Name} · {session.ActiveProfile.Name} · {session.ActiveProfile.Modifications.Count(m => m.IsEnabled)} of {session.ActiveProfile.Modifications.Count} modifications enabled";

    private void SetStatus(string message) => _statusLabel.Text = message;

    private void SetGameControlsEnabled(bool enabled)
    {
        _playButton.Enabled = enabled && !_process.IsRunning;
        _profileSelector.Enabled = enabled;
        _addProfileButton.Enabled = enabled;
        _removeProfileButton.Enabled = enabled;
        _openGameFolderButton.Enabled = enabled;
        _openModsFolderButton.Enabled = enabled;
        _search.Enabled = enabled;
        _importButton.Enabled = enabled;
        _syncButton.Enabled = enabled;
        _closeGameMenuItem.Enabled = enabled;

        OnListSelectionChanged(this, EventArgs.Empty);
    }

    // ---------------------------------------------------------------- menu

    private void RefreshRecentMenu()
    {
        _recentMenuItem.DropDownItems.Clear();

        var recentGames = _preferences.Current.RecentGames.Where(File.Exists).ToList();

        foreach (var path in recentGames)
        {
            var item = new ToolStripMenuItem(Path.GetFileName(Path.GetDirectoryName(path)) ?? path)
            {
                Tag = path,
                ToolTipText = path
            };

            item.Click += OnRecentGameClicked;

            _recentMenuItem.DropDownItems.Add(item);
        }

        if (recentGames.Count > 0)
            _recentMenuItem.DropDownItems.Add(new ToolStripSeparator());

        var clear = new ToolStripMenuItem("Clear history") { Enabled = recentGames.Count > 0 };

        clear.Click += (_, _) =>
        {
            _preferences.ClearRecentGames();
            RefreshRecentMenu();
        };

        _recentMenuItem.DropDownItems.Add(clear);
        _recentMenuItem.Enabled = recentGames.Count > 0;
    }

    private void OnRecentGameClicked(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem { Tag: string path })
            return;

        var result = _session.Load(path);

        if (result.Failed)
        {
            _dialogs.Error(result.Error!, "Open Game");
            RefreshRecentMenu();
        }
    }

    private void OnNewGameClicked(object? sender, EventArgs e)
    {
        using var form = _newGameFormFactory();
        form.ShowDialog(this);
    }

    private void OnOpenGameClicked(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open Game",
            Filter = $"Bolt game file (*{_repository.FileExtension})|*{_repository.FileExtension}",
            InitialDirectory = _preferences.Current.GamesRoot,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var result = _session.Load(dialog.FileName);

        if (result.Failed)
            _dialogs.Error(result.Error!, "Open Game");
    }

    private void OnCloseGameClicked(object? sender, EventArgs e) => _session.Unload();

    private void OnPreferencesClicked(object? sender, EventArgs e)
    {
        using var form = _preferencesFormFactory();
        form.ShowDialog(this);
    }

    private void OnAboutClicked(object? sender, EventArgs e)
    {
        using var form = new AboutForm(_version);
        form.ShowDialog(this);
    }

    // ---------------------------------------------------------------- game process

    private void OnPlayClicked(object? sender, EventArgs e)
    {
        if (Current is null)
            return;

        var result = _process.Run(Current.Game.ExecutablePath);

        if (result.Failed)
            _dialogs.Error(result.Error!, "Launch Game");
    }

    private void OnGameStarted()
    {
        SetGameRunning(true);
        SetStatus(Current is null ? "Running" : $"Running · {Current.Game.Name}");
    }

    private void OnGameExited()
    {
        if (InvokeRequired)
        {
            BeginInvoke(OnGameExited);
            return;
        }

        SetGameRunning(false);
        SetStatus(Current is null ? string.Empty : BuildIdleStatus(Current));
    }

    /// <summary>
    /// Prevents changes to the loaded game while its process is running. The status bar deliberately
    /// stays enabled so Bolt can continue to report which game owns the locked session.
    /// </summary>
    private void SetGameRunning(bool running)
    {
        _menu.Enabled = !running && !_isBusy;
        _content.Enabled = !running;
    }

    // ---------------------------------------------------------------- profiles

    private void OnProfileSelected(object? sender, EventArgs e)
    {
        // Repopulating the list never notifies, so reaching here always means a user choice.
        if (Current is null || _profileSelector.SelectedItem is not Profile profile)
            return;

        if (profile.Id == Current.Game.ActiveProfileId)
            return;

        var previous = Current.ActiveProfile;

        Current.SelectProfile(profile);

        // The modifications of the profile we are leaving are no longer part of the active profile,
        // so they have to be handed over explicitly to be unlinked.
        var result = _deployment.Synchronize(Current, [.. previous.Modifications.Where(m => m.IsEnabled)]);

        if (result.Failed)
        {
            Current.SelectProfile(previous);
            RefreshProfiles(Current);
            ReportFailure(result, "Switch Profile");
            return;
        }

        _session.Save();
        RefreshList();
    }

    private void OnAddProfileClicked(object? sender, EventArgs e)
    {
        if (Current is null)
            return;

        using var prompt = new TextPromptForm("New Profile", "Profile name");

        if (prompt.ShowDialog(this) != DialogResult.OK || prompt.Value.Length == 0)
            return;

        if (Current.Game.Profiles.Any(p => p.Name.Equals(prompt.Value, StringComparison.OrdinalIgnoreCase)))
        {
            _dialogs.Warning($"A profile named \"{prompt.Value}\" already exists.", "New Profile");
            return;
        }

        var profile = new Profile { Name = prompt.Value };

        Current.Game.Profiles.Add(profile);
        _session.Save();

        RefreshProfiles(Current);
        _profileSelector.SelectedItem = profile;
    }

    private void OnRemoveProfileClicked(object? sender, EventArgs e)
    {
        if (Current is null || _profileSelector.SelectedItem is not Profile profile)
            return;

        if (Current.Game.Profiles.Count == 1)
        {
            _dialogs.Warning("A game must keep at least one profile.", "Delete Profile");
            return;
        }

        if (!_dialogs.Confirm(
            $"Delete the profile \"{profile.Name}\" and its list of {profile.Modifications.Count} modifications?\n\nThe imported files stay on disk.",
            "Delete Profile",
            destructive: true))
        {
            return;
        }

        var fallback = Current.Game.Profiles.First(p => p.Id != profile.Id);

        Current.Game.Profiles.Remove(profile);
        Current.SelectProfile(fallback);

        var result = _deployment.Synchronize(Current, [.. profile.Modifications.Where(m => m.IsEnabled)]);

        if (result.Failed)
        {
            Current.Game.Profiles.Add(profile);
            Current.SelectProfile(profile);
            RefreshProfiles(Current);
            ReportFailure(result, "Delete Profile");
            return;
        }

        _session.Save();
        RefreshProfiles(Current);
        RefreshList();
    }

    // ---------------------------------------------------------------- modifications

    private void OnSearchQueryChanged(object? sender, EventArgs e) => RefreshList();

    private void OnListSelectionChanged(object? sender, EventArgs e)
    {
        var hasSelection = Current is not null && _list.SelectedIndices.Count > 0 && !_isBusy;

        _enableButton.Enabled = hasSelection;
        _disableButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private void OnListMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var selection = _list.SelectedModifications;

        if (selection.Count == 0 || _isBusy)
        {
            e.Cancel = true;
            return;
        }

        _enableMenuItem.Enabled = selection.Any(m => !m.IsEnabled);
        _disableMenuItem.Enabled = selection.Any(m => m.IsEnabled);
    }

    private void OnListKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Delete)
            return;

        OnDeleteSelectedClicked(sender, EventArgs.Empty);
        e.Handled = true;
    }

    private void OnToggleRequested(Modification modification) => SetEnabled([modification], !modification.IsEnabled);

    private void OnEnableSelectedClicked(object? sender, EventArgs e) => SetEnabled(_list.SelectedModifications, enabled: true);

    private void OnDisableSelectedClicked(object? sender, EventArgs e) => SetEnabled(_list.SelectedModifications, enabled: false);

    /// <summary>Applies <paramref name="enabled"/> to every modification and deploys the result.</summary>
    private void SetEnabled(IReadOnlyList<Modification> modifications, bool enabled)
    {
        if (Current is null || _isBusy)
            return;

        var affected = modifications.Where(m => m.IsEnabled != enabled).ToList();

        if (affected.Count == 0)
            return;

        foreach (var modification in affected)
            modification.IsEnabled = enabled;

        var result = _deployment.Synchronize(Current);

        if (result.Failed)
        {
            foreach (var modification in affected)
                modification.IsEnabled = !enabled;

            RefreshList();
            ReportFailure(result, enabled ? "Enable Modifications" : "Disable Modifications");
            return;
        }

        _session.Save();
        RefreshList();
        WarnAboutConflicts();
    }

    private void OnDeleteSelectedClicked(object? sender, EventArgs e)
    {
        if (Current is null || _isBusy)
            return;

        var selection = _list.SelectedModifications;

        if (selection.Count == 0)
            return;

        var prompt = selection.Count == 1
            ? $"Delete \"{selection[0].Name}\" and every file it installed?"
            : $"Delete {selection.Count} modifications and every file they installed?";

        if (!_dialogs.Confirm(prompt, "Delete Modifications", destructive: true))
            return;

        var profile = Current.ActiveProfile;
        var positions = selection.ToDictionary(m => m, profile.Modifications.IndexOf);

        foreach (var modification in selection)
            profile.Modifications.Remove(modification);

        var result = _deployment.Synchronize(Current, selection);

        if (result.Failed)
        {
            foreach (var (modification, index) in positions.OrderBy(pair => pair.Value))
                profile.Modifications.Insert(Math.Min(index, profile.Modifications.Count), modification);

            RefreshList();
            ReportFailure(result, "Delete Modifications");
            return;
        }

        var failures = DeleteModificationFolders(Current, selection);

        _session.Save();
        RefreshList();

        if (failures.Count > 0)
            _dialogs.Warning($"Some files could not be removed:\n{string.Join(Environment.NewLine, failures)}", "Delete Modifications");
    }

    private static List<string> DeleteModificationFolders(GameSession session, IReadOnlyList<Modification> modifications)
    {
        var failures = new List<string>();

        foreach (var modification in modifications)
        {
            var path = session.GetModificationPath(modification);

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                failures.Add($"{modification.Name}: {ex.Message}");
            }
        }

        return failures;
    }

    private void OnSyncClicked(object? sender, EventArgs e)
    {
        if (Current is null || _isBusy)
            return;

        var result = _deployment.Synchronize(Current);

        if (result.Failed)
        {
            ReportFailure(result, "Synchronize");
            return;
        }

        SetStatus($"{BuildIdleStatus(Current)} · synchronized");
        WarnAboutConflicts();
    }

    private void WarnAboutConflicts()
    {
        if (Current is null)
            return;

        var conflicts = _deployment.FindConflicts(Current);

        if (conflicts.Count == 0)
            return;

        SetStatus($"{BuildIdleStatus(Current)} · {conflicts.Count} file(s) provided by more than one modification");
    }

    // ---------------------------------------------------------------- import

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        if (Current is null || _isBusy)
            return;

        var extensions = string.Join(";", _import.SupportedExtensions.Select(extension => $"*{extension}"));

        using var dialog = new OpenFileDialog
        {
            Title = "Import Modifications",
            Filter = $"Modification archives ({extensions})|{extensions}",
            InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Multiselect = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        await ImportAsync(dialog.FileNames).ConfigureAwait(true);
    }

    private async Task ImportAsync(string[] archivePaths)
    {
        var session = Current;

        if (session is null)
            return;

        SetBusy(true);
        SetProgressVisible(true);
        _progressBar.Reset();

        // Reading the archives happens before the first entry is extracted and can take a while on
        // a large package, so the indicator starts moving right away instead of after the fact.
        _progressBar.IsIndeterminate = true;
        _progressLabel.Text = archivePaths.Length == 1
            ? $"Reading {Path.GetFileName(archivePaths[0])}…"
            : $"Reading {archivePaths.Length} archives…";

        var stopwatch = Stopwatch.StartNew();

        var progress = new Progress<ImportProgress>(report =>
        {
            // Thousands of entries would otherwise flood the UI thread with paint requests.
            if (report.Completed < report.Total && stopwatch.ElapsedMilliseconds < 60)
                return;

            stopwatch.Restart();

            _progressBar.IsIndeterminate = false;
            _progressBar.Maximum = Math.Max(report.Total, 1);
            _progressBar.Value = report.Completed;

            var percentage = report.Total == 0 ? 0 : report.Completed * 100 / report.Total;

            _progressLabel.Text = $"Extracting {report.CurrentItem}  —  {report.Completed} of {report.Total} files ({percentage}%)";
        });

        try
        {
            var imported = await _import
                .ImportAsync(archivePaths, session, session.ActiveProfile, progress)
                .ConfigureAwait(true);

            // Saving before deploying keeps the imported list even if the deployment is refused.
            _session.Save();

            var replaced = imported
                .Select(entry => entry.Replaced)
                .OfType<Modification>()
                .ToList();

            var result = _deployment.Synchronize(session, replaced);

            RefreshList();

            if (result.Failed)
                ReportFailure(result, "Import Modifications");
            else
                SetStatus($"Imported {imported.Count} modification(s) · {BuildIdleStatus(session)}");

            WarnAboutConflicts();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _dialogs.Error($"The modifications could not be imported:\n{ex.Message}", "Import Modifications");
        }
        finally
        {
            _progressBar.IsIndeterminate = false;
            SetProgressVisible(false);
            _progressLabel.Text = string.Empty;
            SetBusy(false);
        }
    }

    // ---------------------------------------------------------------- shortcuts

    private void OnOpenGameFolderClicked(object? sender, EventArgs e) => OpenFolder(Current?.Game.TargetPath);

    private void OnOpenModsFolderClicked(object? sender, EventArgs e) => OpenFolder(Current?.ModificationsPath);

    private void OpenFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            _dialogs.Warning("That folder no longer exists.", "Open Folder");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })?.Dispose();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException)
        {
            _dialogs.Error($"The folder could not be opened:\n{ex.Message}", "Open Folder");
        }
    }

    // ---------------------------------------------------------------- helpers

    private void SetBusy(bool busy)
    {
        _isBusy = busy;

        _menu.Enabled = !busy && !_process.IsRunning;
        _importButton.Enabled = !busy && Current is not null;
        _syncButton.Enabled = !busy && Current is not null;
        _playButton.Enabled = !busy && Current is not null && !_process.IsRunning;
        _profileSelector.Enabled = !busy && Current is not null;
        _addProfileButton.Enabled = !busy && Current is not null;
        _removeProfileButton.Enabled = !busy && Current is not null;

        OnListSelectionChanged(this, EventArgs.Empty);

        Cursor = busy ? Cursors.AppStarting : Cursors.Default;
    }

    /// <summary>Reports a failed operation, staying quiet when the user simply declined elevation.</summary>
    private void ReportFailure(OperationResult result, string caption)
    {
        if (result.WasCanceled)
        {
            SetStatus(result.Error ?? "Operation canceled.");
            return;
        }

        _dialogs.Error(result.Error ?? "The operation failed.", caption);
    }
}
