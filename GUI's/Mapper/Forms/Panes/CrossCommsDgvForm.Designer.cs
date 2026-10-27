namespace Honeywell.GUI.Mapper
{
    partial class CrossCommsDgvForm
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
            this.sourceTagNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.targetTagNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sourceAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.targetAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.graphIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.crossCommsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.advancedDataGridViewSearchToolBar1 = new Zuby.ADGV.AdvancedDataGridViewSearchToolBar();
            ((System.ComponentModel.ISupportInitialize)(this.AdvancedDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossCommsBindingSource)).BeginInit();
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
            this.sourceTagNameDataGridViewTextBoxColumn,
            this.targetTagNameDataGridViewTextBoxColumn,
            this.sourceAddressDataGridViewTextBoxColumn,
            this.targetAddressDataGridViewTextBoxColumn,
            this.graphIdDataGridViewTextBoxColumn});
            this.AdvancedDataGridView.DataSource = this.crossCommsBindingSource;
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
            this.AdvancedDataGridView.Size = new System.Drawing.Size(405, 310);
            this.AdvancedDataGridView.SortStringChangedInvokeBeforeDatasourceUpdate = true;
            this.AdvancedDataGridView.TabIndex = 1;
            this.AdvancedDataGridView.SortStringChanged += new System.EventHandler<Zuby.ADGV.AdvancedDataGridView.SortEventArgs>(this.CrossCommsAdvancedDataGridView_SortStringChanged);
            this.AdvancedDataGridView.FilterStringChanged += new System.EventHandler<Zuby.ADGV.AdvancedDataGridView.FilterEventArgs>(this.CrossCommsAdvancedDataGridView_FilterStringChanged);
            // 
            // sourceTagNameDataGridViewTextBoxColumn
            // 
            this.sourceTagNameDataGridViewTextBoxColumn.DataPropertyName = "SourceTagName";
            this.sourceTagNameDataGridViewTextBoxColumn.HeaderText = "Source Tag";
            this.sourceTagNameDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.sourceTagNameDataGridViewTextBoxColumn.Name = "sourceTagNameDataGridViewTextBoxColumn";
            this.sourceTagNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.sourceTagNameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // targetTagNameDataGridViewTextBoxColumn
            // 
            this.targetTagNameDataGridViewTextBoxColumn.DataPropertyName = "TargetTagName";
            this.targetTagNameDataGridViewTextBoxColumn.HeaderText = "Target Tag";
            this.targetTagNameDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.targetTagNameDataGridViewTextBoxColumn.Name = "targetTagNameDataGridViewTextBoxColumn";
            this.targetTagNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.targetTagNameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // sourceAddressDataGridViewTextBoxColumn
            // 
            this.sourceAddressDataGridViewTextBoxColumn.DataPropertyName = "SourceAddress";
            this.sourceAddressDataGridViewTextBoxColumn.HeaderText = "Source Network";
            this.sourceAddressDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.sourceAddressDataGridViewTextBoxColumn.Name = "sourceAddressDataGridViewTextBoxColumn";
            this.sourceAddressDataGridViewTextBoxColumn.ReadOnly = true;
            this.sourceAddressDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // targetAddressDataGridViewTextBoxColumn
            // 
            this.targetAddressDataGridViewTextBoxColumn.DataPropertyName = "TargetAddress";
            this.targetAddressDataGridViewTextBoxColumn.HeaderText = "Target Network";
            this.targetAddressDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.targetAddressDataGridViewTextBoxColumn.Name = "targetAddressDataGridViewTextBoxColumn";
            this.targetAddressDataGridViewTextBoxColumn.ReadOnly = true;
            this.targetAddressDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // graphIdDataGridViewTextBoxColumn
            // 
            this.graphIdDataGridViewTextBoxColumn.DataPropertyName = "GraphId";
            this.graphIdDataGridViewTextBoxColumn.HeaderText = "GraphId";
            this.graphIdDataGridViewTextBoxColumn.MinimumWidth = 22;
            this.graphIdDataGridViewTextBoxColumn.Name = "graphIdDataGridViewTextBoxColumn";
            this.graphIdDataGridViewTextBoxColumn.ReadOnly = true;
            this.graphIdDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.graphIdDataGridViewTextBoxColumn.Visible = false;
            // 
            // crossCommsBindingSource
            // 
            this.crossCommsBindingSource.DataSource = typeof(Mapper.CrossComms);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.Default;
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
            this.splitContainer1.Size = new System.Drawing.Size(405, 338);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 2;
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
            this.advancedDataGridViewSearchToolBar1.Size = new System.Drawing.Size(405, 20);
            this.advancedDataGridViewSearchToolBar1.TabIndex = 0;
            this.advancedDataGridViewSearchToolBar1.Text = "advancedDataGridViewSearchToolBar1";
            this.advancedDataGridViewSearchToolBar1.Search += new Zuby.ADGV.AdvancedDataGridViewSearchToolBarSearchEventHandler(this.advancedDataGridViewSearchToolBar1_Search);
            // 
            // CrossCommsDgvForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 338);
            this.CloseButtonVisible = false;
            this.Controls.Add(this.splitContainer1);
            this.HideOnClose = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "CrossCommsDgvForm";
            this.Text = "Cross Communications";
            ((System.ComponentModel.ISupportInitialize)(this.AdvancedDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossCommsBindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public Zuby.ADGV.AdvancedDataGridView AdvancedDataGridView;
        private SplitContainer splitContainer1;
        private Zuby.ADGV.AdvancedDataGridViewSearchToolBar advancedDataGridViewSearchToolBar1;
        private DataGridViewTextBoxColumn sourceTagNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn targetTagNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn sourceAddressDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn targetAddressDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn graphIdDataGridViewTextBoxColumn;
        private BindingSource crossCommsBindingSource;
    }
}