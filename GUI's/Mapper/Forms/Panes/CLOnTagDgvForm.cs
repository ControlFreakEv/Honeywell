using Honeywell.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Honeywell.GUI.Mapper
{
    public partial class CLOnTagDgvForm : DockContent
    {
        List<DbTdcCL> originalDataSource;
        static string formText = "CL On Tag";
        public CLOnTagDgvForm()
        {
            InitializeComponent();
            using var db = new TdcContext();
            originalDataSource = db.TdcCLs.OrderBy(x => x.FileName).ToList();
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(this, DgvHelper<DbTdcCL>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(this, DgvHelper<DbTdcCL>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        public static void UpdateDataSource(CLOnTagDgvForm form, List<DbTdcCL> datasource, bool reapplySortAndFilter = false)
        {
            form.AdvancedDataGridView.DataSource = datasource;
            form.Text = $"{formText} ({datasource.Count})";

            if (reapplySortAndFilter)
            {
                form.AdvancedDataGridView.CleanFilter();
                form.AdvancedDataGridView.TriggerSortStringChanged();
            }
        }

        public static void UpdateDataSource(CLOnTagDgvForm form, string? tagname, List<DbTdcCL> datasource, bool reapplySortAndFilter = false)
        {
            UpdateDataSource(form, datasource);

            if (tagname != null)
                form.fileNameDataGridViewTextBoxColumn.HeaderText = $"Name ({tagname})";
            else
                form.fileNameDataGridViewTextBoxColumn.HeaderText = "Name";
        }

        private void advancedDataGridViewSearchToolBar1_Search(object sender, Zuby.ADGV.AdvancedDataGridViewSearchToolBarSearchEventArgs e)
        {
            AdvancedDataGridView.Search(e);
        }
    }
}
