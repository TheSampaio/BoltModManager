using Bolt.Utilities;

namespace Bolt.Forms
{
    public partial class FrmPreferences : Form
    {
        private const string MODS_DIR = "ModsDirectory";

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
            Json.Write(MODS_DIR, TxyModsDir.Value);

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
            TxyModsDir.Value = Json.Read(MODS_DIR)!;
        }
    }
}
