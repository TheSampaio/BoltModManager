using Bolt.Data;

namespace Bolt.Forms
{
    public partial class FrmPreferences : Form
    {
        public FrmPreferences()
        {
            InitializeComponent();
        }

        private void BtnModifications_Click(object sender, EventArgs e)
        {
            if (FbdModifications.ShowDialog(this) == DialogResult.OK)
                TxyModifications.Value = FbdModifications.SelectedPath;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate entry
            if (string.IsNullOrWhiteSpace(TxyModifications.Value))
            {
                MessageBox.Show(
                    "Please select a valid modifications directory before continuing.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Ensure directory exists
            if (!Directory.Exists(TxyModifications.Value))
            {
                MessageBox.Show(
                    "The selected directory does not exist. Please choose a valid folder.",
                    "Invalid Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Save modifications directory path
            string gamesPath = TxyModifications.Value;
            AppData.GamesPath = gamesPath;
            ModificationsData.Save(gamesPath);

            MessageBox.Show(
                $"Modifications directory set to:\n{gamesPath}",
                "Modifications Directory Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmPreferences_Load(object sender, EventArgs e)
        {
            string preferencesData = ModificationsData.Load() ?? AppData.GamesPath;

            if (preferencesData is not null)
                TxyModifications.Value = preferencesData;
        }
    }
}
