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
            BtnClose = new Button();
            TxyPackages = new Bolt.Controls.TextEntry();
            BtnLibrary = new Button();
            FbdPackages = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.ForeColor = SystemColors.ActiveBorder;
            panel1.Location = new Point(12, 246);
            panel1.Name = "panel1";
            panel1.Size = new Size(360, 1);
            panel1.TabIndex = 0;
            // 
            // BtnSave
            // 
            BtnSave.Location = new Point(216, 258);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(75, 23);
            BtnSave.TabIndex = 2;
            BtnSave.Text = "Save";
            BtnSave.UseVisualStyleBackColor = true;
            BtnSave.Click += BtnSave_Click;
            // 
            // BtnClose
            // 
            BtnClose.Location = new Point(297, 258);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(75, 23);
            BtnClose.TabIndex = 3;
            BtnClose.Text = "Close";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // TxyModsDir
            // 
            TxyPackages.Location = new Point(17, 28);
            TxyPackages.Margin = new Padding(8, 8, 4, 8);
            TxyPackages.Name = "TxyModsDir";
            TxyPackages.ReadOnly = false;
            TxyPackages.Size = new Size(319, 44);
            TxyPackages.TabIndex = 0;
            TxyPackages.Text = "Mods directory:";
            TxyPackages.Value = "";
            // 
            // BtnLibrary
            // 
            BtnLibrary.Location = new Point(343, 48);
            BtnLibrary.Name = "BtnLibrary";
            BtnLibrary.Size = new Size(24, 24);
            BtnLibrary.TabIndex = 1;
            BtnLibrary.Text = "...";
            BtnLibrary.UseVisualStyleBackColor = true;
            BtnLibrary.Click += BtnPackages_Click;
            // 
            // FrmPreferences
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(384, 293);
            Controls.Add(TxyPackages);
            Controls.Add(BtnLibrary);
            Controls.Add(panel1);
            Controls.Add(BtnSave);
            Controls.Add(BtnClose);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmPreferences";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Preferences";
            Load += FrmPreferences_Load;
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button BtnSave;
        private Button BtnClose;
        private Controls.TextEntry TxyPackages;
        private Button BtnLibrary;
        private FolderBrowserDialog FbdPackages;
    }
}