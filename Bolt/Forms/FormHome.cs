namespace Bolt.Forms
{
    public partial class FrmHome : Form
    {
        public FrmHome()
        {
            InitializeComponent();
        }

        private void NewGame_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowModalWindow(new FrmNewGame());
        }

        private void Quit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Settings_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowModalWindow(new FrmPreferences());
        }

        private static void ShowModalWindow(Form form)
        {
            var frmModal = form;
            frmModal.ShowDialog();
        }
    }
}
