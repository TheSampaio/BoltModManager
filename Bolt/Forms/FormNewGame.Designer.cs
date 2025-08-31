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
            BtnCancel = new Button();
            BtnSave = new Button();
            panel1 = new Panel();
            openFileDialog1 = new OpenFileDialog();
            button1 = new Button();
            textEntry1 = new Bolt.Controls.TextEntry();
            textEntry2 = new Bolt.Controls.TextEntry();
            textEntry3 = new Bolt.Controls.TextEntry();
            SuspendLayout();
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(297, 258);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
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
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // button1
            // 
            button1.Location = new Point(343, 108);
            button1.Name = "button1";
            button1.Size = new Size(24, 24);
            button1.TabIndex = 2;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            // 
            // textEntry1
            // 
            textEntry1.Location = new Point(17, 28);
            textEntry1.Margin = new Padding(8);
            textEntry1.Name = "textEntry1";
            textEntry1.ReadOnly = false;
            textEntry1.Size = new Size(350, 44);
            textEntry1.TabIndex = 0;
            textEntry1.Text = "Name:";
            // 
            // textEntry2
            // 
            textEntry2.Location = new Point(17, 88);
            textEntry2.Margin = new Padding(8, 8, 4, 8);
            textEntry2.Name = "textEntry2";
            textEntry2.ReadOnly = false;
            textEntry2.Size = new Size(319, 44);
            textEntry2.TabIndex = 1;
            textEntry2.Text = "Executable:";
            // 
            // textEntry3
            // 
            textEntry3.Location = new Point(17, 148);
            textEntry3.Margin = new Padding(8);
            textEntry3.Name = "textEntry3";
            textEntry3.ReadOnly = true;
            textEntry3.Size = new Size(350, 44);
            textEntry3.TabIndex = 3;
            textEntry3.TabStop = false;
            textEntry3.Text = "Location:";
            // 
            // FrmNewGame
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(384, 293);
            Controls.Add(textEntry2);
            Controls.Add(textEntry3);
            Controls.Add(textEntry1);
            Controls.Add(button1);
            Controls.Add(panel1);
            Controls.Add(BtnSave);
            Controls.Add(BtnCancel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmNewGame";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Game";
            ResumeLayout(false);
        }

        #endregion
        private Button BtnCancel;
        private Button BtnSave;
        private Panel panel1;
        private OpenFileDialog openFileDialog1;
        private Button button1;
        private Controls.TextEntry textEntry1;
        private Controls.TextEntry textEntry2;
        private Controls.TextEntry textEntry3;
    }
}