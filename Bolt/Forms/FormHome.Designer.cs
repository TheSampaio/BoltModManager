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
            ListViewItem listViewItem1 = new ListViewItem("");
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
            splitContainer1 = new SplitContainer();
            splitContainer3 = new SplitContainer();
            splitContainer4 = new SplitContainer();
            BtnRun = new Button();
            CmbProfiles = new ComboBox();
            listView2 = new ListView();
            splitContainer2 = new SplitContainer();
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewCheckBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewComboBoxColumn();
            listView1 = new ListView();
            PnlHomeSurface = new Panel();
            panel2 = new Panel();
            MnsHome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer4).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            PnlHomeSurface.SuspendLayout();
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
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer3);
            splitContainer1.Panel1MinSize = 300;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(762, 480);
            splitContainer1.SplitterDistance = 300;
            splitContainer1.SplitterWidth = 8;
            splitContainer1.TabIndex = 0;
            splitContainer1.TabStop = false;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.FixedPanel = FixedPanel.Panel1;
            splitContainer3.IsSplitterFixed = true;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(listView2);
            splitContainer3.Size = new Size(300, 480);
            splitContainer3.SplitterDistance = 111;
            splitContainer3.SplitterWidth = 8;
            splitContainer3.TabIndex = 0;
            splitContainer3.TabStop = false;
            // 
            // splitContainer4
            // 
            splitContainer4.Dock = DockStyle.Fill;
            splitContainer4.IsSplitterFixed = true;
            splitContainer4.Location = new Point(0, 0);
            splitContainer4.Margin = new Padding(0);
            splitContainer4.Name = "splitContainer4";
            splitContainer4.Orientation = Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.Controls.Add(BtnRun);
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.Controls.Add(CmbProfiles);
            splitContainer4.Size = new Size(300, 111);
            splitContainer4.SplitterDistance = 65;
            splitContainer4.SplitterWidth = 8;
            splitContainer4.TabIndex = 0;
            splitContainer4.TabStop = false;
            // 
            // BtnRun
            // 
            BtnRun.BackColor = SystemColors.Window;
            BtnRun.Cursor = Cursors.Hand;
            BtnRun.Dock = DockStyle.Fill;
            BtnRun.Enabled = false;
            BtnRun.FlatAppearance.BorderSize = 0;
            BtnRun.Font = new Font("Segoe UI", 12F);
            BtnRun.ImageAlign = ContentAlignment.MiddleLeft;
            BtnRun.Location = new Point(0, 0);
            BtnRun.Margin = new Padding(0);
            BtnRun.Name = "BtnRun";
            BtnRun.Padding = new Padding(16);
            BtnRun.Size = new Size(300, 65);
            BtnRun.TabIndex = 0;
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnRun.UseVisualStyleBackColor = false;
            BtnRun.Click += BtnRun_Click;
            // 
            // CmbProfiles
            // 
            CmbProfiles.Dock = DockStyle.Fill;
            CmbProfiles.DropDownStyle = ComboBoxStyle.DropDownList;
            CmbProfiles.DropDownWidth = 256;
            CmbProfiles.Enabled = false;
            CmbProfiles.Font = new Font("Segoe UI", 16F);
            CmbProfiles.FormattingEnabled = true;
            CmbProfiles.IntegralHeight = false;
            CmbProfiles.Location = new Point(0, 0);
            CmbProfiles.Margin = new Padding(0);
            CmbProfiles.Name = "CmbProfiles";
            CmbProfiles.Size = new Size(300, 38);
            CmbProfiles.TabIndex = 0;
            // 
            // listView2
            // 
            listView2.BorderStyle = BorderStyle.None;
            listView2.Dock = DockStyle.Fill;
            listView2.Items.AddRange(new ListViewItem[] { listViewItem1 });
            listView2.Location = new Point(0, 0);
            listView2.Name = "listView2";
            listView2.Size = new Size(300, 361);
            listView2.TabIndex = 0;
            listView2.UseCompatibleStateImageBehavior = false;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(dataGridView1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(listView1);
            splitContainer2.Size = new Size(454, 480);
            splitContainer2.SplitterDistance = 335;
            splitContainer2.SplitterWidth = 8;
            splitContainer2.TabIndex = 0;
            splitContainer2.TabStop = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = SystemColors.Window;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6 });
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.GridColor = SystemColors.ControlDark;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(454, 335);
            dataGridView1.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.HeaderText = "Status";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            // 
            // Column2
            // 
            Column2.HeaderText = "Name";
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            // 
            // Column3
            // 
            Column3.HeaderText = "Version";
            Column3.Name = "Column3";
            Column3.ReadOnly = true;
            // 
            // Column4
            // 
            Column4.HeaderText = "Category";
            Column4.Name = "Column4";
            Column4.ReadOnly = true;
            // 
            // Column5
            // 
            Column5.HeaderText = "Installation";
            Column5.Name = "Column5";
            Column5.ReadOnly = true;
            // 
            // Column6
            // 
            Column6.HeaderText = "Actions";
            Column6.Name = "Column6";
            Column6.ReadOnly = true;
            // 
            // listView1
            // 
            listView1.BackColor = SystemColors.Window;
            listView1.BorderStyle = BorderStyle.None;
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(0, 0);
            listView1.Name = "listView1";
            listView1.Size = new Size(454, 137);
            listView1.TabIndex = 2;
            listView1.TabStop = false;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // PnlHomeSurface
            // 
            PnlHomeSurface.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PnlHomeSurface.Controls.Add(splitContainer1);
            PnlHomeSurface.Location = new Point(11, 36);
            PnlHomeSurface.Margin = new Padding(2, 12, 2, 10);
            PnlHomeSurface.Name = "PnlHomeSurface";
            PnlHomeSurface.Size = new Size(762, 480);
            PnlHomeSurface.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLightLight;
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 529);
            panel2.Name = "panel2";
            panel2.Size = new Size(784, 32);
            panel2.TabIndex = 2;
            // 
            // FrmHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(784, 561);
            Controls.Add(panel2);
            Controls.Add(PnlHomeSurface);
            Controls.Add(MnsHome);
            MainMenuStrip = MnsHome;
            MinimumSize = new Size(800, 600);
            Name = "FrmHome";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bolt Mod Manager";
            MnsHome.ResumeLayout(false);
            MnsHome.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer4).EndInit();
            splitContainer4.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            PnlHomeSurface.ResumeLayout(false);
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
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private ListView listView1;
        private SplitContainer splitContainer3;
        private Panel PnlHomeSurface;
        private Panel panel2;
        private ListView listView2;
        private Button BtnRun;
        private DataGridView dataGridView1;
        private DataGridViewCheckBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewComboBoxColumn Column6;
        private SplitContainer splitContainer4;
        private ComboBox CmbProfiles;
    }
}