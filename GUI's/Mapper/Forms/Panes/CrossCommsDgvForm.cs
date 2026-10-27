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
    public partial class CrossCommsDgvForm : DockContent
    {
        List<CrossComms> originalDataSource = CrossComms.GetCrossComms();
        string formText = "Cross Communications";
        public CrossCommsDgvForm()
        {
            InitializeComponent();

            UpdateDataSource(originalDataSource);
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void CrossCommsAdvancedDataGridView_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(DgvHelper<CrossComms>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        private void CrossCommsAdvancedDataGridView_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(DgvHelper<CrossComms>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        public void UpdateDataSource(List<CrossComms> datasource)
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
