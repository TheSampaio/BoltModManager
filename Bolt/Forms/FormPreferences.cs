using Bolt.Data;

namespace Bolt.Forms
{
    public partial class FrmPreferences : Form
    {
        public FrmPreferences()
        {
            InitializeComponent();
        }

        private void BtnPackages_Click(object sender, EventArgs e)
        {
            if (FbdPackages.ShowDialog(this) == DialogResult.OK)
                TxyPackages.Value = FbdPackages.SelectedPath;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate entry
            if (string.IsNullOrWhiteSpace(TxyPackages.Value))
            {
                MessageBox.Show(
                    "Please select a valid packages directory before continuing.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Ensure directory exists
            if (!Directory.Exists(TxyPackages.Value))
            {
                MessageBox.Show(
                    "The selected directory does not exist. Please choose a valid folder.",
                    "Invalid Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Save packages directory path
            PackageData.Save(TxyPackages.Value);

            MessageBox.Show(
                $"Packages directory set to:\n{TxyPackages.Value}",
                "Packages Directory Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }


        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmPreferences_Load(object sender, EventArgs e)
        {
            var preferencesData = PackageData.Load();

            if (preferencesData is not null)
                TxyPackages.Value = preferencesData;
        }
    }
}
