using Bolt.Data;

namespace Bolt.Forms
{
    public partial class FrmNewGame : Form
    {
        public FrmNewGame()
        {
            InitializeComponent();
        }

        private void BtnExecutable_Click(object sender, EventArgs e)
        {
            OfdExecutable.Title = "Select Executable";
            OfdExecutable.FileName = string.Empty;
            OfdExecutable.Filter = "Executable File (*.exe)|*.exe";

            if (OfdExecutable.ShowDialog(this) == DialogResult.OK)
            {
                // Validate if it's really an .exe file
                if (!(Path.GetExtension(OfdExecutable.FileName)?.ToLower() == ".exe"))
                {
                    MessageBox.Show(
                        "Please select a valid game executable file.",
                        "Invalid File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                TxyExecutable.Value = OfdExecutable.FileName;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // TODO: Validade entries
            // TODO: Save button logic...
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmNewGame_Load(object sender, EventArgs e)
        {
            TxyLocation.Value = $"{AppDbContext.ModsDirectoryValue}\\";
        }

        private void TxyName_ValueChanged(object sender, EventArgs e)
        {
            TxyLocation.Value = $"{AppDbContext.ModsDirectoryValue}\\{TxyName.Value}";
        }
    }
}
