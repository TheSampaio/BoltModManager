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
            TxeGameName = new Bolt.Controls.TextEntry();
            TxeExecutablePath = new Bolt.Controls.TextEntry();
            TxeGameDirectory = new Bolt.Controls.TextEntry();
            TxeResultPath = new Bolt.Controls.TextEntry();
            BtnCancel = new Button();
            BtnSave = new Button();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // TxeGameName
            // 
            TxeGameName.Location = new Point(12, 24);
            TxeGameName.Name = "TxeGameName";
            TxeGameName.ReadOnly = false;
            TxeGameName.Size = new Size(360, 59);
            TxeGameName.TabIndex = 0;
            TxeGameName.Text = "Game name:";
            // 
            // TxeExecutablePath
            // 
            TxeExecutablePath.Location = new Point(12, 89);
            TxeExecutablePath.Name = "TxeExecutablePath";
            TxeExecutablePath.ReadOnly = false;
            TxeExecutablePath.Size = new Size(360, 59);
            TxeExecutablePath.TabIndex = 1;
            TxeExecutablePath.Text = "Game executable path:";
            // 
            // TxeGameDirectory
            // 
            TxeGameDirectory.Location = new Point(12, 154);
            TxeGameDirectory.Name = "TxeGameDirectory";
            TxeGameDirectory.ReadOnly = false;
            TxeGameDirectory.Size = new Size(360, 59);
            TxeGameDirectory.TabIndex = 2;
            TxeGameDirectory.Text = "Where to create game home directory :";
            // 
            // TxeResultPath
            // 
            TxeResultPath.Location = new Point(12, 219);
            TxeResultPath.Name = "TxeResultPath";
            TxeResultPath.ReadOnly = true;
            TxeResultPath.Size = new Size(360, 59);
            TxeResultPath.TabIndex = 3;
            TxeResultPath.TabStop = false;
            TxeResultPath.Text = "Resulting game home directory:";
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(297, 326);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // BtnSave
            // 
            BtnSave.Location = new Point(216, 326);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(75, 23);
            BtnSave.TabIndex = 4;
            BtnSave.Text = "Save";
            BtnSave.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.ForeColor = SystemColors.ActiveBorder;
            panel1.Location = new Point(12, 315);
            panel1.Name = "panel1";
            panel1.Size = new Size(360, 1);
            panel1.TabIndex = 0;
            // 
            // FrmNewGame
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(384, 361);
            Controls.Add(panel1);
            Controls.Add(BtnSave);
            Controls.Add(BtnCancel);
            Controls.Add(TxeResultPath);
            Controls.Add(TxeGameDirectory);
            Controls.Add(TxeExecutablePath);
            Controls.Add(TxeGameName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmNewGame";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Game";
            ResumeLayout(false);
        }

        #endregion

        private Controls.TextEntry TxeGameName;
        private Controls.TextEntry TxeExecutablePath;
        private Controls.TextEntry TxeGameDirectory;
        private Controls.TextEntry TxeResultPath;
        private Button BtnCancel;
        private Button BtnSave;
        private Panel panel1;
    }
}