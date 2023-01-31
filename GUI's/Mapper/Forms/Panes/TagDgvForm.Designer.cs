namespace Honeywell.GUI.Mapper
{
    partial class TagDgvForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.AdvancedDataGridView = new Zuby.ADGV.AdvancedDataGridView();
            this.dbTdcTagBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.advancedDataGridViewSearchToolBar1 = new Zuby.ADGV.AdvancedDataGridViewSearchToolBar();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pointTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LcnAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PVALGID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CTLALGID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ALGIDDAC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.AdvancedDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTdcTagBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // AdvancedDataGridView
            // 
            this.AdvancedDataGridView.AllowUserToAddRows = false;
            this.AdvancedDataGridView.AllowUserToDeleteRows = false;
            this.AdvancedDataGridView.AllowUserToResizeRows = false;
            this.AdvancedDataGridView.AutoGenerateColumns = false;
            this.AdvancedDataGridView.BackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.AdvancedDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.AdvancedDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AdvancedDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.pointTypeDataGridViewTextBoxColumn,
            this.LcnAddress,
            this.idDataGridViewTextBoxColumn,
            this.PVALGID,
            this.CTLALGID,
            this.ALGIDDAC});
            this.AdvancedDataGridView.DataSource = this.dbTdcTagBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.AdvancedDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.AdvancedDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdvancedDataGridView.EnableHeadersVisualStyles = false;
            this.AdvancedDataGridView.FilterAndSortEnabled = true;
            this.AdvancedDataGridView.FilterStringChangedInvokeBeforeDatasourceUpdate = true;
            this.AdvancedDataGridView.GridColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.AdvancedDataGridView.Location = new System.Drawing.Point(0, 0);
            this.AdvancedDataGridView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AdvancedDataGridView.Name = "AdvancedDataGridView";
            this.AdvancedDataGridView.ReadOnly = true;
            this.AdvancedDataGridView.RightToLeft = System.Windows.Forms.RightToLeft.No;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.AdvancedDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.AdvancedDataGridView.RowHeadersVisible = false;
            this.AdvancedDataGridView.RowHeadersWidth = 51;
            this.AdvancedDataGridView.RowTemplate.Height = 29;
            this.AdvancedDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.AdvancedDataGridView.Size = new System.Drawing.Size(448, 310);
            this.AdvancedDataGridView.SortStringChangedInvokeBeforeDatasourceUpdate = true;
            this.AdvancedDataGridView.TabIndex = 0;
            this.AdvancedDataGridView.SortStringChanged += new System.EventHandler<Zuby.ADGV.AdvancedDataGridView.SortEventArgs>(this.AdvancedDataGridView1_SortStringChanged);
            this.AdvancedDataGridView.FilterStringChanged += new System.EventHandler<Zuby.ADGV.AdvancedDataGridView.FilterEventArgs>(this.AdvancedDataGridView1_FilterStringChanged);
            // 
            // dbTdcTagBindingSource
            // 
            this.dbTdcTagBindingSource.DataSource = typeof(Database.DbTdcTag);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.advancedDataGridViewSearchToolBar1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.AdvancedDataGridView);
            this.splitContainer1.Size = new System.Drawing.Size(448, 338);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 1;
            // 
            // advancedDataGridViewSearchToolBar1
            // 
            this.advancedDataGridViewSearchToolBar1.AllowMerge = false;
            this.advancedDataGridViewSearchToolBar1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.advancedDataGridViewSearchToolBar1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.advancedDataGridViewSearchToolBar1.Location = new System.Drawing.Point(0, 0);
            this.advancedDataGridViewSearchToolBar1.MaximumSize = new System.Drawing.Size(0, 20);
            this.advancedDataGridViewSearchToolBar1.MinimumSize = new System.Drawing.Size(0, 20);
            this.advancedDataGridViewSearchToolBar1.Name = "advancedDataGridViewSearchToolBar1";
            this.advancedDataGridViewSearchToolBar1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.advancedDataGridViewSearchToolBar1.Size = new System.Drawing.Size(448, 20);
            this.advancedDataGridViewSearchToolBar1.TabIndex = 0;
            this.advancedDataGridViewSearchToolBar1.Text = "advancedDataGridViewSearchToolBar1";
            this.advancedDataGridViewSearchToolBar1.Search += new Zuby.ADGV.AdvancedDataGridViewSearchToolBarSearchEventHandler(this.advancedDataGridViewSearchToolBar1_Search);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.nameDataGridViewTextBoxColumn.Width = 91;
            // 
            // pointTypeDataGridViewTextBoxColumn
            // 
            this.pointTypeDataGridViewTextBoxColumn.DataPropertyName = "PointType";
            this.pointTypeDataGridViewTextBoxColumn.HeaderText = "PointType";
            this.pointTypeDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.pointTypeDataGridViewTextBoxColumn.Name = "pointTypeDataGridViewTextBoxColumn";
            this.pointTypeDataGridViewTextBoxColumn.ReadOnly = true;
            this.pointTypeDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.pointTypeDataGridViewTextBoxColumn.Width = 126;
            // 
            // LcnAddress
            // 
            this.LcnAddress.DataPropertyName = "LcnAddress";
            this.LcnAddress.HeaderText = "Network";
            this.LcnAddress.MinimumWidth = 22;
            this.LcnAddress.Name = "LcnAddress";
            this.LcnAddress.ReadOnly = true;
            this.LcnAddress.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Id";
            this.idDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.ReadOnly = true;
            this.idDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.idDataGridViewTextBoxColumn.Visible = false;
            this.idDataGridViewTextBoxColumn.Width = 51;
            // 
            // PVALGID
            // 
            this.PVALGID.DataPropertyName = "PVALGID";
            this.PVALGID.HeaderText = "PVALGID";
            this.PVALGID.MinimumWidth = 22;
            this.PVALGID.Name = "PVALGID";
            this.PVALGID.ReadOnly = true;
            this.PVALGID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // CTLALGID
            // 
            this.CTLALGID.DataPropertyName = "CTLALGID";
            this.CTLALGID.HeaderText = "CTLALGID";
            this.CTLALGID.MinimumWidth = 22;
            this.CTLALGID.Name = "CTLALGID";
            this.CTLALGID.ReadOnly = true;
            this.CTLALGID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // ALGIDDAC
            // 
            this.ALGIDDAC.DataPropertyName = "ALGIDDAC";
            this.ALGIDDAC.HeaderText = "ALGIDDAC";
            this.ALGIDDAC.MinimumWidth = 22;
            this.ALGIDDAC.Name = "ALGIDDAC";
            this.ALGIDDAC.ReadOnly = true;
            this.ALGIDDAC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // TagDgvForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(448, 338);
            this.CloseButtonVisible = false;
            this.Controls.Add(this.splitContainer1);
            this.HideOnClose = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "TagDgvForm";
            this.Text = "TDC Tags";
            ((System.ComponentModel.ISupportInitialize)(this.AdvancedDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTdcTagBindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public Zuby.ADGV.AdvancedDataGridView AdvancedDataGridView;
        private BindingSource dbTdcTagBindingSource;
        private SplitContainer splitContainer1;
        private Zuby.ADGV.AdvancedDataGridViewSearchToolBar advancedDataGridViewSearchToolBar1;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn pointTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn LcnAddress;
        private DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn PVALGID;
        private DataGridViewTextBoxColumn CTLALGID;
        private DataGridViewTextBoxColumn ALGIDDAC;
    }
}