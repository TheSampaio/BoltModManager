namespace Bolt
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            FlpSideBar = new FlowLayoutPanel();
            button1 = new Button();
            Pnl_Display = new Panel();
            FlpSideBar.SuspendLayout();
            SuspendLayout();
            // 
            // FlpSideBar
            // 
            FlpSideBar.BackColor = Color.FromArgb(40, 40, 40);
            FlpSideBar.Controls.Add(button1);
            FlpSideBar.Dock = DockStyle.Left;
            FlpSideBar.FlowDirection = FlowDirection.TopDown;
            FlpSideBar.Location = new Point(0, 0);
            FlpSideBar.Margin = new Padding(0);
            FlpSideBar.Name = "FlpSideBar";
            FlpSideBar.Size = new Size(200, 561);
            FlpSideBar.TabIndex = 0;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(60, 60, 60);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.White;
            button1.Location = new Point(0, 0);
            button1.Margin = new Padding(0);
            button1.Name = "button1";
            button1.Size = new Size(200, 100);
            button1.TabIndex = 0;
            button1.Text = "Image";
            button1.UseVisualStyleBackColor = false;
            // 
            // Pnl_Display
            // 
            Pnl_Display.BackColor = Color.FromArgb(30, 30, 30);
            Pnl_Display.Dock = DockStyle.Fill;
            Pnl_Display.Location = new Point(200, 0);
            Pnl_Display.Name = "Pnl_Display";
            Pnl_Display.Size = new Size(584, 561);
            Pnl_Display.TabIndex = 1;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(784, 561);
            Controls.Add(Pnl_Display);
            Controls.Add(FlpSideBar);
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bolt";
            Shown += FormMain_Shown;
            FlpSideBar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel FlpSideBar;
        private Button button1;
        private Panel Pnl_Display;
    }
}
