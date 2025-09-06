using Bolt.Base;
using Bolt.Data;
using Bolt.Models;
using Bolt.Services;
using Bolt.Utilities;
using System.IO.Compression;

namespace Bolt.Forms
{
    public partial class FrmHome : EventfulForm
    {
        public FrmHome()
        {
            InitializeComponent();
        }

        protected override void InitializeEvents()
        {
            // Game session
            GameSessionService.Instance.GameLoaded += OnGameLoaded;
            GameSessionService.Instance.GameUnloaded += OnGameUnloaded;

            // Game process
            GameProcessService.Instance.GameStarted += OnGameStarted;
            GameProcessService.Instance.GameExited += OnGameExited;
        }

        protected override void TerminateEvents()
        {
            // Game process
            GameProcessService.Instance.GameStarted -= OnGameStarted;
            GameProcessService.Instance.GameExited -= OnGameExited;

            // Game session
            GameSessionService.Instance.GameLoaded -= OnGameLoaded;
            GameSessionService.Instance.GameUnloaded -= OnGameUnloaded;
        }

        private void NewGame_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowModalWindow(new FrmNewGame());
        }

        private void OpenGame_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OfdOpenGame.Title = "Open Game";
            OfdOpenGame.FileName = string.Empty;
            OfdOpenGame.Filter = "Bolt Game File (*.bltg)|*.bltg";
            OfdOpenGame.InitialDirectory = PackageData.Load();
            OfdOpenGame.Multiselect = false;

            if (OfdOpenGame.ShowDialog() == DialogResult.OK)
            {
                // Validate if it's really an .exe file
                if (!(Path.GetExtension(OfdOpenGame.FileName)?.ToLower() == ".bltg"))
                {
                    MessageBox.Show(
                        "Please select a valid Bolt game file.",
                        "Invalid File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                GameSessionService.Instance.LoadGame(OfdOpenGame.FileName);
            }
        }

        private void QuitGame_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnGameUnloaded();
        }

