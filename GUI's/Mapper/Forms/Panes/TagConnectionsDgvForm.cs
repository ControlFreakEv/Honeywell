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
    public partial class TagConnectionsDgvForm : DockContent
    {
        List<TagConnections> originalDataSource;
        string formText = "Connections";
        public TagConnectionsDgvForm()
        {
            InitializeComponent();
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(DgvHelper<TagConnections>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(DgvHelper<TagConnections>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        public void UpdateDataSource(List<TagConnections> datasource, bool reapplySortAndFilter = false)
        {
            AdvancedDataGridView.DataSource = datasource;
            this.Text = $"{formText} ({datasource.Count})";

            if (reapplySortAndFilter)
            {
                AdvancedDataGridView.CleanFilter();
                AdvancedDataGridView.TriggerSortStringChanged();
            }
        }

        private void advancedDataGridViewSearchToolBar1_Search(object sender, Zuby.ADGV.AdvancedDataGridViewSearchToolBarSearchEventArgs e)
        {
            AdvancedDataGridView.Search(e);
        }
    }
}
