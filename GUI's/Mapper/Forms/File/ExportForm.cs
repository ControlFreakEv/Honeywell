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
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Honeywell.GUI.Mapper.Forms
{
    public partial class ExportForm : Form
    {
        public static string ConfigTemplates = "Config Templates";
        public static string ExperionParameters = "Module Parameters";
        public static string Projects = "Projects";
        public static string TdcCl = "CL";
        public static string TdcClRefs = "CL Connections";
        public static string TdcConnections = "Tag Connections";
        public static string TdcFileRef = "File References";
        public static string TdcModules = "Modules";
        public static string TdcNodes = "Nodes in Maps";
        public static string TdcParameters = "TDC Parameters";
        public static string TdcTags = "TDC Tags";

        public ExportForm()
        {
            InitializeComponent();
        }

        private void ExportForm_Load(object sender, EventArgs e)
        {
            //ExportDataGridView.Rows.Add(null, Projects);
           
            ExportDataGridView.Rows.Add(null, TdcModules); 
            //ExportDataGridView.Rows.Add(null, ExperionParameters); 
            //ExportDataGridView.Rows.Add(null, ConfigTemplates); 

            ExportDataGridView.Rows.Add(null, TdcNodes); 
            ExportDataGridView.Rows.Add(null, TdcTags); 
            ExportDataGridView.Rows.Add(null, TdcParameters); 
            ExportDataGridView.Rows.Add(null, TdcConnections); 

            ExportDataGridView.Rows.Add(null, TdcCl); 
            ExportDataGridView.Rows.Add(null, TdcClRefs); 

            ExportDataGridView.Rows.Add(null, TdcFileRef);

            //cb1.ReadOnly= true;
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

                IWorkbook? workbook = null;
                TdcContext? db = null;
                foreach (DataGridViewRow dgvRow in ExportDataGridView.Rows)
                {
                    var selected = dgvRow.Cells[0].Value as bool? ?? false;
                    var table = dgvRow.Cells[1].Value.ToString();
                    if (!selected)
                        continue;

                    if (workbook == null)
                        workbook = new XSSFWorkbook();

                    if (db == null)
                        db = new TdcContext();

                    if (table == Projects)
                        ExcelHelper<DbProject>.ExportSheet(workbook, Projects, db.Projects);
                    else if (table == TdcModules)
                        ExcelHelper<DbTdcModule>.ExportSheet(workbook, TdcModules, db.TdcModules);
                    else if (table == ExperionParameters)
                        ExcelHelper<DbExperionParameter>.ExportSheet(workbook, ExperionParameters, db.ExperionParameters);
                    else if (table == ConfigTemplates)
                        ExcelHelper<DbConfigTemplates>.ExportSheet(workbook, ConfigTemplates, db.ConfigTemplates);
                    else if (table == TdcNodes)
                        ExcelHelper<DbTdcNode>.ExportSheet(workbook, TdcNodes, db.TdcNodes);
                    else if (table == TdcTags)
                        ExcelHelper<DbTdcTag>.ExportSheet(workbook, TdcTags, db.TdcTags);
                    else if (table == TdcParameters)
                        ExcelHelper<DbTdcParameter>.ExportSheet(workbook, TdcParameters, db.TdcParameters);
                    else if (table == TdcConnections)
                        ExcelHelper<DbTdcConnections>.ExportSheet(workbook, TdcConnections, db.ParameterConnections);
                    else if (table == TdcCl)
                        ExcelHelper<DbTdcCL>.ExportSheet(workbook, TdcCl, db.TdcCLs);
                    else if (table == TdcClRefs)
                        ExcelHelper<DbTdcCLRefs>.ExportSheet(workbook, TdcClRefs, db.TdcCLRefs);
                    else if (table == TdcFileRef)
                        ExcelHelper<DbTdcFileRef>.ExportSheet(workbook, TdcFileRef, db.TdcFileRefs);

                }

                //save file
                if (workbook != null)
                {
                    try
                    {
                        using FileStream stream = new(savePath, FileMode.Create, FileAccess.Write);
                        workbook.Write(stream);
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
}
