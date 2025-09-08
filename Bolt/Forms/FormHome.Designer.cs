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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmHome));
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
            SplGame = new SplitContainer();
            BtnRun = new Button();
            NewProfile = new Button();
            BtnRemoveProfile = new Button();
            CmbProfiles = new ComboBox();
            TabGame = new TabControl();
            tabPage3 = new TabPage();
            listView2 = new ListView();
            splitContainer2 = new SplitContainer();
            TabPackages = new TabControl();
            tabPage1 = new TabPage();
            SplPackages = new SplitContainer();
            PrgImport = new ProgressBar();
            BtnImport = new Button();
            LvwModifications = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            tabControl1 = new TabControl();
            tabPage4 = new TabPage();
            listView1 = new ListView();
            tabPage5 = new TabPage();
            PnlHomeSurface = new Panel();
            panel2 = new Panel();
            label2 = new Label();
            LblStatus = new Label();
            MnsHome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SplGame).BeginInit();
            SplGame.Panel1.SuspendLayout();
            SplGame.Panel2.SuspendLayout();
            SplGame.SuspendLayout();
            TabGame.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            TabPackages.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SplPackages).BeginInit();
            SplPackages.Panel1.SuspendLayout();
            SplPackages.Panel2.SuspendLayout();
            SplPackages.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage4.SuspendLayout();
            PnlHomeSurface.SuspendLayout();
            panel2.SuspendLayout();
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
            quitGameToolStripMenuItem.Click += QuitGame_ToolStripMenuItem_Click;
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
            splitContainer1.FixedPanel = FixedPanel.Panel1;
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
            splitContainer3.Panel1.Controls.Add(SplGame);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(TabGame);
            splitContainer3.Size = new Size(300, 480);
            splitContainer3.SplitterDistance = 104;
            splitContainer3.SplitterWidth = 8;
            splitContainer3.TabIndex = 0;
            splitContainer3.TabStop = false;
            // 
            // SplGame
            // 
            SplGame.Dock = DockStyle.Fill;
            SplGame.IsSplitterFixed = true;
            SplGame.Location = new Point(0, 0);
            SplGame.Margin = new Padding(0);
            SplGame.Name = "SplGame";
            SplGame.Orientation = Orientation.Horizontal;
            // 
            // SplGame.Panel1
            // 
            SplGame.Panel1.Controls.Add(BtnRun);
            // 
            // SplGame.Panel2
            // 
            SplGame.Panel2.Controls.Add(NewProfile);
            SplGame.Panel2.Controls.Add(BtnRemoveProfile);
            SplGame.Panel2.Controls.Add(CmbProfiles);
            SplGame.Size = new Size(300, 104);
            SplGame.SplitterDistance = 64;
            SplGame.SplitterWidth = 8;
            SplGame.TabIndex = 0;
            SplGame.TabStop = false;
            // 
            // BtnRun
            // 
            BtnRun.BackColor = SystemColors.Window;
            BtnRun.Cursor = Cursors.Hand;
            BtnRun.Dock = DockStyle.Fill;
            BtnRun.FlatAppearance.BorderSize = 0;
            BtnRun.Font = new Font("Segoe UI", 12F);
            BtnRun.ImageAlign = ContentAlignment.MiddleLeft;
            BtnRun.Location = new Point(0, 0);
            BtnRun.Margin = new Padding(0);
            BtnRun.MaximumSize = new Size(0, 64);
            BtnRun.MinimumSize = new Size(300, 64);
            BtnRun.Name = "BtnRun";
            BtnRun.Padding = new Padding(10);
            BtnRun.Size = new Size(300, 64);
            BtnRun.TabIndex = 0;
            BtnRun.TabStop = false;
            BtnRun.Text = "No Game Loaded";
            BtnRun.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnRun.UseVisualStyleBackColor = false;
            BtnRun.Click += BtnRun_Click;
            // 
            // NewProfile
            // 
            NewProfile.BackgroundImage = (Image)resources.GetObject("NewProfile.BackgroundImage");
            NewProfile.BackgroundImageLayout = ImageLayout.Zoom;
            NewProfile.Font = new Font("Segoe UI", 9F);
            NewProfile.Location = new Point(237, 0);
            NewProfile.Margin = new Padding(0);
            NewProfile.MaximumSize = new Size(32, 32);
            NewProfile.MinimumSize = new Size(32, 32);
            NewProfile.Name = "NewProfile";
            NewProfile.Size = new Size(32, 32);
            NewProfile.TabIndex = 2;
            NewProfile.TabStop = false;
            NewProfile.UseVisualStyleBackColor = true;
            // 
            // BtnRemoveProfile
            // 
            BtnRemoveProfile.BackgroundImage = (Image)resources.GetObject("BtnRemoveProfile.BackgroundImage");
            BtnRemoveProfile.BackgroundImageLayout = ImageLayout.Zoom;
            BtnRemoveProfile.Dock = DockStyle.Right;
            BtnRemoveProfile.Font = new Font("Segoe UI", 9F);
            BtnRemoveProfile.Location = new Point(268, 0);
            BtnRemoveProfile.Margin = new Padding(0);
            BtnRemoveProfile.MaximumSize = new Size(32, 32);
            BtnRemoveProfile.MinimumSize = new Size(32, 32);
            BtnRemoveProfile.Name = "BtnRemoveProfile";
            BtnRemoveProfile.Size = new Size(32, 32);
            BtnRemoveProfile.TabIndex = 1;
            BtnRemoveProfile.TabStop = false;
            BtnRemoveProfile.UseVisualStyleBackColor = true;
            // 
            // CmbProfiles
            // 
            CmbProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CmbProfiles.DropDownStyle = ComboBoxStyle.DropDownList;
            CmbProfiles.DropDownWidth = 256;
            CmbProfiles.Font = new Font("Segoe UI", 13F);
            CmbProfiles.FormattingEnabled = true;
            CmbProfiles.IntegralHeight = false;
            CmbProfiles.Location = new Point(0, 0);
            CmbProfiles.Margin = new Padding(0, 0, 1, 0);
            CmbProfiles.Name = "CmbProfiles";
            CmbProfiles.Size = new Size(236, 31);
            CmbProfiles.TabIndex = 0;
            CmbProfiles.TabStop = false;
            // 
            // TabGame
            // 
            TabGame.Controls.Add(tabPage3);
            TabGame.Dock = DockStyle.Fill;
            TabGame.Location = new Point(0, 0);
            TabGame.Margin = new Padding(0);
            TabGame.Name = "TabGame";
            TabGame.SelectedIndex = 0;
            TabGame.Size = new Size(300, 368);
            TabGame.TabIndex = 0;
            TabGame.TabStop = false;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(listView2);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(292, 340);
            tabPage3.TabIndex = 1;
            tabPage3.Text = "Properties";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // listView2
            // 
            listView2.BorderStyle = BorderStyle.None;
            listView2.Dock = DockStyle.Fill;
            listView2.Location = new Point(0, 0);
            listView2.Margin = new Padding(0);
            listView2.Name = "listView2";
            listView2.Size = new Size(292, 340);
            listView2.TabIndex = 0;
            listView2.UseCompatibleStateImageBehavior = false;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel2;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(TabPackages);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tabControl1);
            splitContainer2.Panel2MinSize = 200;
            splitContainer2.Size = new Size(454, 480);
            splitContainer2.SplitterDistance = 234;
            splitContainer2.SplitterWidth = 8;
            splitContainer2.TabIndex = 0;
            splitContainer2.TabStop = false;
            // 
            // TabPackages
            // 
            TabPackages.Controls.Add(tabPage1);
            TabPackages.Dock = DockStyle.Fill;
            TabPackages.Location = new Point(0, 0);
            TabPackages.Margin = new Padding(0);
            TabPackages.Name = "TabPackages";
            TabPackages.SelectedIndex = 0;
            TabPackages.Size = new Size(454, 234);
            TabPackages.TabIndex = 0;
            TabPackages.TabStop = false;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(SplPackages);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Margin = new Padding(0);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(446, 206);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Packages";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // SplPackages
            // 
            SplPackages.Dock = DockStyle.Fill;
            SplPackages.FixedPanel = FixedPanel.Panel1;
            SplPackages.IsSplitterFixed = true;
            SplPackages.Location = new Point(0, 0);
            SplPackages.Margin = new Padding(0);
            SplPackages.Name = "SplPackages";
            SplPackages.Orientation = Orientation.Horizontal;
            // 
            // SplPackages.Panel1
            // 
            SplPackages.Panel1.Controls.Add(PrgImport);
            SplPackages.Panel1.Controls.Add(BtnImport);
            SplPackages.Panel1MinSize = 32;
            // 
            // SplPackages.Panel2
            // 
            SplPackages.Panel2.Controls.Add(LvwModifications);
            SplPackages.Size = new Size(446, 206);
            SplPackages.SplitterDistance = 32;
            SplPackages.SplitterWidth = 8;
            SplPackages.TabIndex = 0;
            SplPackages.TabStop = false;
            // 
            // PrgImport
            // 
            PrgImport.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PrgImport.Location = new Point(0, 1);
            PrgImport.Margin = new Padding(0, 0, 1, 0);
            PrgImport.Name = "PrgImport";
            PrgImport.Size = new Size(413, 30);
            PrgImport.Style = ProgressBarStyle.Continuous;
            PrgImport.TabIndex = 1;
            // 
            // BtnImport
            // 
            BtnImport.BackgroundImage = (Image)resources.GetObject("BtnImport.BackgroundImage");
            BtnImport.BackgroundImageLayout = ImageLayout.Zoom;
            BtnImport.Dock = DockStyle.Right;
            BtnImport.Location = new Point(414, 0);
            BtnImport.Margin = new Padding(0);
            BtnImport.MaximumSize = new Size(32, 32);
            BtnImport.MinimumSize = new Size(32, 32);
            BtnImport.Name = "BtnImport";
            BtnImport.Size = new Size(32, 32);
            BtnImport.TabIndex = 0;
            BtnImport.TabStop = false;
            BtnImport.UseVisualStyleBackColor = true;
            BtnImport.Click += BtnImport_Click;
            // 
            // LvwModifications
            // 
            LvwModifications.Alignment = ListViewAlignment.Left;
            LvwModifications.BorderStyle = BorderStyle.None;
            LvwModifications.CheckBoxes = true;
            LvwModifications.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            LvwModifications.Dock = DockStyle.Fill;
            LvwModifications.FullRowSelect = true;
            LvwModifications.Location = new Point(0, 0);
            LvwModifications.Margin = new Padding(0);
            LvwModifications.Name = "LvwModifications";
            LvwModifications.Size = new Size(446, 166);
            LvwModifications.TabIndex = 0;
            LvwModifications.UseCompatibleStateImageBehavior = false;
            LvwModifications.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Activate";
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Name";
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Version";
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Category";
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Imported On";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(454, 238);
            tabControl1.TabIndex = 0;
            tabControl1.TabStop = false;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(listView1);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Margin = new Padding(0);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(446, 210);
            tabPage4.TabIndex = 0;
            tabPage4.Text = "About";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.BorderStyle = BorderStyle.None;
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(0, 0);
            listView1.Margin = new Padding(0);
            listView1.Name = "listView1";
            listView1.Size = new Size(446, 210);
            listView1.TabIndex = 0;
            listView1.TabStop = false;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(446, 210);
            tabPage5.TabIndex = 1;
            tabPage5.Text = "Files";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // PnlHomeSurface
            // 
            PnlHomeSurface.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PnlHomeSurface.Controls.Add(splitContainer1);
            PnlHomeSurface.Enabled = false;
            PnlHomeSurface.Location = new Point(11, 36);
            PnlHomeSurface.Margin = new Padding(2, 12, 2, 10);
            PnlHomeSurface.Name = "PnlHomeSurface";
            PnlHomeSurface.Size = new Size(762, 480);
            PnlHomeSurface.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLightLight;
            panel2.Controls.Add(label2);
            panel2.Controls.Add(LblStatus);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 529);
            panel2.Name = "panel2";
            panel2.Size = new Size(784, 32);
            panel2.TabIndex = 2;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.ForeColor = SystemColors.WindowFrame;
            label2.Location = new Point(734, 6);
            label2.Name = "label2";
            label2.Size = new Size(39, 19);
            label2.TabIndex = 0;
            label2.Text = "0.1.0";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LblStatus
            // 
            LblStatus.AutoSize = true;
            LblStatus.Font = new Font("Segoe UI", 10F);
            LblStatus.ForeColor = SystemColors.WindowFrame;
            LblStatus.Location = new Point(11, 6);
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(464, 19);
            LblStatus.TabIndex = 0;
            LblStatus.Text = "Press (Ctrl + O) to open a Bolt game file, or (Ctrl + N) to create a new one.";
            LblStatus.TextAlign = ContentAlignment.MiddleLeft;
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
            Load += FrmHome_Load;
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
            SplGame.Panel1.ResumeLayout(false);
            SplGame.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplGame).EndInit();
            SplGame.ResumeLayout(false);
            TabGame.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            TabPackages.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            SplPackages.Panel1.ResumeLayout(false);
            SplPackages.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplPackages).EndInit();
            SplPackages.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            PnlHomeSurface.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
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
        private SplitContainer splitContainer3;
        private Panel PnlHomeSurface;
        private Panel panel2;
        private Button BtnRun;
        private SplitContainer SplGame;
        private ComboBox CmbProfiles;
        private Button BtnRemoveProfile;
        private TabControl TabPackages;
        private TabPage tabPage1;
        private SplitContainer SplPackages;
        private ProgressBar PrgImport;
        private Button BtnImport;
        private TabControl TabGame;
        private TabPage tabPage3;
        private TabControl tabControl1;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private ListView listView1;
        private ListView listView2;
        private Label label2;
        private Label LblStatus;
        private ListView LvwModifications;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private Button NewProfile;
    }
}