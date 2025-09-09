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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmNewGame));
            BtnClose = new Button();
            BtnSave = new Button();
            panel1 = new Panel();
            OfdExecutable = new OpenFileDialog();
            BtnExecutable = new Button();
            TxyName = new Bolt.Controls.TextEntry();
            TxyExecutable = new Bolt.Controls.TextEntry();
            TxyResultLocation = new Bolt.Controls.TextEntry();
            BtnTarget = new Button();
            TxyTarget = new Bolt.Controls.TextEntry();
            FbdTarget = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // BtnClose
            // 
            BtnClose.Location = new Point(297, 258);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(75, 23);
            BtnClose.TabIndex = 6;
            BtnClose.Text = "Close";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // BtnSave
            // 
            BtnSave.Location = new Point(216, 258);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(75, 23);
            BtnSave.TabIndex = 5;
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
            BtnExecutable.Location = new Point(343, 147);
            BtnExecutable.Name = "BtnExecutable";
            BtnExecutable.Size = new Size(24, 24);
            BtnExecutable.TabIndex = 4;
            BtnExecutable.Text = "...";
            BtnExecutable.UseVisualStyleBackColor = true;
            BtnExecutable.Click += BtnExecutable_Click;
            // 
            // TxyName
            // 
            TxyName.Location = new Point(17, 71);
            TxyName.Margin = new Padding(8, 6, 8, 6);
            TxyName.Name = "TxyName";
            TxyName.ReadOnly = false;
            TxyName.Size = new Size(350, 44);
            TxyName.TabIndex = 2;
            TxyName.Text = "Game name:";
            TxyName.Value = "";
            TxyName.ValueChanged += TxyName_ValueChanged;
            // 
            // TxyExecutable
            // 
            TxyExecutable.Location = new Point(17, 127);
            TxyExecutable.Margin = new Padding(8, 6, 4, 6);
            TxyExecutable.Name = "TxyExecutable";
            TxyExecutable.ReadOnly = false;
            TxyExecutable.Size = new Size(319, 44);
            TxyExecutable.TabIndex = 3;
            TxyExecutable.Text = "Executable file:";
            TxyExecutable.Value = "";
            // 
            // TxyResultLocation
            // 
            TxyResultLocation.Location = new Point(17, 183);
            TxyResultLocation.Margin = new Padding(8, 6, 8, 6);
            TxyResultLocation.Name = "TxyResultLocation";
            TxyResultLocation.ReadOnly = true;
            TxyResultLocation.Size = new Size(350, 44);
            TxyResultLocation.TabIndex = 0;
            TxyResultLocation.TabStop = false;
            TxyResultLocation.Text = "Result location:";
            TxyResultLocation.Value = "";
            // 
            // BtnTarget
            // 
            BtnTarget.Location = new Point(343, 35);
            BtnTarget.Name = "BtnTarget";
            BtnTarget.Size = new Size(24, 24);
            BtnTarget.TabIndex = 1;
            BtnTarget.Text = "...";
            BtnTarget.UseVisualStyleBackColor = true;
            BtnTarget.Click += BtnTarget_Click;
            // 
            // TxyTarget
            // 
            TxyTarget.Location = new Point(17, 15);
            TxyTarget.Margin = new Padding(8, 6, 4, 6);
            TxyTarget.Name = "TxyTarget";
            TxyTarget.ReadOnly = false;
            TxyTarget.Size = new Size(319, 44);
            TxyTarget.TabIndex = 0;
            TxyTarget.Text = "Target directory:";
            TxyTarget.Value = "";
            TxyTarget.ValueChanged += TxyTarget_ValueChanged;
            // 
            // FrmNewGame
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(384, 293);
            Controls.Add(TxyTarget);
            Controls.Add(TxyExecutable);
            Controls.Add(TxyResultLocation);
            Controls.Add(BtnTarget);
            Controls.Add(TxyName);
            Controls.Add(BtnExecutable);
            Controls.Add(panel1);
            Controls.Add(BtnSave);
            Controls.Add(BtnClose);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmNewGame";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Game";
            Load += FrmNewGame_Load;
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
        private Controls.TextEntry TxyResultLocation;
        private Button BtnTarget;
        private Controls.TextEntry TxyTarget;
        private FolderBrowserDialog FbdTarget;
    }
}