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

        private bool _isLoadingMods = false;

        internal FrmHome(
            IGameSessionService gameSession,
            IGameProcessService gameProcess,
            IModImportService modImportService)
        {
            _gameSession = gameSession;
            _gameProcess = gameProcess;
            _modImportService = modImportService;

            InitializeComponent();
        }

        protected override void InitializeEvents()
        {
            _gameSession.GameLoaded += OnGameLoaded;
            _gameSession.GameUnloaded += OnGameUnloaded;

            _gameProcess.GameStarted += OnGameStarted;
            _gameProcess.GameExited += OnGameExited;

            LvwModifications.ItemCheck += LvwModifications_ItemCheck;
        }

        protected override void TerminateEvents()
        {
            _gameProcess.GameStarted -= OnGameStarted;
            _gameProcess.GameExited -= OnGameExited;

            _gameSession.GameLoaded -= OnGameLoaded;
            _gameSession.GameUnloaded -= OnGameUnloaded;

            LvwModifications.ItemCheck -= LvwModifications_ItemCheck;
        }

        private void LvwModifications_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isLoadingMods)
                return;

            var item = LvwModifications.Items[e.Index];

            if (item.Tag is not ModificationModel mod)
                return;

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

                RestoreBackups(mod);
                mod.IsEnabled = false;
            }
            else if (e.NewValue == CheckState.Checked)
            {
                CreateSymbolicLinks([mod]);
                mod.IsEnabled = true;
            }

            var currentGame = _gameSession.CurrentGame!;
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
                Path.GetExtension(file)?.ToLower() == ".zip" ? Archive.OpenRead(file).Entries.Count(x => !string.IsNullOrEmpty(x.Name)) : 0);

            PrgImport.Maximum = totalFiles;

            var progress = new Progress<int>(value =>
            {
                PrgImport.Value = value;
                PrgImport.Refresh();
            });

            try
            {
                await _modImportService.ImportModsAsync(OfdOpenGame.FileNames, currentGame, currentProfile, progress);

                MessageBox.Show("All modifications processed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                string gameFilename = $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}";
                _gameSession.LoadGame(gameFilename);
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

            CreateSymbolicLinks([.. modifications.Where(m => m.IsEnabled)]);

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

        private void CreateSymbolicLinks(List<ModificationModel> modifications)
        {
            var currentGame = _gameSession.CurrentGame!;
            var operations = new List<LinkOperationModel>();

            foreach (var modification in modifications)
            {
                string modBasePath = Path.Combine(currentGame.ModificationsPath, modification.Name);

                foreach (var sourcePath in modification.Content)
                {
                    if (System.IO.Directory.Exists(sourcePath)) continue;

                    string relativePath = Path.GetRelativePath(modBasePath, sourcePath);

                    operations.Add(new LinkOperationModel
                    {
                        Action = "Link",
                        SourcePath = sourcePath,
                        DestinationPath = Path.Combine(currentGame.TargetPath, relativePath),
                        BackupPath = Path.Combine(currentGame.BackupsPath, relativePath)
                    });
                }
            }

            ExecuteElevatedHelper(operations);
        }

        private void RestoreBackups(ModificationModel modification)
        {
            var currentGame = _gameSession.CurrentGame!;
            string modBasePath = Path.Combine(currentGame.ModificationsPath, modification.Name);
            var operations = new List<LinkOperationModel>();

            foreach (var sourcePath in modification.Content)
            {
                if (System.IO.Directory.Exists(sourcePath)) continue;

                string relativePath = Path.GetRelativePath(modBasePath, sourcePath);

                operations.Add(new LinkOperationModel
                {
                    Action = "Restore",
                    DestinationPath = Path.Combine(currentGame.TargetPath, relativePath),
                    BackupPath = Path.Combine(currentGame.BackupsPath, relativePath)
                });
            }

            ExecuteElevatedHelper(operations);
        }

        private static void ExecuteElevatedHelper(List<LinkOperationModel> operations)
        {
            if (operations.Count == 0) return;

            // Usando a sua classe Json já existente para criar o arquivo de comunicação
            string manifestPath = Path.Combine(Path.GetTempPath(), $"BoltManifest_{Guid.NewGuid():N}.json");
            Json.Serialize(operations, manifestPath);

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Environment.ProcessPath, // Chama o próprio Bolt.exe
                    Arguments = $"--elevated-helper \"{manifestPath}\"",
                    UseShellExecute = true,
                    Verb = "runas", // Solicita a tela do UAC exatamente aqui
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                using var process = System.Diagnostics.Process.Start(psi);

                // A UI do app principal vai aguardar (congelar) até o UAC ser aprovado e os links serem criados.
                // Se preferir não congelar a UI principal, pode transformar esse método em async e usar await process.WaitForExitAsync()
                process?.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("A elevação de privilégios foi cancelada pelo usuário. Os mods não foram aplicados corretamente.", "Operação Cancelada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (File.Exists(manifestPath)) File.Delete(manifestPath);
            }
        }

        private static bool IsSymbolicLink(string path)
        {
            if (!File.Exists(path) && !System.IO.Directory.Exists(path))
                return false;

            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
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

            // Load the most recent game if available
            BeginInvoke(new Action(() =>
            {
                var recentGames = RecentGamesData.Load();
                var lastGame = recentGames.FirstOrDefault();

                if (!string.IsNullOrEmpty(lastGame) && File.Exists(lastGame))
                {
                    _gameSession.LoadGame(lastGame);
                }
            }));
        }
    }
}