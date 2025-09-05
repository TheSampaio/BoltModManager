using Bolt.Data;
using Bolt.Models;
using Bolt.Services;

namespace Bolt.Forms
{
    public partial class FrmHome : Form
    {
        public FrmHome()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // Game session
            GameSessionService.Instance.GameLoaded += OnGameLoaded;
            GameSessionService.Instance.GameUnloaded += OnGameUnloaded;

            // Game process
            GameProcessService.Instance.GameStarted += OnGameStarted;
            GameProcessService.Instance.GameExited += OnGameExited;
        }

        private void TerminateEvents()
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

        private void FrmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            TerminateEvents();
        }
    }
}
