namespace Honeywell.GUI.Parser
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
            this.EbPathTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ParseEbButton = new System.Windows.Forms.Button();
            this.MapCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CLPathTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.D3KTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SubDirectoryCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.JSPathTextBox = new System.Windows.Forms.TextBox();
            this.ImportD3kFileRefsButton = new System.Windows.Forms.Button();
            this.CdsTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.HgIOWithoutTagsTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.CustomConnectionsTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ClSlotsTextBox = new System.Windows.Forms.TextBox();
            this.ParseGroupButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // EbPathTextBox
            // 
            this.EbPathTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.EbPathTextBox.Location = new System.Drawing.Point(12, 38);
            this.EbPathTextBox.Multiline = true;
            this.EbPathTextBox.Name = "EbPathTextBox";
            this.EbPathTextBox.Size = new System.Drawing.Size(408, 39);
            this.EbPathTextBox.TabIndex = 0;
            this.EbPathTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\EB";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "EB Folder Path";
            // 
            // ParseEbButton
            // 
            this.ParseEbButton.Location = new System.Drawing.Point(460, 80);
            this.ParseEbButton.Name = "ParseEbButton";
            this.ParseEbButton.Size = new System.Drawing.Size(75, 23);
            this.ParseEbButton.TabIndex = 10;
            this.ParseEbButton.Text = "Parse Tags";
            this.ParseEbButton.UseVisualStyleBackColor = true;
            this.ParseEbButton.Click += new System.EventHandler(this.ParseEbButton_Click);
            // 
            // MapCheckBox
            // 
            this.MapCheckBox.AutoSize = true;
            this.MapCheckBox.Checked = true;
            this.MapCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MapCheckBox.Location = new System.Drawing.Point(460, 61);
            this.MapCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MapCheckBox.Name = "MapCheckBox";
            this.MapCheckBox.Size = new System.Drawing.Size(105, 19);
            this.MapCheckBox.TabIndex = 9;
            this.MapCheckBox.Text = "Generate Maps";
            this.MapCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "CL Folder Path";
            // 
            // CLPathTextBox
            // 
            this.CLPathTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.CLPathTextBox.Location = new System.Drawing.Point(12, 98);
            this.CLPathTextBox.Multiline = true;
            this.CLPathTextBox.Name = "CLPathTextBox";
            this.CLPathTextBox.Size = new System.Drawing.Size(408, 39);
            this.CLPathTextBox.TabIndex = 1;
            this.CLPathTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\CL";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(426, 420);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Create EB\'s";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.CreateEbButton1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 403);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "D3K File Path";
            // 
            // D3KTextBox
            // 
            this.D3KTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.D3KTextBox.Location = new System.Drawing.Point(12, 421);
            this.D3KTextBox.Multiline = true;
            this.D3KTextBox.Name = "D3KTextBox";
            this.D3KTextBox.Size = new System.Drawing.Size(408, 39);
            this.D3KTextBox.TabIndex = 5;
            this.D3KTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\D3K\\101.mdb";
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.Location = new System.Drawing.Point(12, 386);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(776, 2);
            this.label4.TabIndex = 9;
            // 
            // SubDirectoryCheckBox
            // 
            this.SubDirectoryCheckBox.AutoSize = true;
            this.SubDirectoryCheckBox.Checked = true;
            this.SubDirectoryCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SubDirectoryCheckBox.Location = new System.Drawing.Point(460, 40);
            this.SubDirectoryCheckBox.Name = "SubDirectoryCheckBox";
            this.SubDirectoryCheckBox.Size = new System.Drawing.Size(148, 19);
            this.SubDirectoryCheckBox.TabIndex = 8;
            this.SubDirectoryCheckBox.Text = "Search Sub Directories?";
            this.SubDirectoryCheckBox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 140);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 15);
            this.label6.TabIndex = 14;
            this.label6.Text = "JS Folder Path";
            // 
            // JSPathTextBox
            // 
            this.JSPathTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.JSPathTextBox.Location = new System.Drawing.Point(12, 158);
            this.JSPathTextBox.Multiline = true;
            this.JSPathTextBox.Name = "JSPathTextBox";
            this.JSPathTextBox.Size = new System.Drawing.Size(408, 39);
            this.JSPathTextBox.TabIndex = 2;
            this.JSPathTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\JS";
            // 
            // ImportD3kFileRefsButton
            // 
            this.ImportD3kFileRefsButton.Enabled = false;
            this.ImportD3kFileRefsButton.Location = new System.Drawing.Point(507, 421);
            this.ImportD3kFileRefsButton.Name = "ImportD3kFileRefsButton";
            this.ImportD3kFileRefsButton.Size = new System.Drawing.Size(145, 23);
            this.ImportD3kFileRefsButton.TabIndex = 12;
            this.ImportD3kFileRefsButton.Text = "Import File Refs into DB";
            this.ImportD3kFileRefsButton.UseVisualStyleBackColor = true;
            this.ImportD3kFileRefsButton.Click += new System.EventHandler(this.ImportD3kFileRefsButton_Click);
            // 
            // CdsTextBox
            // 
            this.CdsTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.CdsTextBox.Location = new System.Drawing.Point(12, 489);
            this.CdsTextBox.Multiline = true;
            this.CdsTextBox.Name = "CdsTextBox";
            this.CdsTextBox.Size = new System.Drawing.Size(408, 39);
            this.CdsTextBox.TabIndex = 6;
            this.CdsTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\CDS.csv";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 471);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 15);
            this.label7.TabIndex = 43;
            this.label7.Text = "CDS File Path";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 540);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(109, 15);
            this.label8.TabIndex = 45;
            this.label8.Text = "HG IO without Tags";
            // 
            // HgIOWithoutTagsTextBox
            // 
            this.HgIOWithoutTagsTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.HgIOWithoutTagsTextBox.Location = new System.Drawing.Point(11, 558);
            this.HgIOWithoutTagsTextBox.Multiline = true;
            this.HgIOWithoutTagsTextBox.Name = "HgIOWithoutTagsTextBox";
            this.HgIOWithoutTagsTextBox.Size = new System.Drawing.Size(408, 39);
            this.HgIOWithoutTagsTextBox.TabIndex = 7;
            this.HgIOWithoutTagsTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\Hidden IO.csv";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 207);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(119, 15);
            this.label9.TabIndex = 47;
            this.label9.Text = "Custom Connections";
            // 
            // CustomConnectionsTextBox
            // 
            this.CustomConnectionsTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.CustomConnectionsTextBox.Location = new System.Drawing.Point(12, 225);
            this.CustomConnectionsTextBox.Multiline = true;
            this.CustomConnectionsTextBox.Name = "CustomConnectionsTextBox";
            this.CustomConnectionsTextBox.Size = new System.Drawing.Size(408, 39);
            this.CustomConnectionsTextBox.TabIndex = 3;
            this.CustomConnectionsTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\Custom Connections.csv";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 274);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 15);
            this.label5.TabIndex = 49;
            this.label5.Text = "CL Slots";
            // 
            // ClSlotsTextBox
            // 
            this.ClSlotsTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClSlotsTextBox.Location = new System.Drawing.Point(11, 292);
            this.ClSlotsTextBox.Multiline = true;
            this.ClSlotsTextBox.Name = "ClSlotsTextBox";
            this.ClSlotsTextBox.Size = new System.Drawing.Size(408, 39);
            this.ClSlotsTextBox.TabIndex = 4;
            this.ClSlotsTextBox.Text = "C:\\Users\\HCA\\Desktop\\Test Files for Parser\\CLSLOTS.ZB";
            // 
            // ParseGroupButton
            // 
            this.ParseGroupButton.Location = new System.Drawing.Point(541, 80);
            this.ParseGroupButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ParseGroupButton.Name = "ParseGroupButton";
            this.ParseGroupButton.Size = new System.Drawing.Size(94, 23);
            this.ParseGroupButton.TabIndex = 50;
            this.ParseGroupButton.Text = "Parse Groups";
            this.ParseGroupButton.UseVisualStyleBackColor = true;
            this.ParseGroupButton.Click += new System.EventHandler(this.ParseGroupButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(800, 602);
            this.Controls.Add(this.ParseGroupButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ClSlotsTextBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.CustomConnectionsTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.HgIOWithoutTagsTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.CdsTextBox);
            this.Controls.Add(this.ImportD3kFileRefsButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.JSPathTextBox);
            this.Controls.Add(this.SubDirectoryCheckBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.D3KTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CLPathTextBox);
            this.Controls.Add(this.MapCheckBox);
            this.Controls.Add(this.ParseEbButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EbPathTextBox);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "TDC Parser";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.Form1_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox EbPathTextBox;
        private Label label1;
        private Button ParseEbButton;
        private CheckBox MapCheckBox;
        private Label label2;
        private TextBox CLPathTextBox;
        private Button button1;
        private Label label3;
        private TextBox D3KTextBox;
        private Label label4;
        private CheckBox SubDirectoryCheckBox;
        private Label label6;
        private TextBox JSPathTextBox;
        private Button ImportD3kFileRefsButton;
        private TextBox CdsTextBox;
        private Label label7;
        private Label label8;
        private TextBox HgIOWithoutTagsTextBox;
        private Label label9;
        private TextBox CustomConnectionsTextBox;
        private Label label5;
        private TextBox ClSlotsTextBox;
        private Button ParseGroupButton;
    }
}