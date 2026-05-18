using Bolt.Data;
using Bolt.Models;
using Bolt.Interfaces;

namespace Bolt.Forms
{
    public partial class FrmNewGame : Form
    {
        private readonly IGameSessionService _gameSession;

        public FrmNewGame(IGameSessionService gameSession)
        {
            _gameSession = gameSession;
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
            if (string.IsNullOrWhiteSpace(TxyName.Value) || string.IsNullOrWhiteSpace(TxyTarget.Value) || string.IsNullOrWhiteSpace(TxyExecutable.Value))
            {
                MessageBox.Show("Please enter values for the \"Name\", \"Target\" and \"Executable\" fields before continuing.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Directory.Exists(TxyResultLocation.Value))
            {
                MessageBox.Show("A game with this name already exists in the selected location.", "Duplicate Game", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string gameLocation = TxyResultLocation.Value;

            var gameModel = new GameModel
            {
                Id = Guid.NewGuid(),
                Name = TxyName.Value,
                ExecutablePath = TxyExecutable.Value,
                BackupsPath = $"{gameLocation}\\Backups",
                ModificationsPath = $"{gameLocation}\\Modifications",
                TargetPath = TxyTarget.Value,
                Profiles = { new() { Id = Guid.NewGuid(), Name = "Main" } }
            };

            Directory.CreateDirectory(gameLocation);
            Directory.CreateDirectory(gameModel.BackupsPath);
            Directory.CreateDirectory(gameModel.ModificationsPath);

            string gameFilePath = Path.Combine(TxyResultLocation.Value, AppData.GameFile);
            GameData.Save(gameModel, gameFilePath);

            MessageBox.Show($"Game saved into \"{gameFilePath}\" directory.", "Game File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _gameSession.LoadGame(gameFilePath);

            Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmNewGame_Load(object sender, EventArgs e)
        {
            string? gamesPath = ModificationsData.Load();

            if (gamesPath is null)
            {
                gamesPath = AppData.GamesPath;
                ModificationsData.Save(gamesPath);
            }

            TxyResultLocation.Value = $"{gamesPath}\\";
        }

        private void TxyTarget_ValueChanged(object sender, EventArgs e)
        {
            TxyName.Value = Path.GetFileName(TxyTarget.Value);
        }

        private void TxyName_ValueChanged(object sender, EventArgs e)
        {
            TxyResultLocation.Value = $"{ModificationsData.Load()}\\{TxyName.Value}";
        }
    }
}
