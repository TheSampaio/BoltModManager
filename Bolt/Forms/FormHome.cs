using Bolt.Base;
using Bolt.Data;
using Bolt.Interfaces;
using Bolt.Models;
using Bolt.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Forms
{
    public partial class FrmHome : EventfulForm
    {
        private readonly IGameSessionService _gameSession;
        private readonly IGameProcessService _gameProcess;
        private readonly IModImportService _modImportService;
        private readonly IModDeploymentService _modDeployment;

        private bool _isLoadingMods = false;

        internal FrmHome(
            IGameSessionService gameSession,
            IGameProcessService gameProcess,
            IModImportService modImportService,
            IModDeploymentService modDeployment)
        {
            _gameSession = gameSession;
            _gameProcess = gameProcess;
            _modImportService = modImportService;
            _modDeployment = modDeployment;

            InitializeComponent();
        }

        protected override void InitializeEvents()
        {
            _gameSession.GameLoaded += OnGameLoaded;
            _gameSession.GameUnloaded += OnGameUnloaded;

            _gameProcess.GameStarted += OnGameStarted;
            _gameProcess.GameExited += OnGameExited;

            LvwModifications.ItemCheck += LvwModifications_ItemCheck;
            LvwModifications.KeyDown += LvwModifications_KeyDown;
        }

        protected override void TerminateEvents()
        {
            _gameProcess.GameStarted -= OnGameStarted;
            _gameProcess.GameExited -= OnGameExited;

            _gameSession.GameLoaded -= OnGameLoaded;
            _gameSession.GameUnloaded -= OnGameUnloaded;

            LvwModifications.ItemCheck -= LvwModifications_ItemCheck;
            LvwModifications.KeyDown -= LvwModifications_KeyDown;
        }

        private void LvwModifications_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedModification();
            }
        }

        private void EnableToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleSelectedModifications(true);
        }

        private void DisableToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleSelectedModifications(false);
        }

        private void DeleteToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            DeleteSelectedModification();
        }

        private void DeleteSelectedModification()
        {
            if (_isLoadingMods || LvwModifications.SelectedItems.Count == 0)
                return;

            var selectedItems = LvwModifications.SelectedItems.Cast<ListViewItem>().ToList();
            var modsToDelete = selectedItems.Select(i => (ModificationModel)i.Tag!).ToList();

            var currentGame = _gameSession.CurrentGame!;
            var currentProfile = currentGame.Profiles[CmbProfiles.SelectedIndex];

            string prompt = modsToDelete.Count == 1
                ? $"Are you sure you want to delete '{modsToDelete[0].Name}'?"
                : $"Are you sure you want to delete {modsToDelete.Count} modifications?";

            var result = MessageBox.Show(prompt, "Delete Modification(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;

            var activeModsToDelete = modsToDelete.Where(m => m.IsEnabled).ToList();

            if (activeModsToDelete.Count > 0)
            {
                if (!_modDeployment.RevertModifications(currentGame, activeModsToDelete))
                    return;
            }

            foreach (var mod in modsToDelete)
            {
                currentProfile.Modifications.Remove(mod);

                string modDirectory = Path.Combine(currentGame.ModificationsPath, mod.Name);

                if (System.IO.Directory.Exists(modDirectory))
                {
                    try
                    {
                        System.IO.Directory.Delete(modDirectory, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to remove modification files for '{mod.Name}':\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            foreach (var item in selectedItems)
                LvwModifications.Items.Remove(item);

            string gameFilename = $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}";
            GameData.Save(currentGame, gameFilename);
        }

        private void LvwModifications_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isLoadingMods)
                return;

            var item = LvwModifications.Items[e.Index];

            if (item.Tag is not ModificationModel mod)
                return;

            var currentGame = _gameSession.CurrentGame!;

            if (e.NewValue == CheckState.Unchecked)
            {
                var result = MessageBox.Show(
                    "Do you really want to disable this mod?",
                    "Disable Mod",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    e.NewValue = e.CurrentValue;
                    return;
                }

                if (!_modDeployment.RevertModification(currentGame, mod))
                {
                    e.NewValue = e.CurrentValue;
                    return;
                }

                mod.IsEnabled = false;
            }
            else if (e.NewValue == CheckState.Checked)
            {
                if (!_modDeployment.DeployModification(currentGame, mod))
                {
                    e.NewValue = e.CurrentValue;
                    return;
                }

                mod.IsEnabled = true;
            }

            string gameFilename = $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}";
            GameData.Save(currentGame, gameFilename);
        }

        private void NewGame_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = Program.ServiceProvider.GetRequiredService<FrmNewGame>();
            ShowModalWindow(frm);
        }

        private void OpenGame_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OfdOpenGame.Title = "Open Game";
            OfdOpenGame.FileName = string.Empty;
            OfdOpenGame.Filter = "Bolt Game File (*.bltg)|*.bltg";
            OfdOpenGame.InitialDirectory = ModificationsData.Load();
            OfdOpenGame.Multiselect = false;

            if (OfdOpenGame.ShowDialog() == DialogResult.OK)
            {
                if (!(Path.GetExtension(OfdOpenGame.FileName)?.ToLower() == ".bltg"))
                {
                    MessageBox.Show("Please select a valid Bolt game file.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _gameSession.LoadGame(OfdOpenGame.FileName);
            }
        }

        private void QuitGame_ToolStripMenuItem_Click(object sender, EventArgs e) => OnGameUnloaded();

        private void Quit_ToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void Settings_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = Program.ServiceProvider.GetRequiredService<FrmPreferences>();
            ShowModalWindow(frm);
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            if (_gameSession.CurrentGame is null)
            {
                MessageBox.Show($"Unable to launch the game.", "Game Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _gameProcess.RunGame(_gameSession.CurrentGame.ExecutablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Game Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnImport_Click(object sender, EventArgs e)
        {
            OfdOpenGame.Title = "Import Modification(s)";
            OfdOpenGame.FileName = string.Empty;
            OfdOpenGame.Filter = "Zip File (*.zip)|*.zip";
            OfdOpenGame.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            OfdOpenGame.Multiselect = true;

            if (OfdOpenGame.ShowDialog() != DialogResult.OK)
                return;

            var currentGame = _gameSession.CurrentGame!;
            var currentProfile = currentGame.Profiles[CmbProfiles.SelectedIndex];

            PrgImport.Value = 0;

            int totalFiles = OfdOpenGame.FileNames.Sum(file =>
                Path.GetExtension(file)?.ToLower() == ".zip" ? Bolt.Utilities.Archive.OpenRead(file).Entries.Count(x => !string.IsNullOrEmpty(x.Name)) : 0);

            PrgImport.Maximum = totalFiles;

            var progress = new SyncProgress<int>(value =>
            {
                PrgImport.Value = value;
                PrgImport.Refresh();
            }, this);

            try
            {
                await _modImportService.ImportModsAsync(OfdOpenGame.FileNames, currentGame, currentProfile, progress);

                string gameFilename = $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}";
                GameData.Save(currentGame, gameFilename);

                _gameSession.LoadGame(gameFilename);

                MessageBox.Show("All modifications processed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import modifications:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                PrgImport.Value = 0;
            }
        }

        private void OnGameStarted()
        {
            MnsHome.Enabled = false;
            PnlHomeSurface.Enabled = false;
            LblStatus.Text = $"Running - {_gameSession.CurrentGame!.Name} - {CmbProfiles.SelectedItem!.ToString()!.Trim()}";
        }

        private void OnGameExited()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnGameExited));
                return;
            }

            MnsHome.Enabled = true;
            PnlHomeSurface.Enabled = true;
            LblStatus.Text = $"Idle - {_gameSession.CurrentGame!.Name} - {CmbProfiles.SelectedItem!.ToString()!.Trim()}";
        }

        private void OnGameLoaded(GameModel game)
        {
            _isLoadingMods = true;
            LvwModifications.Items.Clear();

            if (game is null)
            {
                _isLoadingMods = false;
                return;
            }

            string gameFilename = $"{AppData.GamesPath}\\{game.Name}\\{AppData.GameFile}";
            RecentGamesData.Save(gameFilename);
            UpdateRecentMenu();

            PnlHomeSurface.Enabled = true;
            BtnRun.Text = $"  {game.Name}";
            BtnRun.TextAlign = ContentAlignment.MiddleLeft;

            try
            {
                var icon = Icon.ExtractAssociatedIcon(game.ExecutablePath);

                if (icon is not null)
                    BtnRun.Image = icon.ToBitmap();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Game executable not found:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            CmbProfiles.Items.Clear();
            CmbProfiles.Items.AddRange([.. game.Profiles.Select(p => $"  {p.Name}")]);
            CmbProfiles.SelectedIndex = 0;

            LblStatus.Text = $"Idle - {game.Name} - {CmbProfiles.SelectedItem!.ToString()!.Trim()}";

            var modifications = game.Profiles[CmbProfiles.SelectedIndex].Modifications;
            var activeMods = modifications.Where(m => m.IsEnabled).ToList();

            if (activeMods.Count > 0)
                _modDeployment.DeployModifications(game, activeMods);

            foreach (var modification in modifications)
            {
                ListViewItem value = new([
                    string.Empty,
                    modification.Name,
                    modification.Version,
                    modification.Category,
                    modification.InstalledAt.ToString()
                ])
                {
                    Checked = modification.IsEnabled,
                    Tag = modification
                };

                LvwModifications.Items.Add(value);
            }

            _isLoadingMods = false;
        }

        private void OnGameUnloaded()
        {
            if (_gameSession.CurrentGame is null)
                return;

            PnlHomeSurface.Enabled = false;
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextAlign = ContentAlignment.MiddleCenter;
            BtnRun.Image = null;
            CmbProfiles.Items.Clear();
            LblStatus.Text = "Press (Ctrl + O) to open a Bolt game file, or (Ctrl + N) to create a new one.";
            LvwModifications.Items.Clear();
        }

        private static void ShowModalWindow(Form form) => form.ShowDialog();

        private void UpdateRecentMenu()
        {
            recentToolStripMenuItem.DropDownItems.Clear();

            var recentGames = RecentGamesData.Load();

            foreach (var path in recentGames)
            {
                if (!File.Exists(path)) continue;

                var gameName = Path.GetFileName(Path.GetDirectoryName(path)) ?? "Unknown Game";

                var item = new ToolStripMenuItem(gameName)
                {
                    Tag = path
                };

                item.Click += (s, e) =>
                {
                    if (s is ToolStripMenuItem menu && menu.Tag is string gamePath)
                        _gameSession.LoadGame(gamePath);
                };

                recentToolStripMenuItem.DropDownItems.Add(item);
            }

            if (recentToolStripMenuItem.DropDownItems.Count > 0)
                recentToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            var clearItem = new ToolStripMenuItem("Clear History")
            {
                Enabled = recentGames.Count > 0
            };
            clearItem.Click += (s, e) =>
            {
                RecentGamesData.Clear();
                UpdateRecentMenu();
            };

            recentToolStripMenuItem.DropDownItems.Add(clearItem);
        }

        private void FrmHome_Load(object sender, EventArgs e)
        {
            string? gamesPath = ModificationsData.Load();

            if (!string.IsNullOrEmpty(gamesPath) && AppData.GamesPath != gamesPath)
                AppData.GamesPath = gamesPath;

            UpdateRecentMenu();

            BeginInvoke(new Action(() =>
            {
                var recentGames = RecentGamesData.Load();
                var lastGame = recentGames.FirstOrDefault();

                if (!string.IsNullOrEmpty(lastGame) && File.Exists(lastGame))
                    _gameSession.LoadGame(lastGame);
            }));
        }

        private void ToggleSelectedModifications(bool enable)
        {
            if (_isLoadingMods || LvwModifications.SelectedItems.Count == 0)
                return;

            var selectedItems = LvwModifications.SelectedItems.Cast<ListViewItem>().ToList();

            var modsToToggle = selectedItems
                .Select(i => (ModificationModel)i.Tag!)
                .Where(m => m.IsEnabled != enable)
                .ToList();

            if (modsToToggle.Count == 0)
                return;

            var currentGame = _gameSession.CurrentGame!;
            bool success = enable
                ? _modDeployment.DeployModifications(currentGame, modsToToggle)
                : _modDeployment.RevertModifications(currentGame, modsToToggle);

            if (!success)
                return;

            _isLoadingMods = true;

            foreach (var item in selectedItems)
            {
                if (item.Tag is ModificationModel mod && mod.IsEnabled != enable)
                {
                    mod.IsEnabled = enable;
                    item.Checked = enable;
                }
            }

            _isLoadingMods = false;

            string gameFilename = $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}";
            GameData.Save(currentGame, gameFilename);
        }
    }
}