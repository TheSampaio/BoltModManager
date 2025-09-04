namespace Bolt.Forms
{
    partial class FrmHome
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
            MnsHome = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newGameToolStripMenuItem = new ToolStripMenuItem();
            openGameToolStripMenuItem = new ToolStripMenuItem();
            quitGameToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            recentToolStripMenuItem = new ToolStripMenuItem();
            clearHistoryToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            quitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            donateToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            OfdOpenGame = new OpenFileDialog();
            panel1 = new Panel();
            TxtGameTitle = new TextBox();
            MnsHome.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // MnsHome
            // 
            MnsHome.BackColor = SystemColors.ControlLightLight;
            MnsHome.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, helpToolStripMenuItem });
            MnsHome.Location = new Point(0, 0);
            MnsHome.Name = "MnsHome";
            MnsHome.Size = new Size(784, 24);
            MnsHome.TabIndex = 0;
            MnsHome.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newGameToolStripMenuItem, openGameToolStripMenuItem, quitGameToolStripMenuItem, toolStripSeparator1, recentToolStripMenuItem, toolStripSeparator2, quitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // newGameToolStripMenuItem
            // 
            newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            newGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newGameToolStripMenuItem.Size = new Size(182, 22);
            newGameToolStripMenuItem.Text = "New Game";
            newGameToolStripMenuItem.Click += NewGame_ToolStripMenuItem_Click;
            // 
            // openGameToolStripMenuItem
            // 
            openGameToolStripMenuItem.Name = "openGameToolStripMenuItem";
            openGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openGameToolStripMenuItem.Size = new Size(182, 22);
            openGameToolStripMenuItem.Text = "Open Game";
            openGameToolStripMenuItem.Click += OpenGame_ToolStripMenuItem_Click;
            // 
            // quitGameToolStripMenuItem
            // 
            quitGameToolStripMenuItem.Name = "quitGameToolStripMenuItem";
            quitGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.W;
            quitGameToolStripMenuItem.Size = new Size(182, 22);
            quitGameToolStripMenuItem.Text = "Close Game";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(179, 6);
            // 
            // recentToolStripMenuItem
            // 
            recentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { clearHistoryToolStripMenuItem });
            recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            recentToolStripMenuItem.Size = new Size(182, 22);
            recentToolStripMenuItem.Text = "Recent";
            // 
            // clearHistoryToolStripMenuItem
            // 
            clearHistoryToolStripMenuItem.Name = "clearHistoryToolStripMenuItem";
            clearHistoryToolStripMenuItem.Size = new Size(140, 22);
            clearHistoryToolStripMenuItem.Text = "Clear history";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(179, 6);
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Q;
            quitToolStripMenuItem.Size = new Size(182, 22);
            quitToolStripMenuItem.Text = "Quit";
            quitToolStripMenuItem.Click += Quit_ToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(50, 20);
            editToolStripMenuItem.Text = "Editor";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;
            settingsToolStripMenuItem.Size = new Size(185, 22);
            settingsToolStripMenuItem.Text = "Preferences...";
            settingsToolStripMenuItem.Click += Settings_ToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { donateToolStripMenuItem, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // donateToolStripMenuItem
            // 
            donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            donateToolStripMenuItem.Size = new Size(116, 22);
            donateToolStripMenuItem.Text = "Donate";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(116, 22);
            aboutToolStripMenuItem.Text = "About...";
            // 
            // OfdOpenGame
            // 
            OfdOpenGame.FileName = "OfdOpenGame";
            // 
            // panel1
            // 
            panel1.Controls.Add(TxtGameTitle);
            panel1.Location = new Point(12, 36);
            panel1.Margin = new Padding(3, 12, 3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(760, 513);
            panel1.TabIndex = 1;
            // 
            // TxtGameTitle
            // 
            TxtGameTitle.BackColor = SystemColors.ControlLightLight;
            TxtGameTitle.Dock = DockStyle.Top;
            TxtGameTitle.Font = new Font("Segoe UI", 12F);
            TxtGameTitle.Location = new Point(0, 0);
            TxtGameTitle.Margin = new Padding(0);
            TxtGameTitle.Name = "TxtGameTitle";
            TxtGameTitle.PlaceholderText = "No Game Open";
            TxtGameTitle.ReadOnly = true;
            TxtGameTitle.Size = new Size(760, 29);
            TxtGameTitle.TabIndex = 0;
            TxtGameTitle.TabStop = false;
            TxtGameTitle.TextAlign = HorizontalAlignment.Center;
            // 
            // FrmHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(784, 561);
            Controls.Add(panel1);
            Controls.Add(MnsHome);
            MainMenuStrip = MnsHome;
            MinimumSize = new Size(800, 600);
            Name = "FrmHome";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bolt Mod Manager";
            MnsHome.ResumeLayout(false);
            MnsHome.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip MnsHome;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newGameToolStripMenuItem;
        private ToolStripMenuItem openGameToolStripMenuItem;
        private ToolStripMenuItem recentToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem quitGameToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem quitToolStripMenuItem;
        private ToolStripMenuItem clearHistoryToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem donateToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private OpenFileDialog OfdOpenGame;
        private Panel panel1;
        private TextBox TxtGameTitle;
    }
}