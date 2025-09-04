using Bolt.Data;
using Bolt.Models;
using Bolt.Utilities;

namespace Bolt.Forms
{
    public partial class FrmNewGame : Form
    {
        public FrmNewGame()
        {
            InitializeComponent();
        }

        private void BtnTarget_Click(object sender, EventArgs e)
        {
            if (FbdTarget.ShowDialog(this) == DialogResult.OK)
                TxyTarget.Value = FbdTarget.SelectedPath;
        }

        private void BtnExecutable_Click(object sender, EventArgs e)
        {
            OfdExecutable.Title = "Select Executable";
            OfdExecutable.FileName = string.Empty;
            OfdExecutable.Filter = "Executable File (*.exe)|*.exe";
            OfdExecutable.InitialDirectory = TxyTarget.Value;

            if (OfdExecutable.ShowDialog(this) == DialogResult.OK)
            {
                // Validate if it's really an .exe file
                if (!(Path.GetExtension(OfdExecutable.FileName)?.ToLower() == ".exe"))
                {
                    MessageBox.Show(
                        "Please select a valid game executable file.",
                        "Invalid File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                TxyExecutable.Value = OfdExecutable.FileName;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validates if the entries are null or empty
            if (string.IsNullOrWhiteSpace(TxyName.Value)
                || string.IsNullOrWhiteSpace(TxyTarget.Value)
                || string.IsNullOrWhiteSpace(TxyExecutable.Value))
            {
                MessageBox.Show(
                    "Please enter values for the \"Name\", \"Target\" and \"Executable\" fields before continuing.",
                    "Invalid File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Validates if the result location already exists
            if (Directory.Exists(TxyResultLocation.Value))
            {
                MessageBox.Show(
                    "A game with this name already exists in the selected location.",
                    "Duplicate Game",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Create a game model in memory to be serialized
            var gameModel = new GameModel
            {
                Id = Guid.NewGuid(),
                Name = TxyName.Value,
                ExecutablePath = TxyExecutable.Value,
                Profiles =
                {
                    new ProfileModel
                    {
                        Id = Guid.NewGuid(),
                        Name = "Main",
                        BackupPath = $"{TxyResultLocation.Value}\\Backups",
                        PackagesPath = $"{TxyResultLocation.Value}\\Packages",
                        TargetPath = TxyTarget.Value,
                    }
                }
            };

            // Create the necessary directories
            Directory.CreateDirectory(TxyResultLocation.Value);
            Directory.CreateDirectory(gameModel.Profiles[0].BackupPath);
            Directory.CreateDirectory(gameModel.Profiles[0].PackagesPath);

            // Save the gameModel as Json file
            string gameFilePath = Path.Combine(TxyResultLocation.Value, AppData.GameFile);
            GameData.Save(gameModel, gameFilePath);

            // Feedback the user and close the form
            MessageBox.Show(
                    $"Game saved into \"{gameFilePath}\" directory.",
                    "Game File Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
            );

            Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmNewGame_Load(object sender, EventArgs e)
        {
            TxyResultLocation.Value = $"{PackageData.DirectoryValue}\\";
        }

        private void TxyName_ValueChanged(object sender, EventArgs e)
        {
            TxyResultLocation.Value = $"{PackageData.DirectoryValue}\\{TxyName.Value}";
        }
    }
}
