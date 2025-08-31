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
            textEntry1 = new Bolt.Controls.TextEntry();
            textEntry2 = new Bolt.Controls.TextEntry();
            textEntry3 = new Bolt.Controls.TextEntry();
            textEntry4 = new Bolt.Controls.TextEntry();
            BtnCancel = new Button();
            BtnSave = new Button();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // textEntry1
            // 
            textEntry1.Location = new Point(12, 24);
            textEntry1.Name = "textEntry1";
            textEntry1.ReadOnly = false;
            textEntry1.Size = new Size(360, 59);
            textEntry1.TabIndex = 0;
            textEntry1.Text = "Game name:";
            // 
            // textEntry2
            // 
            textEntry2.Location = new Point(12, 89);
            textEntry2.Name = "textEntry2";
            textEntry2.ReadOnly = false;
            textEntry2.Size = new Size(360, 59);
            textEntry2.TabIndex = 1;
            textEntry2.Text = "Game executable path:";
            // 
            // textEntry3
            // 
            textEntry3.Location = new Point(12, 154);
            textEntry3.Name = "textEntry3";
            textEntry3.ReadOnly = false;
            textEntry3.Size = new Size(360, 59);
            textEntry3.TabIndex = 2;
            textEntry3.Text = "Where to create game home directory :";
            // 
            // textEntry4
            // 
            textEntry4.Location = new Point(12, 219);
            textEntry4.Name = "textEntry4";
            textEntry4.ReadOnly = true;
            textEntry4.Size = new Size(360, 59);
            textEntry4.TabIndex = 3;
            textEntry4.TabStop = false;
            textEntry4.Text = "Resulting game home directory:";
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(297, 326);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
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
            Controls.Add(textEntry4);
            Controls.Add(textEntry3);
            Controls.Add(textEntry2);
            Controls.Add(textEntry1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmNewGame";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Game";
            ResumeLayout(false);
        }

        #endregion

        private Controls.TextEntry textEntry1;
        private Controls.TextEntry textEntry2;
        private Controls.TextEntry textEntry3;
        private Controls.TextEntry textEntry4;
        private Button BtnCancel;
        private Button BtnSave;
        private Panel panel1;
    }
}