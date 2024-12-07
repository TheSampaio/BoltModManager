using Bolt.Source.Tools;
using System.Diagnostics;

namespace Bolt.Source
{
    public partial class FormMods : Form
    {
        private readonly string? _pathToGame = JsonManager.ReadJson("GameDirectory");
        private readonly string? _pathToMods = JsonManager.ReadJson("ModsDirectory");

        public FormMods()
        {
            InitializeComponent();
        }

        private void Pnl_DropFile_DragDrop(object sender, DragEventArgs e)
        {
            // Safely cast the data to string[] with a null-check
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true &&
                e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                foreach (string path in paths)
                {
                    try
                    {
                        // Get the name of the file or folder
                        string name = Path.GetFileName(path);

                        if (_pathToGame == null || _pathToMods == null)
                        {
                            MessageBox.Show("Game directory or Mods directory is not set.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string targetPath = Path.Combine(_pathToMods, name);
                        string linkPath = Path.Combine(_pathToGame, name);

                        // Ensure the target directory exists
                        Directory.CreateDirectory(_pathToMods);

                        // Check if the path is a directory or a file
                        if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            // Check if the directory already exists
                            if (Directory.Exists(targetPath))
                            {
                                MessageBox.Show($"Directory already exists: {targetPath}", "Warning",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            // Copy the directory to handle cross-volume moves
                            CopyDirectory(path, targetPath);

                            // Delete the original directory
                            Directory.Delete(path, true);

                            // Create the symbolic link
                            CreateSymbolicLink(linkPath, targetPath, true);
                        }

                        else
                        {
                            // Check if the file already exists
                            if (File.Exists(targetPath))
                            {
                                MessageBox.Show($"File already exists: {targetPath}", "Warning",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            // Move the file
                            File.Move(path, targetPath);

                            // Create the symbolic link after moving the file
                            CreateSymbolicLink(linkPath, targetPath);
                        }
                    }

                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show($"Access denied: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile);
            }

            // Copy all subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        public static void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory = false)
        {
            if (isDirectory)
                ExecuteAsAdministrator($"MKLINK /D \"{path}\" \"{pathToTarget}\"");

            else
                ExecuteAsAdministrator($"MKLINK \"{path}\" \"{pathToTarget}\"");
        }

        private static void ExecuteAsAdministrator(string command)
        {
            try
            {
                // Create a new process to run the command
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                Process process = new() { StartInfo = processStartInfo };
                process.Start();
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error running command: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Pnl_DropFile_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) ?
                DragDropEffects.Copy : DragDropEffects.None;
        }

        private void FormMods_Shown(object sender, EventArgs e)
        {
            // Setup Data Grid View

            // Columns
            Dgv_ModsTable.Columns.Add("Status", "Status");
            Dgv_ModsTable.Columns.Add("ModName", "Name");
            Dgv_ModsTable.Columns.Add("Version", "Version");
            Dgv_ModsTable.Columns.Add("Category", "Category");
            Dgv_ModsTable.Columns.Add("Time", "Time");

            // Rows
            Dgv_ModsTable.Rows.Add("Enabled", "FixerPack.zip", "2.2.4", "Fix", "7 Days");
            Dgv_ModsTable.Rows.Add("Disabled", "MainMenuOverhaul.zip", "1.1.2", "UI", "3 Days");
            Dgv_ModsTable.Rows.Add("Enabled", "BetterPhysics.zip", "1.1.6", "Fix", "Today");
            Dgv_ModsTable.Rows.Add("Enabled", "SkipIntro.zip", "1.1.0", "Fix", "Today");
        }
    }
}
