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
            // Validade entry
            if (TxyModsDir.Value == string.Empty)
            {
                MessageBox.Show(
                    "The mods directory cannot be empty.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // Save mods directory path
            Json.Write(AppDbContext.MODS_DIRECTORY, TxyModsDir.Value);

            MessageBox.Show(
                $"The directory \"{TxyModsDir.Value}\" has been successfully saved as the mods folder.",
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
