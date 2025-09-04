using Bolt.Data;
using Bolt.Models;
using System.Diagnostics;

namespace Bolt.Forms
{
    public partial class FrmHome : Form
    {
        private GameModel? _currentGameModel;

        public FrmHome()
        {
            InitializeComponent();
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

                LoadGameModelFromDisk(GameData.Load(OfdOpenGame.FileName)!);
            }
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
            if (_currentGameModel is null)
            {
                MessageBox.Show(
                    $"Unable to launch the game. The file \"{OfdOpenGame.FileName}\" could not be loaded.",
                    "Game Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _currentGameModel.ExecutablePath,
                WorkingDirectory = Path.GetDirectoryName(_currentGameModel.ExecutablePath),
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        private void LoadGameModelFromDisk(GameModel gameModel)
        {
            _currentGameModel = gameModel;

            if (_currentGameModel is null)
            {
                MessageBox.Show(
                    $"Failure to load the game file \"{OfdOpenGame.FileName}\".",
                    "Invalid File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            BtnRun.Enabled = true;
        }

        private static void ShowModalWindow(Form form)
        {
            var frmModal = form;
            frmModal.ShowDialog();
        }
    }
}
