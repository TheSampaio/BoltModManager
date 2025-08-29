namespace Bolt.Forms
{
    partial class FormHome
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
            PnlGameImage = new Panel();
            BtnGames = new Button();
            FlpSurface = new FlowLayoutPanel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(PnlGameImage);
            panel1.Controls.Add(BtnGames);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(200, 561);
            panel1.TabIndex = 0;
            // 
            // PnlGameImage
            // 
            PnlGameImage.BackColor = Color.LightGray;
            PnlGameImage.Location = new Point(0, 0);
            PnlGameImage.Margin = new Padding(0);
            PnlGameImage.Name = "PnlGameImage";
            PnlGameImage.Size = new Size(200, 100);
            PnlGameImage.TabIndex = 1;
            // 
            // BtnGames
            // 
            BtnGames.FlatAppearance.BorderSize = 0;
            BtnGames.FlatStyle = FlatStyle.Flat;
            BtnGames.Location = new Point(0, 100);
            BtnGames.Margin = new Padding(0);
            BtnGames.Name = "BtnGames";
            BtnGames.Size = new Size(200, 50);
            BtnGames.TabIndex = 0;
            BtnGames.Text = "Games";
            BtnGames.UseVisualStyleBackColor = true;
            // 
            // FlpSurface
            // 
            FlpSurface.Dock = DockStyle.Fill;
            FlpSurface.Location = new Point(200, 0);
            FlpSurface.Name = "FlpSurface";
            FlpSurface.Size = new Size(584, 561);
            FlpSurface.TabIndex = 1;
            // 
            // FormHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(FlpSurface);
            Controls.Add(panel1);
            MinimumSize = new Size(800, 600);
            Name = "FormHome";
            Text = "Bolt Mod Manager";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button BtnGames;
        private Panel PnlGameImage;
        private FlowLayoutPanel FlpSurface;
    }
}