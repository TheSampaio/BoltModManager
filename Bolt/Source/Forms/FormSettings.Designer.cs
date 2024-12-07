namespace Bolt.Source
{
    partial class FormSettings
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
            label1 = new Label();
            Txt_GameDirectory = new TextBox();
            label2 = new Label();
            Txt_ModsDirectory = new TextBox();
            Btn_SaveSettings = new Button();
            Btn_ModsPath = new Button();
            Btn_GamePath = new Button();
            Fbd_Settings = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.ForeColor = Color.White;
            label1.Location = new Point(23, 18);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(117, 21);
            label1.TabIndex = 0;
            label1.Text = "Game directory";
            // 
            // Txt_GameDirectory
            // 
            Txt_GameDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Txt_GameDirectory.BackColor = Color.FromArgb(50, 50, 50);
            Txt_GameDirectory.BorderStyle = BorderStyle.FixedSingle;
            Txt_GameDirectory.Font = new Font("Segoe UI", 10F);
            Txt_GameDirectory.ForeColor = Color.White;
            Txt_GameDirectory.Location = new Point(23, 47);
            Txt_GameDirectory.Margin = new Padding(8);
            Txt_GameDirectory.Name = "Txt_GameDirectory";
            Txt_GameDirectory.Size = new Size(708, 25);
            Txt_GameDirectory.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.ForeColor = Color.White;
            label2.Location = new Point(23, 89);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(117, 21);
            label2.TabIndex = 0;
            label2.Text = "Mods Directory";
            // 
            // Txt_ModsDirectory
            // 
            Txt_ModsDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Txt_ModsDirectory.BackColor = Color.FromArgb(50, 50, 50);
            Txt_ModsDirectory.BorderStyle = BorderStyle.FixedSingle;
            Txt_ModsDirectory.Font = new Font("Segoe UI", 10F);
            Txt_ModsDirectory.ForeColor = Color.White;
            Txt_ModsDirectory.Location = new Point(23, 118);
            Txt_ModsDirectory.Margin = new Padding(8);
            Txt_ModsDirectory.Name = "Txt_ModsDirectory";
            Txt_ModsDirectory.Size = new Size(708, 25);
            Txt_ModsDirectory.TabIndex = 1;
            // 
            // Btn_SaveSettings
            // 
            Btn_SaveSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_SaveSettings.BackColor = Color.FromArgb(50, 50, 50);
            Btn_SaveSettings.Cursor = Cursors.Hand;
            Btn_SaveSettings.FlatAppearance.BorderSize = 0;
            Btn_SaveSettings.FlatStyle = FlatStyle.Flat;
            Btn_SaveSettings.Font = new Font("Segoe UI", 12F);
            Btn_SaveSettings.ForeColor = Color.White;
            Btn_SaveSettings.Location = new Point(666, 173);
            Btn_SaveSettings.Name = "Btn_SaveSettings";
            Btn_SaveSettings.Size = new Size(97, 30);
            Btn_SaveSettings.TabIndex = 2;
            Btn_SaveSettings.Text = "Save";
            Btn_SaveSettings.UseVisualStyleBackColor = false;
            Btn_SaveSettings.Click += Btn_SaveSettings_Click;
            // 
            // Btn_ModsPath
            // 
            Btn_ModsPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_ModsPath.BackColor = Color.FromArgb(50, 50, 50);
            Btn_ModsPath.Cursor = Cursors.Hand;
            Btn_ModsPath.FlatAppearance.BorderSize = 0;
            Btn_ModsPath.FlatStyle = FlatStyle.Flat;
            Btn_ModsPath.Font = new Font("Segoe UI", 12F);
            Btn_ModsPath.ForeColor = Color.White;
            Btn_ModsPath.Location = new Point(738, 118);
            Btn_ModsPath.Name = "Btn_ModsPath";
            Btn_ModsPath.Size = new Size(25, 25);
            Btn_ModsPath.TabIndex = 2;
            Btn_ModsPath.Text = "F";
            Btn_ModsPath.UseVisualStyleBackColor = false;
            Btn_ModsPath.Click += Btn_ModsPath_Click;
            // 
            // Btn_GamePath
            // 
            Btn_GamePath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_GamePath.BackColor = Color.FromArgb(50, 50, 50);
            Btn_GamePath.Cursor = Cursors.Hand;
            Btn_GamePath.FlatAppearance.BorderSize = 0;
            Btn_GamePath.FlatStyle = FlatStyle.Flat;
            Btn_GamePath.Font = new Font("Segoe UI", 12F);
            Btn_GamePath.ForeColor = Color.White;
            Btn_GamePath.Location = new Point(738, 47);
            Btn_GamePath.Name = "Btn_GamePath";
            Btn_GamePath.Size = new Size(25, 25);
            Btn_GamePath.TabIndex = 2;
            Btn_GamePath.Text = "F";
            Btn_GamePath.UseVisualStyleBackColor = false;
            Btn_GamePath.Click += Btn_GamePath_Click;
            // 
            // FormSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(784, 561);
            Controls.Add(Btn_GamePath);
            Controls.Add(Btn_ModsPath);
            Controls.Add(Btn_SaveSettings);
            Controls.Add(Txt_ModsDirectory);
            Controls.Add(label2);
            Controls.Add(Txt_GameDirectory);
            Controls.Add(label1);
            Name = "FormSettings";
            Text = "FormSettings";
            Shown += FormSettings_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox Txt_GameDirectory;
        private Label label2;
        private TextBox Txt_ModsDirectory;
        private Button Btn_SaveSettings;
        private Button Btn_ModsPath;
        private Button Btn_GamePath;
        private FolderBrowserDialog Fbd_Settings;
    }
}