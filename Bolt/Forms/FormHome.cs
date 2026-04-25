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
        }

        protected override void TerminateEvents()
        {
            _gameProcess.GameStarted -= OnGameStarted;
            _gameProcess.GameExited -= OnGameExited;

            _gameSession.GameLoaded -= OnGameLoaded;
            _gameSession.GameUnloaded -= OnGameUnloaded;
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
            LblStatus.Text = $"Running - {_gameSession.CurrentGame!.Name} - {CmbProfiles.SelectedItem}";
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
            LblStatus.Text = $"Idle - {_gameSession.CurrentGame!.Name} - {CmbProfiles.SelectedItem}";
        }

        private void OnGameLoaded(GameModel game)
        {
            LvwModifications.Items.Clear();

            if (game is null) return;

            PnlHomeSurface.Enabled = true;
            BtnRun.Text = $"  {game.Name}";
            BtnRun.TextAlign = ContentAlignment.MiddleLeft;
            BtnRun.Image = Icon.ExtractAssociatedIcon(game.ExecutablePath)!.ToBitmap();

            CmbProfiles.Items.Clear();
            CmbProfiles.Items.AddRange([.. game.Profiles.Select(p => $"  {p.Name}")]);
            CmbProfiles.SelectedIndex = 0;

            LblStatus.Text = $"Idle - {game.Name} - {CmbProfiles.SelectedItem}";

            var modifications = game.Profiles[CmbProfiles.SelectedIndex].Modifications;
            CreateSymbolicLinks(modifications);

            foreach (var modification in modifications)
            {
                ListViewItem value = new([
                    string.Empty,
                    modification.Name,
                    modification.Version,
                    modification.Category,
                    modification.InstalledAt.ToString()
                ]);

                LvwModifications.Items.Add(value);
            }
        }

        private void CreateSymbolicLinks(List<ModificationModel> modifications)
        {
            var currentGame = _gameSession.CurrentGame!;

            foreach (var modification in modifications)
            {
                string modBasePath = Path.Combine(currentGame.ModificationsPath, modification.Name);

                foreach (var sourcePath in modification.Content)
                {
                    string relativePath = Path.GetRelativePath(modBasePath, sourcePath);
                    string destinationPath = Path.Combine(currentGame.TargetPath, relativePath);

                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                    try
                    {
                        if ((File.Exists(destinationPath) || System.IO.Directory.Exists(destinationPath)) && !IsSymbolicLink(destinationPath))
                        {
                            string backupPath = Path.Combine(currentGame.BackupsPath, relativePath);
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

                            if (File.Exists(destinationPath))
                                File.Move(destinationPath, backupPath, true);
                        }

                        if (System.IO.Directory.Exists(sourcePath))
                            SymbolicLink.Create(destinationPath, sourcePath, Enums.SymbolicLinkType.Directory);
                        else if (File.Exists(sourcePath))
                            SymbolicLink.Create(destinationPath, sourcePath, Enums.SymbolicLinkType.File);
                    }
                    catch (IOException) { /* TODO: Log system */ }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create symlink for '{sourcePath}':\n{ex.Message}", "SymLink Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static bool IsSymbolicLink(string path)
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        private void OnGameUnloaded()
        {
            if (_gameSession.CurrentGame is null) return;

            PnlHomeSurface.Enabled = false;
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextAlign = ContentAlignment.MiddleCenter;
            BtnRun.Image = null;
            CmbProfiles.Items.Clear();
            LblStatus.Text = "Press (Ctrl + O) to open a Bolt game file, or (Ctrl + N) to create a new one.";
            LvwModifications.Items.Clear();
        }

        private static void ShowModalWindow(Form form) => form.ShowDialog();

        private void FrmHome_Load(object sender, EventArgs e)
        {
            string? gamesPath = ModificationsData.Load();
            if (!string.IsNullOrEmpty(gamesPath) && AppData.GamesPath != gamesPath)
            {
                AppData.GamesPath = gamesPath;
            }
        }
    }
}