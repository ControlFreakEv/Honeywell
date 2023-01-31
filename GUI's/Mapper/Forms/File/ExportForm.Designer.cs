namespace Honeywell.GUI.Mapper.Forms
{
    partial class ExportForm
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
            this.ExportDataGridView = new System.Windows.Forms.DataGridView();
            this.ExportColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DbDataColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ExportButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ExportDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExportDataGridView
            // 
            this.ExportDataGridView.AllowUserToAddRows = false;
            this.ExportDataGridView.AllowUserToDeleteRows = false;
            this.ExportDataGridView.AllowUserToResizeColumns = false;
            this.ExportDataGridView.AllowUserToResizeRows = false;
            this.ExportDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ExportDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ExportColumn,
            this.DbDataColumn});
            this.ExportDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportDataGridView.Location = new System.Drawing.Point(0, 0);
            this.ExportDataGridView.Name = "ExportDataGridView";
            this.ExportDataGridView.RowHeadersVisible = false;
            this.ExportDataGridView.RowTemplate.Height = 25;
            this.ExportDataGridView.Size = new System.Drawing.Size(177, 302);
            this.ExportDataGridView.TabIndex = 0;
            // 
            // ExportColumn
            // 
            this.ExportColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ExportColumn.HeaderText = "Export";
            this.ExportColumn.Name = "ExportColumn";
            this.ExportColumn.Width = 47;
            // 
            // DbDataColumn
            // 
            this.DbDataColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.DbDataColumn.HeaderText = "Data";
            this.DbDataColumn.Name = "DbDataColumn";
            this.DbDataColumn.ReadOnly = true;
            this.DbDataColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DbDataColumn.Width = 56;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ExportDataGridView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ExportButton);
            this.splitContainer1.Size = new System.Drawing.Size(177, 332);
            this.splitContainer1.SplitterDistance = 302;
            this.splitContainer1.TabIndex = 1;
            // 
            // ExportButton
            // 
            this.ExportButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportButton.Location = new System.Drawing.Point(0, 0);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(177, 26);
            this.ExportButton.TabIndex = 0;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(177, 332);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ExportForm";
            this.Text = "ExportForm";
            this.Load += new System.EventHandler(this.ExportForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ExportDataGridView)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridView ExportDataGridView;
        private SplitContainer splitContainer1;
        private DataGridViewCheckBoxColumn ExportColumn;
        private DataGridViewTextBoxColumn DbDataColumn;
        private Button ExportButton;
    }
}