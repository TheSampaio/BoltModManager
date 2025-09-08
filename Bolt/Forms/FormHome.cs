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
                // Validate if it's really a .bltg file
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
                        MessageBoxButtons.OK
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

                    // Count only files for accurate progress bar
                    int total = archive.Entries.Count(e => !string.IsNullOrEmpty(e.Name));
                    int current = 0;

                    PrgImport.Minimum = 0;
                    PrgImport.Maximum = total;
                    PrgImport.Value = 0;

                    // Detect top-level folder
                    string topLevelFolder = archive.Entries
                        .Where(e => !string.IsNullOrEmpty(e.FullName) && e.FullName.Contains('/'))
                        .Select(e => e.FullName.Split('/')[0])
                        .FirstOrDefault() ?? string.Empty;

                    // Initialize modification content list
                    List<string> modificationContent = [];

                    foreach (var entry in archive.Entries)
                    {
                        string relativePath = entry.FullName;

                        // Trim top-level folder if needed
                        if (!string.IsNullOrEmpty(topLevelFolder) && relativePath.StartsWith(topLevelFolder))
                            relativePath = relativePath[topLevelFolder.Length..].TrimStart('/', '\\');

                        // Normalize path (only path separators and invalid path chars)
                        relativePath = relativePath
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Trim()
                            .Replace('/', Path.DirectorySeparatorChar)
                            .Replace('\\', Path.DirectorySeparatorChar);

                        foreach (char invalidChar in Path.GetInvalidPathChars())
                            relativePath = relativePath.Replace(invalidChar, '_');

                        string destinationFile = Path.Combine(destinationPath, relativePath);

                        // Split into directory and file
                        string directory = Path.GetDirectoryName(destinationFile)!;
                        string fileName = Path.GetFileName(destinationFile);

                        // Sanitize only the file name
                        foreach (char invalidChar in Path.GetInvalidFileNameChars())
                            fileName = fileName.Replace(invalidChar, '_');

                        // Recombine directory and file
                        destinationFile = Path.Combine(directory, fileName);

                        if (!string.IsNullOrEmpty(relativePath))
                            modificationContent.Add(destinationFile);

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            // Entry is a directory
                            System.IO.Directory.CreateDirectory(destinationFile);
                            continue; // Do not count directories in progress
                        }

                        // Entry is a file
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                        await Task.Run(() => entry.ExtractToFile(destinationFile, true));

                        // Update progress bar for files
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
                        Content = modificationContent
                    };

                    // Save mod to current profile
                    currentProfile.Modifications.Add(currentModification);
                    GameData.Save(currentGame, $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}");

                    // Populate list view (temporary)
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
                        MessageBoxButtons.OK
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

                // Reset after success
                PrgImport.Value = 0;
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
            // Clear ListView
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

            // Enable panel
            PnlHomeSurface.Enabled = true;

            // Update Run button
            BtnRun.Text = $"  {game.Name}";
            BtnRun.TextAlign = ContentAlignment.MiddleLeft;
            BtnRun.Image = Icon.ExtractAssociatedIcon(game.ExecutablePath)!.ToBitmap();

            // Update profiles combo box
            CmbProfiles.Items.Clear();
            CmbProfiles.Items.AddRange([.. game.Profiles.Select(p => p.Name)]);
            CmbProfiles.SelectedIndex = 0;

            // Update status label
            LblStatus.Text = $"Idle - {game.Name} - {CmbProfiles.SelectedItem}";

            // Get modifications
            var modifications = game.Profiles[CmbProfiles.SelectedIndex].Modifications;

            // Create symbolic links for mods
            CreateSymbolicLinks(modifications);

            // Update UI
            foreach (var modification in modifications)
            {
                ListViewItem value = new(
                [
                    string.Empty,                       // Activate
                    modification.Name,                  // Name
                    modification.Version,               // Version
                    modification.Category,              // Category
                    modification.InstalledAt.ToString() // Imported On
                ]);

                LvwModifications.Items.Add(value);
            }
        }

        private static void CreateSymbolicLinks(List<ModificationModel> modifications)
        {
            var currentGame = GameSessionService.Instance.CurrentGame!;

            foreach (var modification in modifications)
            {
                // Base mod path
                string modBasePath = Path.Combine(currentGame.ModificationsPath, modification.Name);

                foreach (var item in modification.Content)
                {
                    // Full path of the file/directory in Modifications
                    string sourcePath = item;

                    // Relative file path to the mod folder
                    string relativePath = Path.GetRelativePath(modBasePath, sourcePath);

                    // Final path within TargetPath maintaining a folder structure
                    string destinationPath = Path.Combine(currentGame.TargetPath, relativePath);

                    // Certifies that the parent directory exists
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                    try
                    {
                        // Backup of the original file if it exists (BUGGY)
                        // TODO: Fix backup of symbolic links
                        //if (File.Exists(destinationPath))
                        //{
                        //    string backupPath = Path.Combine(currentGame.BackupsPath, relativePath);
                        //    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                        //    File.Move(destinationPath, backupPath, true);
                        //}

                        // Backup original only if it's not a symlink
                        // TODO: Fix backups of mod files inside symlinks directories
                        if ((File.Exists(destinationPath) || System.IO.Directory.Exists(destinationPath)))
                        {
                            if (!IsSymbolicLink(destinationPath))
                            {
                                // Backup real files or folders
                                string backupPath = Path.Combine(currentGame.BackupsPath, relativePath);
                                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

                                if (File.Exists(destinationPath))
                                    File.Move(destinationPath, backupPath, true);

                                //else if (System.IO.Directory.Exists(destinationPath))
                                //    System.IO.Directory.Move(destinationPath, backupPath);
                            }
                        }

                        // Create the symbolic link for directories
                        if (System.IO.Directory.Exists(sourcePath))
                            SymbolicLink.Create(destinationPath, sourcePath, Enums.SymbolicLinkType.Directory);

                        // Create the symbolic link for files
                        else if (File.Exists(sourcePath))
                            SymbolicLink.Create(destinationPath, sourcePath, Enums.SymbolicLinkType.File);
                    }

                    catch (IOException) { }

                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Failed to create symbolic link for '{sourcePath}':\n{ex.Message}",
                            "Symbolic Link Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
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
            if (GameSessionService.Instance.CurrentGame is null)
                return;

            // Disable panel
            PnlHomeSurface.Enabled = false;

            // Update Run button
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextAlign = ContentAlignment.MiddleCenter;
            BtnRun.Image = null;

            // Clear profiles combo box
            CmbProfiles.Items.Clear();

            // Reset status label
            LblStatus.Text = "Press (Ctrl + O) to open a Bolt game file, or (Ctrl + N) to create a new one.";

            // Clear ListView
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
