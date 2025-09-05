using Bolt.Data;
using Bolt.Models;
using Bolt.Services;
using System.Diagnostics;

namespace Bolt.Forms
{
    public partial class FrmHome : Form
    {
        public FrmHome()
        {
            InitializeComponent();

            GameSessionService.Instance.GameLoaded += OnGameLoaded;
            GameSessionService.Instance.GameUnloaded += OnGameUnloaded;
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

            string ExecutablePath = GameSessionService.Instance.CurrentGame.ExecutablePath;
            var startInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                WorkingDirectory = Path.GetDirectoryName(ExecutablePath),
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
        }

        private static void ShowModalWindow(Form form) => form.ShowDialog();

        private void FrmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            GameSessionService.Instance.GameLoaded -= OnGameLoaded;
            GameSessionService.Instance.GameUnloaded -= OnGameUnloaded;
        }
    }
}
