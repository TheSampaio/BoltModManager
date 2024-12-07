using Bolt.Source.Tools;

namespace Bolt.Source
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            try
            {
                // Load settings from JSON and populate the text fields
                Txt_GameDirectory.Text = JsonManager.ReadJson("GameDirectory") ?? string.Empty;
                Txt_ModsDirectory.Text = JsonManager.ReadJson("ModsDirectory") ?? string.Empty;
            }

            catch (Exception ex)
            {
                // Handle any exceptions that might occur when reading the settings
                MessageBox.Show($"Failed to load settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Btn_GamePath_Click(object sender, EventArgs e)
        {
            if (Fbd_Settings.ShowDialog() == DialogResult.OK)
                Txt_GameDirectory.Text = Fbd_Settings.SelectedPath;
        }

        private void Btn_ModsPath_Click(object sender, EventArgs e)
        {
            if (Fbd_Settings.ShowDialog() == DialogResult.OK)
                Txt_ModsDirectory.Text = Fbd_Settings.SelectedPath;
        }

        private void Btn_SaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // Save the settings to the JSON file
                bool gameDirectorySaved = JsonManager.WriteJason("GameDirectory", Txt_GameDirectory.Text);
                bool modsDirectorySaved = JsonManager.WriteJason("ModsDirectory", Txt_ModsDirectory.Text);

                if (gameDirectorySaved && modsDirectorySaved)
                    MessageBox.Show("Settings saved successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else
                    MessageBox.Show("Failed to save some settings. Please verify the JSON file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            catch (Exception ex)
            {
                // Handle any exceptions that might occur when saving the settings
                MessageBox.Show($"An error occurred while saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
