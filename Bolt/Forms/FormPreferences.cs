namespace Bolt.Forms
{
    public partial class FrmPreferences : Form
    {
        public FrmPreferences()
        {
            InitializeComponent();
        }

        private void BtnLibrary_Click(object sender, EventArgs e)
        {
            FbdLibrary.ShowDialog(this);

            if (FbdLibrary.SelectedPath != string.Empty)
                TxyLibrary.Value = FbdLibrary.SelectedPath;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // TODO: Validade entries
            // TODO: Save button logic...
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
