using Bolt.Data;
using Bolt.Utilities;

namespace Bolt.Forms
{
    public partial class FrmPreferences : Form
    {
        public FrmPreferences()
        {
            InitializeComponent();
        }

        private void BtnModsDir_Click(object sender, EventArgs e)
        {
            FbdLibrary.ShowDialog(this);

            if (FbdLibrary.SelectedPath != string.Empty)
                TxyModsDir.Value = FbdLibrary.SelectedPath;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate entry
            if (string.IsNullOrWhiteSpace(TxyModsDir.Value))
            {
                MessageBox.Show(
                    "Please select a valid mods directory before continuing.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Ensure directory exists
            if (!Directory.Exists(TxyModsDir.Value))
            {
                MessageBox.Show(
                    "The selected directory does not exist. Please choose a valid folder.",
                    "Invalid Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Save mods directory path
            Json.Write(AppDbContext.MODS_DIRECTORY, TxyModsDir.Value);

            MessageBox.Show(
                $"Mods directory set to:\n{TxyModsDir.Value}",
                "Mods Directory Saved",
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
            TxyModsDir.Value = Json.Read(AppDbContext.MODS_DIRECTORY)!;
        }
    }
}