        private void Quit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Settings_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowModalWindow(new FrmPreferences());
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            if (GameSessionService.Instance.CurrentGame is null)
            {
                MessageBox.Show(
                    $"Unable to launch the game. The file \"{OfdOpenGame.FileName}\" could not be loaded.",
                    "Game Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            // Run the current game
            GameProcessService.Instance.RunGame(GameSessionService.Instance.CurrentGame.ExecutablePath);
        }

        private async void BtnImport_Click(object sender, EventArgs e)
        {
            // TODO: Uncrompress Zip file and update the progress bar
            // TODO: Move within content to "CURRENT_GAME\Packages\"
            // TODO: Match CURRENT_GAME files and move it to "CURRENT_GAME\Backups\"
            // TODO: Create the symbolic links between "Packages\MOD_FOLDER\" and CURRENT_GAME directory
            // TODO: Save everything inside the Json game file (game.bltg)
            // TODO: Populate LvwPackages

            // Configure OpenFileDialog
            OfdOpenGame.Title = "Import Package";
            OfdOpenGame.FileName = string.Empty;
            OfdOpenGame.Filter = "Zip File (*.zip)|*.zip";
            OfdOpenGame.InitialDirectory = string.Empty;
            OfdOpenGame.Multiselect = true;

            if (OfdOpenGame.ShowDialog() != DialogResult.OK)
                return;

            var selectedFiles = OfdOpenGame.FileNames;
            var currentGame = GameSessionService.Instance.CurrentGame!;
            var currentProfile = currentGame.Profiles[CmbProfiles.SelectedIndex];
            var packagesPath = currentProfile.PackagesPath;

            foreach (var selectedFile in selectedFiles)
            {
                // Validate file extension
                if (Path.GetExtension(selectedFile)?.ToLower() != ".zip")
                {
                    MessageBox.Show(
                        $"Invalid file skipped: {Path.GetFileName(selectedFile)}",
                        "Invalid File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    continue;
                }

                try
                {
                    string packageName = Path.GetFileNameWithoutExtension(selectedFile);
                    string destinationPath = Path.Combine(packagesPath, packageName);

                    // Delete existing package folder if it exists
                    if (System.IO.Directory.Exists(destinationPath))
                        System.IO.Directory.Delete(destinationPath, true);

                    System.IO.Directory.CreateDirectory(destinationPath);

                    using (var archive = Archive.OpenRead(selectedFile))
                    {
                        int total = archive.Entries.Count;
                        int current = 0;

                        PrgImport.Minimum = 0;
                        PrgImport.Maximum = total;
                        PrgImport.Value = 0;

                        string topLevelFolder = archive.Entries
                            .Where(e => !string.IsNullOrEmpty(e.FullName) && e.FullName.Contains('/'))
                            .Select(e => e.FullName.Split('/')[0])
                            .FirstOrDefault() ?? string.Empty;

                        foreach (var entry in archive.Entries)
                        {
                            string relativePath = entry.FullName;

                            if (!string.IsNullOrEmpty(topLevelFolder))
                                relativePath = relativePath[topLevelFolder.Length..].TrimStart('/', '\\');

                            string destinationFile = Path.Combine(destinationPath, relativePath);

                            if (string.IsNullOrEmpty(entry.Name))
                                System.IO.Directory.CreateDirectory(destinationFile);

                            else
                            {
                                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                                await Task.Run(() => entry.ExtractToFile(destinationFile, true));
                            }

                            current++;
                            PrgImport.Value = current;
                            PrgImport.Refresh();
                        }
                    }

                    LvwPackages.Items.Add(new ListViewItem(
                    [
                        string.Empty,
                        packageName,
                        string.Empty,
                        string.Empty,
                        DateTime.UtcNow.ToString()
                    ]));

                    PrgImport.Value = 0;
                }

                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to import package \"{Path.GetFileName(selectedFile)}\":\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            MessageBox.Show(
                "All packages processed successfully!",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void OnGameStarted()
        {
            const bool ENABLE = false;

            MnsHome.Enabled = ENABLE;
            PnlHomeSurface.Enabled = ENABLE;

            LblStatus.Text = $"Running - {GameSessionService.Instance.CurrentGame!.Name} - {CmbProfiles.SelectedItem}";
        }

        private void OnGameExited()
        {
            const bool ENABLE = true;

            // Reinvoke this method on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(OnGameExited));
                return;
            }

            MnsHome.Enabled = ENABLE;
            PnlHomeSurface.Enabled = ENABLE;

            LblStatus.Text = $"Idle - {GameSessionService.Instance.CurrentGame!.Name} - {CmbProfiles.SelectedItem}";
        }

        private void OnGameLoaded(GameModel game)
        {
            if (game is null)
            {
                MessageBox.Show(
                    "Failure to load the game file.",
                    "Invalid File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            // Panel Home Surface
            PnlHomeSurface.Enabled = true;

            // Button Run
            BtnRun.Text = $"  {game.Name}";
            BtnRun.TextAlign = ContentAlignment.MiddleLeft;
            BtnRun.Image = Icon.ExtractAssociatedIcon(game.ExecutablePath)!.ToBitmap();

            // Combo Box Profiles
            CmbProfiles.Items.Clear();
            CmbProfiles.Items.AddRange([.. game.Profiles.Select(p => p.Name)]);
            CmbProfiles.SelectedIndex = 0;

            // Label Status
            LblStatus.Text = $"Idle - {game.Name} - {CmbProfiles.SelectedItem}";
        }

        private void OnGameUnloaded()
        {
            if (GameSessionService.Instance.CurrentGame is null)
                return;

            // Panel Home Surface
            PnlHomeSurface.Enabled = false;

            // Button Run
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextAlign = ContentAlignment.MiddleCenter;
            BtnRun.Image = null;

            // Combo Box Profiles
            CmbProfiles.Items.Clear();

            // Label Status
            LblStatus.Text = "Press (Ctrl + O) to open a Bolt game file, or (Ctrl + N) to create a new one.";
        }

        private static void ShowModalWindow(Form form) => form.ShowDialog();
    }
}
