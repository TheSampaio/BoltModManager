namespace Bolt
{
    partial class FormMods
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Pnl_DropFile = new Panel();
            label1 = new Label();
            Dgv_ModsTable = new DataGridView();
            Pnl_DropFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Dgv_ModsTable).BeginInit();
            SuspendLayout();
            // 
            // Pnl_DropFile
            // 
            Pnl_DropFile.AllowDrop = true;
            Pnl_DropFile.BackColor = Color.FromArgb(40, 40, 40);
            Pnl_DropFile.BorderStyle = BorderStyle.FixedSingle;
            Pnl_DropFile.Controls.Add(label1);
            Pnl_DropFile.Dock = DockStyle.Bottom;
            Pnl_DropFile.Location = new Point(0, 461);
            Pnl_DropFile.Margin = new Padding(0);
            Pnl_DropFile.Name = "Pnl_DropFile";
            Pnl_DropFile.Size = new Size(784, 100);
            Pnl_DropFile.TabIndex = 0;
            Pnl_DropFile.DragDrop += Pnl_DropFile_DragDrop;
            Pnl_DropFile.DragEnter += Pnl_DropFile_DragEnter;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Location = new Point(346, 37);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(98, 21);
            label1.TabIndex = 0;
            label1.Text = "Drop File(s)";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Dgv_ModsTable
            // 
            Dgv_ModsTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Dgv_ModsTable.BackgroundColor = Color.FromArgb(30, 30, 30);
            Dgv_ModsTable.BorderStyle = BorderStyle.None;
            Dgv_ModsTable.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Dgv_ModsTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(50, 50, 50);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(50, 50, 50);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            Dgv_ModsTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            Dgv_ModsTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(30, 30, 30);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(20, 20, 20);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            Dgv_ModsTable.DefaultCellStyle = dataGridViewCellStyle2;
            Dgv_ModsTable.Dock = DockStyle.Fill;
            Dgv_ModsTable.EnableHeadersVisualStyles = false;
            Dgv_ModsTable.GridColor = Color.FromArgb(30, 30, 30);
            Dgv_ModsTable.Location = new Point(0, 0);
            Dgv_ModsTable.Margin = new Padding(10);
            Dgv_ModsTable.Name = "Dgv_ModsTable";
            Dgv_ModsTable.RightToLeft = RightToLeft.No;
            Dgv_ModsTable.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(30, 30, 30);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.White;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(20, 20, 20);
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            Dgv_ModsTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            Dgv_ModsTable.ScrollBars = ScrollBars.Vertical;
            Dgv_ModsTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dgv_ModsTable.Size = new Size(784, 461);
            Dgv_ModsTable.TabIndex = 1;
            // 
            // FormMods
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(784, 561);
            Controls.Add(Dgv_ModsTable);
            Controls.Add(Pnl_DropFile);
            Name = "FormMods";
            Text = "FormMods";
            Shown += FormMods_Shown;
            Pnl_DropFile.ResumeLayout(false);
            Pnl_DropFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Dgv_ModsTable).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel Pnl_DropFile;
        private Label label1;
        private DataGridView Dgv_ModsTable;
    }
}