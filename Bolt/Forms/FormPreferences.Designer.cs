namespace Bolt.Forms
{
    partial class FrmPreferences
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            BtnSave = new Button();
            BtnCancel = new Button();
            TxyLibrary = new Bolt.Controls.TextEntry();
            BtnLibrary = new Button();
            FbdLibrary = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.ForeColor = SystemColors.ActiveBorder;
            panel1.Location = new Point(12, 246);
            panel1.Name = "panel1";
            panel1.Size = new Size(360, 1);
            panel1.TabIndex = 6;
            // 
            // BtnSave
            // 
            BtnSave.Location = new Point(216, 258);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(75, 23);
            BtnSave.TabIndex = 7;
            BtnSave.Text = "Save";
            BtnSave.UseVisualStyleBackColor = true;
            BtnSave.Click += BtnSave_Click;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(297, 258);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 8;
            BtnCancel.Text = "Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // TxyLibrary
            // 
            TxyLibrary.Location = new Point(17, 28);
            TxyLibrary.Margin = new Padding(8, 8, 4, 8);
            TxyLibrary.Name = "TxyLibrary";
            TxyLibrary.ReadOnly = false;
            TxyLibrary.Size = new Size(319, 44);
            TxyLibrary.TabIndex = 9;
            TxyLibrary.Text = "Games directory:";
            // 
            // BtnLibrary
            // 
            BtnLibrary.Location = new Point(343, 48);
            BtnLibrary.Name = "BtnLibrary";
            BtnLibrary.Size = new Size(24, 24);
            BtnLibrary.TabIndex = 10;
            BtnLibrary.Text = "...";
            BtnLibrary.UseVisualStyleBackColor = true;
            BtnLibrary.Click += BtnLibrary_Click;
            // 
            // FrmPreferences
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(384, 293);
            Controls.Add(TxyLibrary);
            Controls.Add(BtnLibrary);
            Controls.Add(panel1);
            Controls.Add(BtnSave);
            Controls.Add(BtnCancel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmPreferences";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Preferences";
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button BtnSave;
        private Button BtnCancel;
        private Controls.TextEntry TxyLibrary;
        private Button BtnLibrary;
        private FolderBrowserDialog FbdLibrary;
    }
}