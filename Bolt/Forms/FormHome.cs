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

                LoadGameModel(GameData.Load(OfdOpenGame.FileName)!);
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

            var process = Process.Start(startInfo);

            if (process is null)
                return;

            // Disables components
            EnableComponents(false);

            process.EnableRaisingEvents = true;
            process.Exited += (s, ev) =>
            {
                // Enables components
                Invoke(() => EnableComponents(true));
            };
        }

        private void EnableComponents(bool value)
        {
            MnsHome.Enabled = value;

            PnlHomeSurface.Enabled = value;
            PnlHomeSurface.Visible = value;
        }

        private void LoadGameModel(GameModel gameModel)
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
            BtnRun.Text = $"  {_currentGameModel.Name}";
            BtnRun.TextAlign = ContentAlignment.MiddleLeft;
            BtnRun.Image = Icon.ExtractAssociatedIcon(_currentGameModel.ExecutablePath)!.ToBitmap();
            BtnRun.Padding = new(10);

            CmbProfiles.Enabled = true;
        }

        private void UnloadGameModel()
        {
            if (_currentGameModel is null)
                return;

            BtnRun.Enabled = false;
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextAlign = ContentAlignment.MiddleCenter;
            BtnRun.Image = null;
            CmbProfiles.Enabled = true;
        }

        private static void ShowModalWindow(Form form)
        {
            var frmModal = form;
            frmModal.ShowDialog();
        }
    }
}
