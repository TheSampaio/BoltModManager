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
            OfdOpenGame.InitialDirectory = ModificationsData.Load();
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
            // Configure OpenFileDialog
            OfdOpenGame.Title = "Import Package";
            OfdOpenGame.FileName = string.Empty;
            OfdOpenGame.Filter = "Zip File (*.zip)|*.zip";
            OfdOpenGame.InitialDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"
            );
            OfdOpenGame.Multiselect = true;

            if (OfdOpenGame.ShowDialog() != DialogResult.OK)
                return;

            var selectedFiles = OfdOpenGame.FileNames;
            var currentGame = GameSessionService.Instance.CurrentGame!;
            var modificationsPath = currentGame.ModificationsPath;
            var currentProfile = currentGame.Profiles[CmbProfiles.SelectedIndex];

            // Reset progress bar before processing
            PrgImport.Value = 0;

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
                    string modificationName = Path.GetFileNameWithoutExtension(selectedFile);
                    string destinationPath = Path.Combine(modificationsPath, modificationName);

                    // Delete existing package folder if it exists
                    if (System.IO.Directory.Exists(destinationPath))
                        System.IO.Directory.Delete(destinationPath, true);

                    // Create a subdirectory for the mod inside Modifications
                    System.IO.Directory.CreateDirectory(destinationPath);

                    using var archive = Archive.OpenRead(selectedFile);

                    int total = archive.Entries.Count;
                    int current = 0;

                    PrgImport.Minimum = 0;
                    PrgImport.Maximum = total;
                    PrgImport.Value = 0;

                    // Try to detect a top-level folder
                    string topLevelFolder = archive.Entries
                        .Where(e => !string.IsNullOrEmpty(e.FullName) && e.FullName.Contains('/'))
                        .Select(e => e.FullName.Split('/')[0])
                        .FirstOrDefault() ?? string.Empty;

                    foreach (var entry in archive.Entries)
                    {
                        string relativePath = entry.FullName;

                        // Only trim if the entry actually starts with the top-level folder
                        if (!string.IsNullOrEmpty(topLevelFolder) && relativePath.StartsWith(topLevelFolder))
                            relativePath = relativePath[topLevelFolder.Length..].TrimStart('/', '\\');

                        // 🔧 Normalize path (remove quebras de linha, chars inválidos, etc.)
                        relativePath = relativePath
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Trim()
                            .Replace('/', Path.DirectorySeparatorChar)
                            .Replace('\\', Path.DirectorySeparatorChar);

                        foreach (char invalidChar in Path.GetInvalidPathChars())
                            relativePath = relativePath.Replace(invalidChar, '_');

                        foreach (char invalidChar in Path.GetInvalidFileNameChars())
                            relativePath = relativePath.Replace(invalidChar, '_');

                        string destinationFile = Path.Combine(destinationPath, relativePath);

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            // Directory entry
                            System.IO.Directory.CreateDirectory(destinationFile);
                        }

                        else
                        {
                            // File entry
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                            await Task.Run(() => entry.ExtractToFile(destinationFile, true));
                        }

                        current++;
                        PrgImport.Value = current;
                        PrgImport.Refresh();
                    }

                    // Create modification model (once per mod)
                    var currentModification = new ModificationModel()
                    {
                        Id = Guid.NewGuid(),
                        Name = modificationName,
                        Version = "N/A",
                        Category = "N/A",
                        InstalledAt = DateTime.UtcNow,
                    };

                    // Save mod to current profile
                    currentProfile.Modifications.Add(currentModification);
                    GameData.Save(currentGame, $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}");

                    // Populate list view (Temporary)
                    LvwModifications.Items.Add(new ListViewItem(
                    [
                        string.Empty,                               // Activate
                        modificationName,                           // Name
                        currentModification.Version,                // Version
                        currentModification.Category,               // Category
                        currentModification.InstalledAt.ToString()  // Imported On
                    ]));
                }

                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to import modification \"{Path.GetFileName(selectedFile)}\":\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            // Success message if progress bar reached max
            if (PrgImport.Value == PrgImport.Maximum)
            {
                MessageBox.Show(
                    "All modifications processed successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                PrgImport.Value = 0; // Reset after success
            }
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
            // ListView Imports
            LvwModifications.Items.Clear();

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

            // ListView Imports
            var modifications = game.Profiles[CmbProfiles.SelectedIndex].Modifications;

            foreach (var modification in modifications)
            {
                LvwModifications.Items.Add(new ListViewItem(
                [
                    string.Empty,                       // Activate
                    modification.Name,                  // Name
                    modification.Version,               // Version
                    modification.Category,              // Category
                    modification.InstalledAt.ToString() // Imported On
                ]));
            }
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

            // ListView Imports
            LvwModifications.Items.Clear();
        }

        private static void ShowModalWindow(Form form) => form.ShowDialog();

        private void FrmHome_Load(object sender, EventArgs e)
        {
            string? gamesPath = ModificationsData.Load();

            if (!string.IsNullOrEmpty(gamesPath))
            {
                // Update only if different
                if (AppData.GamesPath != gamesPath)
                    AppData.GamesPath = gamesPath;
            }
        }
    }
}
