namespace Honeywell.GUI.Mapper
{
    partial class CLViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CLViewerForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.PackageToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveCLToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.PrettyPrintToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.OriginalCLToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ModifiedCLToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.CommentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.UncommentTolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.HighlightToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.HighlightToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.RedToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.FindToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ScintillaTextEditorCL = new Mapper.ScintillaExt.ScintillaTextEditor();
            this.GreenToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.BlueToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.YellowToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.OrangeToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.NoneToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ScintillaTextEditorCL);
            this.splitContainer1.Size = new System.Drawing.Size(942, 338);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.PackageToolStripComboBox,
            this.toolStripSeparator8,
            this.SaveCLToolStripButton,
            this.toolStripSeparator2,
            this.PrettyPrintToolStripButton,
            this.toolStripSeparator3,
            this.OriginalCLToolStripButton,
            this.toolStripSeparator1,
            this.ModifiedCLToolStripButton,
            this.toolStripSeparator4,
            this.CommentToolStripButton,
            this.toolStripSeparator5,
            this.UncommentTolStripButton,
            this.toolStripSeparator6,
            this.HighlightToolStripButton,
            this.HighlightToolStripDropDownButton,
            this.toolStripSeparator7,
            this.FindToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(942, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(51, 22);
            this.toolStripLabel1.Text = "Package";
            // 
            // PackageToolStripComboBox
            // 
            this.PackageToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PackageToolStripComboBox.Name = "PackageToolStripComboBox";
            this.PackageToolStripComboBox.Size = new System.Drawing.Size(121, 25);
            this.PackageToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.PackageToolStripComboBox_SelectedIndexChanged);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // SaveCLToolStripButton
            // 
            this.SaveCLToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SaveCLToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveCLToolStripButton.Image")));
            this.SaveCLToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveCLToolStripButton.Name = "SaveCLToolStripButton";
            this.SaveCLToolStripButton.Size = new System.Drawing.Size(35, 22);
            this.SaveCLToolStripButton.Text = "Save";
            this.SaveCLToolStripButton.ToolTipText = "CTRL+S";
            this.SaveCLToolStripButton.Click += new System.EventHandler(this.SaveCLToolStripButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // PrettyPrintToolStripButton
            // 
            this.PrettyPrintToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.PrettyPrintToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("PrettyPrintToolStripButton.Image")));
            this.PrettyPrintToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PrettyPrintToolStripButton.Name = "PrettyPrintToolStripButton";
            this.PrettyPrintToolStripButton.Size = new System.Drawing.Size(70, 22);
            this.PrettyPrintToolStripButton.Text = "Pretty Print";
            this.PrettyPrintToolStripButton.Click += new System.EventHandler(this.PrettyPrintToolStripButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // OriginalCLToolStripButton
            // 
            this.OriginalCLToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OriginalCLToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("OriginalCLToolStripButton.Image")));
            this.OriginalCLToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OriginalCLToolStripButton.Name = "OriginalCLToolStripButton";
            this.OriginalCLToolStripButton.Size = new System.Drawing.Size(70, 22);
            this.OriginalCLToolStripButton.Text = "Original CL";
            this.OriginalCLToolStripButton.Click += new System.EventHandler(this.OriginalCLToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ModifiedCLToolStripButton
            // 
            this.ModifiedCLToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ModifiedCLToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ModifiedCLToolStripButton.Image")));
            this.ModifiedCLToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ModifiedCLToolStripButton.Name = "ModifiedCLToolStripButton";
            this.ModifiedCLToolStripButton.Size = new System.Drawing.Size(76, 22);
            this.ModifiedCLToolStripButton.Text = "Modified CL";
            this.ModifiedCLToolStripButton.Click += new System.EventHandler(this.ModifiedCLToolStripButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // CommentToolStripButton
            // 
            this.CommentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.CommentToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("CommentToolStripButton.Image")));
            this.CommentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CommentToolStripButton.Name = "CommentToolStripButton";
            this.CommentToolStripButton.Size = new System.Drawing.Size(65, 22);
            this.CommentToolStripButton.Text = "Comment";
            this.CommentToolStripButton.Click += new System.EventHandler(this.CommentToolStripButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // UncommentTolStripButton
            // 
            this.UncommentTolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.UncommentTolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("UncommentTolStripButton.Image")));
            this.UncommentTolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UncommentTolStripButton.Name = "UncommentTolStripButton";
            this.UncommentTolStripButton.Size = new System.Drawing.Size(78, 22);
            this.UncommentTolStripButton.Text = "Uncomment";
            this.UncommentTolStripButton.Click += new System.EventHandler(this.UncommentTolStripButton_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // HighlightToolStripButton
            // 
            this.HighlightToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.HighlightToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("HighlightToolStripButton.Image")));
            this.HighlightToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.HighlightToolStripButton.Name = "HighlightToolStripButton";
            this.HighlightToolStripButton.Size = new System.Drawing.Size(61, 22);
            this.HighlightToolStripButton.Text = "Highlight";
            this.HighlightToolStripButton.Click += new System.EventHandler(this.HighlightoolStripButton_Click);
            // 
            // HighlightToolStripDropDownButton
            // 
            this.HighlightToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.HighlightToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NoneToolStripTextBox,
            this.OrangeToolStripTextBox,
            this.YellowToolStripTextBox,
            this.BlueToolStripTextBox,
            this.GreenToolStripTextBox,
            this.RedToolStripTextBox});
            this.HighlightToolStripDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("HighlightToolStripDropDownButton.Image")));
            this.HighlightToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.HighlightToolStripDropDownButton.Name = "HighlightToolStripDropDownButton";
            this.HighlightToolStripDropDownButton.Size = new System.Drawing.Size(49, 22);
            this.HighlightToolStripDropDownButton.Text = "None";
            // 
            // RedToolStripTextBox
            // 
            this.RedToolStripTextBox.BackColor = System.Drawing.Color.Red;
            this.RedToolStripTextBox.Name = "RedToolStripTextBox";
            this.RedToolStripTextBox.Size = new System.Drawing.Size(180, 22);
            this.RedToolStripTextBox.Text = "Red";
            this.RedToolStripTextBox.Click += new System.EventHandler(this.RedToolStripTextBox_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // FindToolStripButton
            // 
            this.FindToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.FindToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("FindToolStripButton.Image")));
            this.FindToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FindToolStripButton.Name = "FindToolStripButton";
            this.FindToolStripButton.Size = new System.Drawing.Size(34, 22);
            this.FindToolStripButton.Text = "Find";
            this.FindToolStripButton.ToolTipText = "Ctrl+F";
            this.FindToolStripButton.Click += new System.EventHandler(this.FindToolStripButton_Click);
            // 
            // ScintillaTextEditorCL
            // 
            this.ScintillaTextEditorCL.AutoCMaxHeight = 9;
            this.ScintillaTextEditorCL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScintillaTextEditorCL.Location = new System.Drawing.Point(0, 0);
            this.ScintillaTextEditorCL.Name = "ScintillaTextEditorCL";
            this.ScintillaTextEditorCL.ShowLineMargin = true;
            this.ScintillaTextEditorCL.Size = new System.Drawing.Size(942, 309);
            this.ScintillaTextEditorCL.TabIndex = 0;
            this.ScintillaTextEditorCL.Text = "scintillaTextEditor1";
            this.ScintillaTextEditorCL.StyleNeeded += new System.EventHandler<ScintillaNET.StyleNeededEventArgs>(this.ScintillaTextEditorCL_StyleNeeded);
            // 
            // GreenToolStripTextBox
            // 
            this.GreenToolStripTextBox.BackColor = System.Drawing.Color.Lime;
            this.GreenToolStripTextBox.Name = "GreenToolStripTextBox";
            this.GreenToolStripTextBox.Size = new System.Drawing.Size(180, 22);
            this.GreenToolStripTextBox.Text = "Lime";
            this.GreenToolStripTextBox.Click += new System.EventHandler(this.GreenToolStripTextBox_Click);
            // 
            // BlueToolStripTextBox
            // 
            this.BlueToolStripTextBox.BackColor = System.Drawing.Color.Cyan;
            this.BlueToolStripTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.BlueToolStripTextBox.Name = "BlueToolStripTextBox";
            this.BlueToolStripTextBox.Size = new System.Drawing.Size(180, 22);
            this.BlueToolStripTextBox.Text = "Cyan";
            this.BlueToolStripTextBox.Click += new System.EventHandler(this.BlueToolStripTextBox_Click);
            // 
            // YellowToolStripTextBox
            // 
            this.YellowToolStripTextBox.BackColor = System.Drawing.Color.LightYellow;
            this.YellowToolStripTextBox.Name = "YellowToolStripTextBox";
            this.YellowToolStripTextBox.Size = new System.Drawing.Size(180, 22);
            this.YellowToolStripTextBox.Text = "Light Yellow";
            this.YellowToolStripTextBox.Click += new System.EventHandler(this.YellowToolStripTextBox_Click);
            // 
            // OrangeToolStripTextBox
            // 
            this.OrangeToolStripTextBox.BackColor = System.Drawing.Color.DarkOrange;
            this.OrangeToolStripTextBox.Name = "OrangeToolStripTextBox";
            this.OrangeToolStripTextBox.Size = new System.Drawing.Size(180, 22);
            this.OrangeToolStripTextBox.Text = "Dark Orange";
            this.OrangeToolStripTextBox.Click += new System.EventHandler(this.OrangeToolStripTextBox_Click);
            // 
            // NoneToolStripTextBox
            // 
            this.NoneToolStripTextBox.Name = "NoneToolStripTextBox";
            this.NoneToolStripTextBox.Size = new System.Drawing.Size(180, 22);
            this.NoneToolStripTextBox.Text = "None";
            this.NoneToolStripTextBox.Click += new System.EventHandler(this.NoneToolStripTextBox_Click);
            // 
            // CLViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(942, 338);
            this.Controls.Add(this.splitContainer1);
            this.HideOnClose = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "CLViewerForm";
            this.Text = "CL Viewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private ToolStrip toolStrip1;
        private ToolStripButton OriginalCLToolStripButton;
        private ToolStripButton ModifiedCLToolStripButton;
        private ToolStripButton SaveCLToolStripButton;
        private ToolStripComboBox PackageToolStripComboBox;
        private ToolStripButton PrettyPrintToolStripButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton CommentToolStripButton;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton UncommentTolStripButton;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton HighlightToolStripButton;
        private ToolStripDropDownButton HighlightToolStripDropDownButton;
        public ScintillaExt.ScintillaTextEditor ScintillaTextEditorCL;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripButton FindToolStripButton;
        private ToolStripLabel toolStripLabel1;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem RedToolStripTextBox;
        private ToolStripMenuItem NoneToolStripTextBox;
        private ToolStripMenuItem OrangeToolStripTextBox;
        private ToolStripMenuItem YellowToolStripTextBox;
        private ToolStripMenuItem BlueToolStripTextBox;
        private ToolStripMenuItem GreenToolStripTextBox;
    }
}