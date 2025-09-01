using Bolt.Data;

namespace Bolt.Forms
{
    public partial class FrmPreferences : Form
    {
        public FrmPreferences()
        {
            InitializeComponent();
        }

        private void BtnPackagesDir_Click(object sender, EventArgs e)
        {
            if (FbdLibrary.ShowDialog(this) == DialogResult.OK)
                TxyPackagesDir.Value = FbdLibrary.SelectedPath;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate entry
            if (string.IsNullOrWhiteSpace(TxyPackagesDir.Value))
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
            if (!Directory.Exists(TxyPackagesDir.Value))
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
            AppDbContext.PackagesDirectoryValue = TxyPackagesDir.Value;

            MessageBox.Show(
                $"Packages directory set to:\n{TxyPackagesDir.Value}",
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
            TxyPackagesDir.Value = AppDbContext.PackagesDirectoryValue;
        }
    }
}
