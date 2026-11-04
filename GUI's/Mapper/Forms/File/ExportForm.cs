using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Honeywell.Database;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Honeywell.GUI.Mapper.Forms
{
    public partial class ExportForm : Form
    {
        public ExportForm()
        {
            InitializeComponent();
        }

        private void ExportForm_Load(object sender, EventArgs e)
        {
           
            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcModules); 

            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcNodes); 
            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcTags); 
            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcParameters); 
            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcConnections); 

            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcCl); 
            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcClRefs); 

            ExportDataGridView.Rows.Add(null!, DatabaseWorkbook.TdcFileRef);

        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sd = new();
            sd.Title = "Save Excel File";
            sd.Filter = "Excel files|*.xlsx";
            sd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            sd.RestoreDirectory = true;
            sd.DefaultExt = "xlsx";
            //sd.CheckFileExists = true;
            sd.CheckPathExists = true;

            if (sd.ShowDialog() == DialogResult.OK)
            {
                var savePath = sd.FileName;

                var sheetNames = ExportDataGridView.Rows.Cast<DataGridViewRow>()
                    .Where(row => row.Cells[0].Value as bool? ?? false)
                    .Select(row => row.Cells[1].Value?.ToString())
                    .OfType<string>()
                    .ToList();
                if (sheetNames.Count == 0)
                    return;

                try
                {
                    DatabaseWorkbook.Export(sheetNames, savePath);
                    MessageBox.Show($"File saved at {savePath}");
                }
                catch (IOException)
                {
                    MessageBox.Show($"Close {savePath} and try again");
                }
            }
        }
    }
}
