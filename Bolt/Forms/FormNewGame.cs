using Bolt.Data;
using Bolt.Utilities;

namespace Bolt.Forms
{
    public partial class FrmNewGame : Form
    {
        public FrmNewGame()
        {
            InitializeComponent();
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
            TxyLocation.Value = $"{Json.Read(AppDbContext.MODS_DIRECTORY)!}\\";
        }

        private void TxyName_ValueChanged(object sender, EventArgs e)
        {
            TxyLocation.Value = $"{Json.Read(AppDbContext.MODS_DIRECTORY)!}\\{TxyName.Value}";
        }
    }
}
