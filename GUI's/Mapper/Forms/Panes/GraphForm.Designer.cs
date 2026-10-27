namespace Honeywell.GUI.Mapper
{
    partial class GraphForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphForm));
            this.gViewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.SuspendLayout();
            // 
            // gViewer
            // 
            this.gViewer.ArrowheadLength = 10D;
            this.gViewer.AsyncLayout = false;
            this.gViewer.AutoScroll = true;
            this.gViewer.BackColor = System.Drawing.Color.Black;
            this.gViewer.BackwardEnabled = false;
            this.gViewer.BuildHitTree = true;
            this.gViewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.UseSettingsOfTheGraph;
            this.gViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gViewer.EdgeInsertButtonVisible = true;
            this.gViewer.FileName = "";
            this.gViewer.ForwardEnabled = false;
            this.gViewer.Graph = null;
            this.gViewer.InsertingEdge = false;
            this.gViewer.LayoutAlgorithmSettingsButtonVisible = true;
            this.gViewer.LayoutEditingEnabled = true;
            this.gViewer.Location = new System.Drawing.Point(0, 0);
            this.gViewer.LooseOffsetForRouting = 0.25D;
            this.gViewer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gViewer.MouseHitDistance = 0.05D;
            this.gViewer.Name = "gViewer";
            this.gViewer.NavigationVisible = true;
            this.gViewer.NeedToCalculateLayout = true;
            this.gViewer.OffsetForRelaxingInRouting = 0.6D;
            this.gViewer.PaddingForEdgeRouting = 8D;
            this.gViewer.PanButtonPressed = false;
            this.gViewer.SaveAsImageEnabled = true;
            this.gViewer.SaveAsMsaglEnabled = true;
            this.gViewer.SaveButtonVisible = true;
            this.gViewer.SaveGraphButtonVisible = true;
            this.gViewer.SaveInVectorFormatEnabled = true;
            this.gViewer.Size = new System.Drawing.Size(503, 338);
            this.gViewer.TabIndex = 0;
            this.gViewer.TightOffsetForRouting = 0.125D;
            this.gViewer.ToolBarIsVisible = true;
            this.gViewer.Transform = ((Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation)(resources.GetObject("gViewer.Transform")));
            this.gViewer.UndoRedoButtonsVisible = true;
            this.gViewer.WindowZoomButtonPressed = false;
            this.gViewer.ZoomF = 1D;
            this.gViewer.ZoomWindowThreshold = 0.05D;
            this.gViewer.GraphChanged += new System.EventHandler(this.GraphViewer_GraphChanged);
            this.gViewer.ObjectUnderMouseCursorChanged += new System.EventHandler<Microsoft.Msagl.Drawing.ObjectUnderMouseCursorChangedEventArgs>(this.GraphViewer_ObjectUnderMouseCursorChanged);
            this.gViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphViewer_MouseMove);
            this.gViewer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphViewer_KeyDown);
            this.gViewer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphViewer_MouseDown);
            this.gViewer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphViewer_MouseUp);
            // 
            // GraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(503, 338);
            this.CloseButton = false;
            this.CloseButtonVisible = false;
            this.Controls.Add(this.gViewer);
            this.HideOnClose = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "GraphForm";
            this.Text = "Map";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GraphForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn pointTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nTWKNUMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nODENUMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn aMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nODETYPDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn descDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn ebFileDataGridViewTextBoxColumn;
        private DataGridViewCheckBoxColumn packageExistsDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn hWYNUMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn bOXNUMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn sLOTNUMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn mODNUMDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn pVALGIDDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn cTLALGIDDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn paramsDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nodesDataGridViewTextBoxColumn;
        //public ToolStripTextBox MaxNodes = new() { Text = "1000"};
        public Microsoft.Msagl.GraphViewerGdi.GViewer gViewer;
    }
}