namespace Honeywell.GUI.ExperionGfx
{
    partial class GfxRefForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GfxRefForm));
            this.ResizeCheckBox = new System.Windows.Forms.CheckBox();
            this.shapeBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.AssociatedDisplayDataGrid = new System.Windows.Forms.DataGridView();
            this.UniqueShapeIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GraphicColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShapeSourceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShapeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShapePathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomParameterPointNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomParameterParamNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomPropMiscNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PointColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParameterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomPropMiscValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ToolTipColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CssColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewPointColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewParameterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReplaceMiscPropColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewToolTipColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewCssColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReplaceShapeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomParameterTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GfxShapeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataObjectIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BindingIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CsvIndexColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.GhostRefCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportButton = new System.Windows.Forms.Button();
            this.ShapeReplacementTextBox = new System.Windows.Forms.TextBox();
            this.GfxPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ReplaceShapeButton = new System.Windows.Forms.Button();
            this.GoButton = new System.Windows.Forms.Button();
            this.ExportButton = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.HelpToolStripButton = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.shapeBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AssociatedDisplayDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ResizeCheckBox
            // 
            this.ResizeCheckBox.AutoSize = true;
            this.ResizeCheckBox.Location = new System.Drawing.Point(1156, 39);
            this.ResizeCheckBox.Name = "ResizeCheckBox";
            this.ResizeCheckBox.Size = new System.Drawing.Size(102, 19);
            this.ResizeCheckBox.TabIndex = 15;
            this.ResizeCheckBox.Text = "Resize shapes?";
            this.ResizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // AssociatedDisplayDataGrid
            // 
            this.AssociatedDisplayDataGrid.AllowUserToAddRows = false;
            this.AssociatedDisplayDataGrid.AllowUserToDeleteRows = false;
            this.AssociatedDisplayDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AssociatedDisplayDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UniqueShapeIdColumn,
            this.GraphicColumn,
            this.ShapeSourceColumn,
            this.ShapeNameColumn,
            this.ShapePathColumn,
            this.CustomParameterPointNameColumn,
            this.CustomParameterParamNameColumn,
            this.CustomPropMiscNameColumn,
            this.PointColumn,
            this.ParameterColumn,
            this.CustomPropMiscValueColumn,
            this.ToolTipColumn,
            this.CssColumn,
            this.TypeColumn,
            this.NewPointColumn,
            this.NewParameterColumn,
            this.ReplaceMiscPropColumn,
            this.NewToolTipColumn,
            this.NewCssColumn,
            this.ReplaceShapeColumn,
            this.CustomParameterTypeColumn,
            this.GfxShapeColumn,
            this.DataObjectIdColumn,
            this.BindingIdColumn,
            this.CsvIndexColumn});
            this.AssociatedDisplayDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssociatedDisplayDataGrid.Location = new System.Drawing.Point(0, 0);
            this.AssociatedDisplayDataGrid.Name = "AssociatedDisplayDataGrid";
            this.AssociatedDisplayDataGrid.RowHeadersWidth = 51;
            this.AssociatedDisplayDataGrid.RowTemplate.Height = 25;
            this.AssociatedDisplayDataGrid.Size = new System.Drawing.Size(1425, 356);
            this.AssociatedDisplayDataGrid.TabIndex = 7;
            // 
            // UniqueShapeIdColumn
            // 
            this.UniqueShapeIdColumn.HeaderText = "Unique Shape ID";
            this.UniqueShapeIdColumn.Name = "UniqueShapeIdColumn";
            // 
            // GraphicColumn
            // 
            this.GraphicColumn.HeaderText = "Graphic";
            this.GraphicColumn.MinimumWidth = 6;
            this.GraphicColumn.Name = "GraphicColumn";
            this.GraphicColumn.Width = 200;
            // 
            // ShapeSourceColumn
            // 
            this.ShapeSourceColumn.HeaderText = "Shape Source";
            this.ShapeSourceColumn.MinimumWidth = 6;
            this.ShapeSourceColumn.Name = "ShapeSourceColumn";
            this.ShapeSourceColumn.Width = 200;
            // 
            // ShapeNameColumn
            // 
            this.ShapeNameColumn.HeaderText = "Shape Name";
            this.ShapeNameColumn.MinimumWidth = 6;
            this.ShapeNameColumn.Name = "ShapeNameColumn";
            this.ShapeNameColumn.Width = 200;
            // 
            // ShapePathColumn
            // 
            this.ShapePathColumn.HeaderText = "Shape Path";
            this.ShapePathColumn.MinimumWidth = 6;
            this.ShapePathColumn.Name = "ShapePathColumn";
            this.ShapePathColumn.Width = 200;
            // 
            // CustomParameterPointNameColumn
            // 
            this.CustomParameterPointNameColumn.HeaderText = "Custom Property (Point)";
            this.CustomParameterPointNameColumn.MinimumWidth = 6;
            this.CustomParameterPointNameColumn.Name = "CustomParameterPointNameColumn";
            this.CustomParameterPointNameColumn.Width = 125;
            // 
            // CustomParameterParamNameColumn
            // 
            this.CustomParameterParamNameColumn.HeaderText = "Custom Property (Parameter)";
            this.CustomParameterParamNameColumn.MinimumWidth = 6;
            this.CustomParameterParamNameColumn.Name = "CustomParameterParamNameColumn";
            this.CustomParameterParamNameColumn.Width = 125;
            // 
            // CustomPropMiscNameColumn
            // 
            this.CustomPropMiscNameColumn.HeaderText = "Custom Property (Misc)";
            this.CustomPropMiscNameColumn.MinimumWidth = 6;
            this.CustomPropMiscNameColumn.Name = "CustomPropMiscNameColumn";
            this.CustomPropMiscNameColumn.Width = 125;
            // 
            // PointColumn
            // 
            this.PointColumn.HeaderText = "Point";
            this.PointColumn.MinimumWidth = 6;
            this.PointColumn.Name = "PointColumn";
            this.PointColumn.Width = 200;
            // 
            // ParameterColumn
            // 
            this.ParameterColumn.HeaderText = "Parameter";
            this.ParameterColumn.MinimumWidth = 6;
            this.ParameterColumn.Name = "ParameterColumn";
            this.ParameterColumn.Width = 200;
            // 
            // CustomPropMiscValueColumn
            // 
            this.CustomPropMiscValueColumn.HeaderText = "Misc Prop Value";
            this.CustomPropMiscValueColumn.MinimumWidth = 6;
            this.CustomPropMiscValueColumn.Name = "CustomPropMiscValueColumn";
            this.CustomPropMiscValueColumn.Width = 125;
            // 
            // ToolTipColumn
            // 
            this.ToolTipColumn.HeaderText = "ToolTip";
            this.ToolTipColumn.Name = "ToolTipColumn";
            // 
            // CssColumn
            // 
            this.CssColumn.HeaderText = "CSS";
            this.CssColumn.Name = "CssColumn";
            // 
            // TypeColumn
            // 
            this.TypeColumn.HeaderText = "Type";
            this.TypeColumn.MinimumWidth = 6;
            this.TypeColumn.Name = "TypeColumn";
            this.TypeColumn.Width = 125;
            // 
            // NewPointColumn
            // 
            this.NewPointColumn.HeaderText = "Replace Point Name";
            this.NewPointColumn.MinimumWidth = 6;
            this.NewPointColumn.Name = "NewPointColumn";
            this.NewPointColumn.Width = 125;
            // 
            // NewParameterColumn
            // 
            this.NewParameterColumn.HeaderText = "Replace Parameter";
            this.NewParameterColumn.MinimumWidth = 6;
            this.NewParameterColumn.Name = "NewParameterColumn";
            this.NewParameterColumn.Width = 125;
            // 
            // ReplaceMiscPropColumn
            // 
            this.ReplaceMiscPropColumn.HeaderText = "Replace Misc Prop Value";
            this.ReplaceMiscPropColumn.MinimumWidth = 6;
            this.ReplaceMiscPropColumn.Name = "ReplaceMiscPropColumn";
            this.ReplaceMiscPropColumn.Width = 125;
            // 
            // NewToolTipColumn
            // 
            this.NewToolTipColumn.HeaderText = "Replace ToolTip";
            this.NewToolTipColumn.Name = "NewToolTipColumn";
            // 
            // NewCssColumn
            // 
            this.NewCssColumn.HeaderText = "New CSS";
            this.NewCssColumn.Name = "NewCssColumn";
            // 
            // ReplaceShapeColumn
            // 
            this.ReplaceShapeColumn.HeaderText = "Replace Shape";
            this.ReplaceShapeColumn.MinimumWidth = 6;
            this.ReplaceShapeColumn.Name = "ReplaceShapeColumn";
            this.ReplaceShapeColumn.Width = 125;
            // 
            // CustomParameterTypeColumn
            // 
            this.CustomParameterTypeColumn.HeaderText = "Custom Parameter Type";
            this.CustomParameterTypeColumn.MinimumWidth = 6;
            this.CustomParameterTypeColumn.Name = "CustomParameterTypeColumn";
            this.CustomParameterTypeColumn.Visible = false;
            this.CustomParameterTypeColumn.Width = 125;
            // 
            // GfxShapeColumn
            // 
            this.GfxShapeColumn.HeaderText = "Gfx.Shape";
            this.GfxShapeColumn.Name = "GfxShapeColumn";
            // 
            // DataObjectIdColumn
            // 
            this.DataObjectIdColumn.HeaderText = "Data Object ID";
            this.DataObjectIdColumn.MinimumWidth = 6;
            this.DataObjectIdColumn.Name = "DataObjectIdColumn";
            this.DataObjectIdColumn.Visible = false;
            this.DataObjectIdColumn.Width = 125;
            // 
            // BindingIdColumn
            // 
            this.BindingIdColumn.HeaderText = "Binding ID";
            this.BindingIdColumn.MinimumWidth = 6;
            this.BindingIdColumn.Name = "BindingIdColumn";
            this.BindingIdColumn.Visible = false;
            this.BindingIdColumn.Width = 125;
            // 
            // CsvIndexColumn
            // 
            this.CsvIndexColumn.HeaderText = "CSV Custom Parameter Index";
            this.CsvIndexColumn.MinimumWidth = 6;
            this.CsvIndexColumn.Name = "CsvIndexColumn";
            this.CsvIndexColumn.Visible = false;
            this.CsvIndexColumn.Width = 125;
            // 
            // ReplaceButton
            // 
            this.ReplaceButton.Location = new System.Drawing.Point(337, 57);
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.Size = new System.Drawing.Size(159, 23);
            this.ReplaceButton.TabIndex = 10;
            this.ReplaceButton.Text = "Replace Points && Params";
            this.ReplaceButton.UseVisualStyleBackColor = true;
            this.ReplaceButton.Click += new System.EventHandler(this.ReplaceButton_Click);
            // 
            // GhostRefCheckBox
            // 
            this.GhostRefCheckBox.AutoSize = true;
            this.GhostRefCheckBox.Checked = true;
            this.GhostRefCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GhostRefCheckBox.Location = new System.Drawing.Point(502, 60);
            this.GhostRefCheckBox.Name = "GhostRefCheckBox";
            this.GhostRefCheckBox.Size = new System.Drawing.Size(280, 19);
            this.GhostRefCheckBox.TabIndex = 11;
            this.GhostRefCheckBox.Text = "Remove ghost refs from .dsd (Shape Name = ??)";
            this.GhostRefCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportButton
            // 
            this.ImportButton.Location = new System.Drawing.Point(12, 57);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(110, 23);
            this.ImportButton.TabIndex = 9;
            this.ImportButton.Text = "Import";
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // ShapeReplacementTextBox
            // 
            this.ShapeReplacementTextBox.Location = new System.Drawing.Point(1033, 29);
            this.ShapeReplacementTextBox.Name = "ShapeReplacementTextBox";
            this.ShapeReplacementTextBox.Size = new System.Drawing.Size(272, 23);
            this.ShapeReplacementTextBox.TabIndex = 12;
            this.ShapeReplacementTextBox.Text = "_ReplaceMe.htm";
            // 
            // GfxPath
            // 
            this.GfxPath.Location = new System.Drawing.Point(248, 28);
            this.GfxPath.Name = "GfxPath";
            this.GfxPath.Size = new System.Drawing.Size(525, 23);
            this.GfxPath.TabIndex = 6;
            this.GfxPath.Text = "C:\\ProgramData\\Honeywell\\Experion PKS\\Client\\Abstract";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(869, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "Shape Replacement HTM:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(152, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Graphics folder:";
            // 
            // ReplaceShapeButton
            // 
            this.ReplaceShapeButton.Location = new System.Drawing.Point(1033, 58);
            this.ReplaceShapeButton.Name = "ReplaceShapeButton";
            this.ReplaceShapeButton.Size = new System.Drawing.Size(100, 23);
            this.ReplaceShapeButton.TabIndex = 14;
            this.ReplaceShapeButton.Text = "Replace Shapes";
            this.ReplaceShapeButton.UseVisualStyleBackColor = true;
            this.ReplaceShapeButton.Click += new System.EventHandler(this.ReplaceShapeButton_Click);
            // 
            // GoButton
            // 
            this.GoButton.Location = new System.Drawing.Point(248, 57);
            this.GoButton.Name = "GoButton";
            this.GoButton.Size = new System.Drawing.Size(83, 23);
            this.GoButton.TabIndex = 4;
            this.GoButton.Text = "Parse Folder";
            this.GoButton.UseVisualStyleBackColor = true;
            this.GoButton.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(12, 32);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(110, 23);
            this.ExportButton.TabIndex = 8;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            this.splitContainer1.Panel1.Controls.Add(this.ExportButton);
            this.splitContainer1.Panel1.Controls.Add(this.GoButton);
            this.splitContainer1.Panel1.Controls.Add(this.ReplaceShapeButton);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.GfxPath);
            this.splitContainer1.Panel1.Controls.Add(this.ShapeReplacementTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.ImportButton);
            this.splitContainer1.Panel1.Controls.Add(this.GhostRefCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.ReplaceButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.AssociatedDisplayDataGrid);
            this.splitContainer1.Size = new System.Drawing.Size(1425, 450);
            this.splitContainer1.SplitterDistance = 90;
            this.splitContainer1.TabIndex = 16;
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1425, 25);
            this.toolStrip1.TabIndex = 15;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // HelpToolStripButton
            // 
            this.HelpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.HelpToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("HelpToolStripButton.Image")));
            this.HelpToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.HelpToolStripButton.Name = "HelpToolStripButton";
            this.HelpToolStripButton.Size = new System.Drawing.Size(36, 22);
            this.HelpToolStripButton.Text = "Help";
            this.HelpToolStripButton.Click += new System.EventHandler(this.HelpToolStripButton_Click);
            // 
            // GfxRefForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1425, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ResizeCheckBox);
            this.Name = "GfxRefForm";
            this.Text = "Experion Graphics Parser";
            ((System.ComponentModel.ISupportInitialize)(this.shapeBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AssociatedDisplayDataGrid)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private CheckBox ResizeCheckBox;
        private BindingSource shapeBindingSource;
        private DataGridView AssociatedDisplayDataGrid;
        private DataGridViewTextBoxColumn UniqueShapeIdColumn;
        private DataGridViewTextBoxColumn GraphicColumn;
        private DataGridViewTextBoxColumn ShapeSourceColumn;
        private DataGridViewTextBoxColumn ShapeNameColumn;
        private DataGridViewTextBoxColumn ShapePathColumn;
        private DataGridViewTextBoxColumn CustomParameterPointNameColumn;
        private DataGridViewTextBoxColumn CustomParameterParamNameColumn;
        private DataGridViewTextBoxColumn CustomPropMiscNameColumn;
        private DataGridViewTextBoxColumn PointColumn;
        private DataGridViewTextBoxColumn ParameterColumn;
        private DataGridViewTextBoxColumn CustomPropMiscValueColumn;
        private DataGridViewTextBoxColumn ToolTipColumn;
        private DataGridViewTextBoxColumn CssColumn;
        private DataGridViewTextBoxColumn TypeColumn;
        private DataGridViewTextBoxColumn NewPointColumn;
        private DataGridViewTextBoxColumn NewParameterColumn;
        private DataGridViewTextBoxColumn ReplaceMiscPropColumn;
        private DataGridViewTextBoxColumn NewToolTipColumn;
        private DataGridViewTextBoxColumn NewCssColumn;
        private DataGridViewTextBoxColumn ReplaceShapeColumn;
        private DataGridViewTextBoxColumn CustomParameterTypeColumn;
        private DataGridViewTextBoxColumn GfxShapeColumn;
        private DataGridViewTextBoxColumn DataObjectIdColumn;
        private DataGridViewTextBoxColumn BindingIdColumn;
        private DataGridViewTextBoxColumn CsvIndexColumn;
        private Button ReplaceButton;
        private CheckBox GhostRefCheckBox;
        private Button ImportButton;
        private TextBox ShapeReplacementTextBox;
        private TextBox GfxPath;
        private Label label2;
        private Label label1;
        private Button ReplaceShapeButton;
        private Button GoButton;
        private Button ExportButton;
        private SplitContainer splitContainer1;
        private ToolStrip toolStrip1;
        private ToolStripButton HelpToolStripButton;
    }
}