using WeifenLuo.WinFormsUI.Docking;

namespace Honeywell.GUI.Mapper
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConnectLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConnectServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.localDBFromServerDBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.MapHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.bulkBuildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bulkEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.templatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapTDCToExperionParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockPanel
            // 
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.DockBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dockPanel.Location = new System.Drawing.Point(0, 0);
            this.dockPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.ShowAutoHideContentOnHover = false;
            this.dockPanel.Size = new System.Drawing.Size(820, 535);
            this.dockPanel.TabIndex = 1;
            this.dockPanel.ActiveDocumentChanged += new System.EventHandler(this.dockPanel_ActiveDocumentChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton3,
            this.toolStripDropDownButton2,
            this.ToolsDropDownButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(820, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.createToolStripMenuItem,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(38, 22);
            this.toolStripDropDownButton1.Text = "File";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConnectLocalToolStripMenuItem,
            this.ConnectServerToolStripMenuItem});
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            // 
            // ConnectLocalToolStripMenuItem
            // 
            this.ConnectLocalToolStripMenuItem.Name = "ConnectLocalToolStripMenuItem";
            this.ConnectLocalToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.ConnectLocalToolStripMenuItem.Text = "Connect (Local)";
            this.ConnectLocalToolStripMenuItem.Click += new System.EventHandler(this.ConnectLocalToolStripMenuItem_Click_1);
            // 
            // ConnectServerToolStripMenuItem
            // 
            this.ConnectServerToolStripMenuItem.Enabled = false;
            this.ConnectServerToolStripMenuItem.Name = "ConnectServerToolStripMenuItem";
            this.ConnectServerToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.ConnectServerToolStripMenuItem.Text = "Connect (Server)";
            this.ConnectServerToolStripMenuItem.Click += new System.EventHandler(this.ConnectServerToolStripMenuItem_Click_1);
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.localDBFromServerDBToolStripMenuItem});
            this.createToolStripMenuItem.Enabled = false;
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            this.createToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.createToolStripMenuItem.Text = "Create";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItem2.Text = "Server DB from Local DB";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // localDBFromServerDBToolStripMenuItem
            // 
            this.localDBFromServerDBToolStripMenuItem.Name = "localDBFromServerDBToolStripMenuItem";
            this.localDBFromServerDBToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.localDBFromServerDBToolStripMenuItem.Text = "Local DB from Server DB";
            this.localDBFromServerDBToolStripMenuItem.Click += new System.EventHandler(this.localDBFromServerDBToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripDropDownButton3
            // 
            this.toolStripDropDownButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton3.Enabled = false;
            this.toolStripDropDownButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton3.Image")));
            this.toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            this.toolStripDropDownButton3.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton3.Text = "View";
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MapHelpToolStripMenuItem,
            this.ClHelpToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton2.Text = "Help";
            // 
            // MapHelpToolStripMenuItem
            // 
            this.MapHelpToolStripMenuItem.Name = "MapHelpToolStripMenuItem";
            this.MapHelpToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.MapHelpToolStripMenuItem.Text = "Map";
            this.MapHelpToolStripMenuItem.Click += new System.EventHandler(this.MapHelpToolStripMenuItem_Click);
            // 
            // ClHelpToolStripMenuItem
            // 
            this.ClHelpToolStripMenuItem.Name = "ClHelpToolStripMenuItem";
            this.ClHelpToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ClHelpToolStripMenuItem.Text = "CL";
            this.ClHelpToolStripMenuItem.Click += new System.EventHandler(this.ClHelpToolStripMenuItem_Click);
            // 
            // ToolsDropDownButton
            // 
            this.ToolsDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolsDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bulkBuildToolStripMenuItem,
            this.bulkEditToolStripMenuItem,
            this.templatesToolStripMenuItem,
            this.mapTDCToExperionParametersToolStripMenuItem});
            this.ToolsDropDownButton.Enabled = false;
            this.ToolsDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolsDropDownButton.Image")));
            this.ToolsDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolsDropDownButton.Name = "ToolsDropDownButton";
            this.ToolsDropDownButton.Size = new System.Drawing.Size(47, 22);
            this.ToolsDropDownButton.Text = "Tools";
            // 
            // bulkBuildToolStripMenuItem
            // 
            this.bulkBuildToolStripMenuItem.Name = "bulkBuildToolStripMenuItem";
            this.bulkBuildToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.bulkBuildToolStripMenuItem.Text = "Bulk Build";
            this.bulkBuildToolStripMenuItem.Click += new System.EventHandler(this.bulkBuildToolStripMenuItem_Click);
            // 
            // bulkEditToolStripMenuItem
            // 
            this.bulkEditToolStripMenuItem.Name = "bulkEditToolStripMenuItem";
            this.bulkEditToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.bulkEditToolStripMenuItem.Text = "Bulk Edit";
            this.bulkEditToolStripMenuItem.Click += new System.EventHandler(this.bulkEditToolStripMenuItem_Click);
            // 
            // templatesToolStripMenuItem
            // 
            this.templatesToolStripMenuItem.Name = "templatesToolStripMenuItem";
            this.templatesToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.templatesToolStripMenuItem.Text = "Templates";
            this.templatesToolStripMenuItem.Click += new System.EventHandler(this.templatesToolStripMenuItem_Click);
            // 
            // mapTDCToExperionParametersToolStripMenuItem
            // 
            this.mapTDCToExperionParametersToolStripMenuItem.Name = "mapTDCToExperionParametersToolStripMenuItem";
            this.mapTDCToExperionParametersToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.mapTDCToExperionParametersToolStripMenuItem.Text = "Map TDC to Experion Parameters";
            this.mapTDCToExperionParametersToolStripMenuItem.Click += new System.EventHandler(this.mapTDCToExperionParametersToolStripMenuItem_Click);
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
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dockPanel);
            this.splitContainer1.Size = new System.Drawing.Size(820, 564);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 564);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "Mapper";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private ToolStrip toolStrip1;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem connectLocalToolStripMenuItem;
        private ToolStripMenuItem connectServerToolStripMenuItem;
        private SplitContainer splitContainer1;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem MapHelpToolStripMenuItem;
        private ToolStripMenuItem ConnectLocalToolStripMenuItem;
        private ToolStripMenuItem ConnectServerToolStripMenuItem;
        private ToolStripMenuItem _connectLocalToolStripMenuItem;
        private ToolStripMenuItem _connectServerToolStripMenuItem;
        private ToolStripMenuItem importToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripMenuItem ClHelpToolStripMenuItem;
        private ToolStripMenuItem createToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem localDBFromServerDBToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton3;
        private ToolStripDropDownButton ToolsDropDownButton;
        private ToolStripMenuItem bulkBuildToolStripMenuItem;
        private ToolStripMenuItem bulkEditToolStripMenuItem;
        private ToolStripMenuItem templatesToolStripMenuItem;
        private ToolStripMenuItem mapTDCToExperionParametersToolStripMenuItem;
    }
}