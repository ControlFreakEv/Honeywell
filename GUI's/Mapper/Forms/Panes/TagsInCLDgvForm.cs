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
using Zuby.ADGV;

namespace Honeywell.GUI.Mapper
{
    public partial class TagsInCLDgvForm : DockContent
    {
        List<TagsInCL> originalDataSource;
        string formText = "Tags in CL";
        public TagsInCLDgvForm()
        {
            InitializeComponent();
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(DgvHelper<TagsInCL>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(DgvHelper<TagsInCL>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        public void UpdateDataSource(List<TagsInCL> datasource, bool reapplySortAndFilter = false)
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
