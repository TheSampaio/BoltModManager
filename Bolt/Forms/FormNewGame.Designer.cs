namespace Bolt.Forms
{
    partial class FrmNewGame
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
            BtnClose = new Button();
            BtnSave = new Button();
            panel1 = new Panel();
            OfdExecutable = new OpenFileDialog();
            BtnExecutable = new Button();
            TxyName = new Bolt.Controls.TextEntry();
            TxyExecutable = new Bolt.Controls.TextEntry();
            TxyLocation = new Bolt.Controls.TextEntry();
            SuspendLayout();
            // 
            // BtnClose
            // 
            BtnClose.Location = new Point(297, 258);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(75, 23);
            BtnClose.TabIndex = 5;
            BtnClose.Text = "Close";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // BtnSave
            // 
            BtnSave.Location = new Point(216, 258);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(75, 23);
            BtnSave.TabIndex = 4;
            BtnSave.Text = "Save";
            BtnSave.UseVisualStyleBackColor = true;
            BtnSave.Click += BtnSave_Click;
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
            // OfdExecutable
            // 
            OfdExecutable.FileName = "openFileDialog1";
            // 
            // BtnExecutable
            // 
            BtnExecutable.Location = new Point(343, 108);
            BtnExecutable.Name = "BtnExecutable";
            BtnExecutable.Size = new Size(24, 24);
            BtnExecutable.TabIndex = 2;
            BtnExecutable.Text = "...";
            BtnExecutable.UseVisualStyleBackColor = true;
            // 
            // TxyName
            // 
            TxyName.Location = new Point(17, 28);
            TxyName.Margin = new Padding(8);
            TxyName.Name = "TxyName";
            TxyName.ReadOnly = false;
            TxyName.Size = new Size(350, 44);
            TxyName.TabIndex = 0;
            TxyName.Text = "Name:";
            TxyName.Value = "";
            // 
            // TxyExecutable
            // 
            TxyExecutable.Location = new Point(17, 88);
            TxyExecutable.Margin = new Padding(8, 8, 4, 8);
            TxyExecutable.Name = "TxyExecutable";
            TxyExecutable.ReadOnly = false;
            TxyExecutable.Size = new Size(319, 44);
            TxyExecutable.TabIndex = 1;
            TxyExecutable.Text = "Executable:";
            TxyExecutable.Value = "";
            // 
            // TxyLocation
            // 
            TxyLocation.Location = new Point(17, 148);
            TxyLocation.Margin = new Padding(8);
            TxyLocation.Name = "TxyLocation";
            TxyLocation.ReadOnly = true;
            TxyLocation.Size = new Size(350, 44);
            TxyLocation.TabIndex = 3;
            TxyLocation.TabStop = false;
            TxyLocation.Text = "Location:";
            TxyLocation.Value = "";
            // 
            // FrmNewGame
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(384, 293);
            Controls.Add(TxyExecutable);
            Controls.Add(TxyLocation);
            Controls.Add(TxyName);
            Controls.Add(BtnExecutable);
            Controls.Add(panel1);
            Controls.Add(BtnSave);
            Controls.Add(BtnClose);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmNewGame";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Game";
            ResumeLayout(false);
        }

        #endregion
        private Button BtnClose;
        private Button BtnSave;
        private Panel panel1;
        private OpenFileDialog OfdExecutable;
        private Button BtnExecutable;
        private Controls.TextEntry TxyName;
        private Controls.TextEntry TxyExecutable;
        private Controls.TextEntry TxyLocation;
    }
}