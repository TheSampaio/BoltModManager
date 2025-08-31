using System;
using System.Collections.Generic;
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
