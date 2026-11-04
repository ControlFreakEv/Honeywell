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
    public partial class TagsOnMapDgvForm : DockContent
    {
        List<TagsOnMap>? originalDataSource;
        string formText = "Tags on Map";
        public TagsOnMapDgvForm()
        {
            InitializeComponent();
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            ShowRows(DgvHelper<TagsOnMap>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            ShowRows(DgvHelper<TagsOnMap>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        private void ShowRows(List<TagsOnMap> rows)
        {
            AdvancedDataGridView.DataSource = rows;
            this.Text = $"{formText} ({rows.Count})";
        }

        public void UpdateDataSource(List<TagsOnMap> datasource, bool reapplySortAndFilter = false)
        {
            originalDataSource = datasource;
            ShowRows(datasource);

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
