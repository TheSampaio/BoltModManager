namespace Bolt.Controls
{
    partial class TextEntry
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            LblTextEntry = new Label();
            TxtTextEntry = new TextBox();
            SuspendLayout();
            // 
            // LblTextEntry
            // 
            LblTextEntry.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            LblTextEntry.AutoSize = true;
            LblTextEntry.Location = new Point(10, 5);
            LblTextEntry.Name = "LblTextEntry";
            LblTextEntry.Size = new Size(38, 15);
            LblTextEntry.TabIndex = 0;
            LblTextEntry.Text = "label1";
            // 
            // TxtTextEntry
            // 
            TxtTextEntry.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TxtTextEntry.Location = new Point(10, 25);
            TxtTextEntry.Margin = new Padding(10, 5, 10, 10);
            TxtTextEntry.Name = "TxtTextEntry";
            TxtTextEntry.Size = new Size(400, 23);
            TxtTextEntry.TabIndex = 1;
            // 
            // TextEntry
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(TxtTextEntry);
            Controls.Add(LblTextEntry);
            Name = "TextEntry";
            Size = new Size(421, 59);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label LblTextEntry;
        private TextBox TxtTextEntry;
    }
}
