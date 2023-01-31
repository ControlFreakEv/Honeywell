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
    //todo: subscribe to sql notifications
    public partial class ModuleDgvForm : DockContent
    {
        List<DbTdcModule> originalDataSource;
        string formText = "Modules";
        public ModuleDgvForm()
        {
            InitializeComponent();
            using var db = new TdcContext();
            originalDataSource = db.TdcModules.OrderBy(x => x.Name).ToList();
            UpdateDataSource(originalDataSource);
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(DgvHelper<DbTdcModule>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(DgvHelper<DbTdcModule>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        public void UpdateDataSource(List<DbTdcModule> datasource)
        {
            AdvancedDataGridView.DataSource = datasource;
            this.Text = $"{formText} ({datasource.Count})";
        }

        private void advancedDataGridViewSearchToolBar1_Search(object sender, Zuby.ADGV.AdvancedDataGridViewSearchToolBarSearchEventArgs e)
        {
            AdvancedDataGridView.Search(e);
        }
    }
}
