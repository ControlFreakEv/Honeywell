using Honeywell.Database;
using Honeywell.Parsers.TDC;
using Parser;
using System.Diagnostics;

namespace Honeywell.GUI.Parser
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ParseEbButton_Click(object sender, EventArgs e)
        {
            #region EB
            var ebPath = EbPathTextBox.Text;
            if (!Directory.Exists(ebPath))
            {
                MessageBox.Show("Invalid EB File Path");
                return;
            }
            #endregion

            #region CL
            var clPath = CLPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(clPath))
                clPath = null;

            if (clPath != null && !Directory.Exists(clPath))
            {
                MessageBox.Show("Invalid CL File Path");
                return;
            }
            #endregion

            #region JS
            var jsPath = JSPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(jsPath))
                jsPath = null;

            if (jsPath != null && !Directory.Exists(jsPath))
            {
                MessageBox.Show("Invalid JS File Path");
                return;
            }
            #endregion

            #region Custom Connections
            var customConnectionPath = CustomConnectionsTextBox.Text;
            if (string.IsNullOrWhiteSpace(customConnectionPath))
                customConnectionPath = null;

            if (customConnectionPath != null && !File.Exists(customConnectionPath))
            {
                MessageBox.Show("Invalid Custom Connection File Path");
                return;
            }
            #endregion

            #region CL Slots
            var clSlotsConnectionPath = ClSlotsTextBox.Text;
            if (string.IsNullOrWhiteSpace(clSlotsConnectionPath))
                clSlotsConnectionPath = null;

            if (clSlotsConnectionPath != null && !File.Exists(clSlotsConnectionPath))
            {
                MessageBox.Show("Invalid CLSLOTS.ZB File Path");
                return;
            }
            #endregion

            if (MapCheckBox.Checked)
            {
                var searchOption = SubDirectoryCheckBox.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                Parsers.TDC.Parser.ParseTdcFiles(ebPath, clPath, jsPath, customConnectionPath, clSlotsConnectionPath, MapCheckBox.Checked, searchOption);
                ImportFileRefs();
                MessageBox.Show($"Maps generated at \n{TdcContext.ParserConnectionString}");
            }
            else
            {
                Parsers.TDC.Parser.ExportModules(ebPath);
                MessageBox.Show($"Parsed EB files saved at \n{Parsers.TDC.Parser.ModulesFilePath}");
            }
    
        }

        private void CreateEbButton1_Click(object sender, EventArgs e)
        {
            Parsers.TDC.Parser.CreateEBsFromD3K(D3KTextBox.Text, CdsTextBox.Text, HgIOWithoutTagsTextBox.Text);
            MessageBox.Show($"EB file saved at \n{Parsers.TDC.Parser.D3kEbFilePath}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var procmodFile = "LY9185P.CL";

            //Parser.ParseTdc(@"C:\Users\ebmiller\OneDrive - Hargrove Engineers + Constructors\Current Projects\OxyChem F550 & 570\EB", @"C:\Users\ebmiller\OneDrive - Hargrove Engineers + Constructors\Current Projects\OxyChem F550 & 570\CL", false);

            var cl = Parsers.TDC.Parser.TdcCL.First(x => x.FileName == procmodFile);

        }

        private void ImportD3kFileRefsButton_Click(object sender, EventArgs e)
        {
            ImportFileRefs();
        }

        private void ImportFileRefs()
        {
            var fileRefsConnectionPath = D3KTextBox.Text;
            if (string.IsNullOrWhiteSpace(fileRefsConnectionPath))
                fileRefsConnectionPath = null;

            if (fileRefsConnectionPath != null && !File.Exists(fileRefsConnectionPath))
            {
                MessageBox.Show("Invalid Custom Connection File Path");
                return;
            }
            else if (fileRefsConnectionPath != null)
                Parsers.TDC.Parser.ImportFileRefsFromD3k(fileRefsConnectionPath);
        }

        private void ParseGroupButton_Click(object sender, EventArgs e)
        {
            var searchOption = SubDirectoryCheckBox.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            Parsers.TDC.Parser.ExportGroups(EbPathTextBox.Text, searchOption);
            MessageBox.Show($"Parsed Groups saved at \n{Parsers.TDC.Parser.GroupFilePath}");
        }

        private void Form1_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var help = new HelpForm();
            help.ShowDialog();
        }
    }
}