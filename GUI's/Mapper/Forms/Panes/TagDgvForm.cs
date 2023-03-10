using Honeywell.Database;
using Microsoft.Msagl.Drawing;
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
    public partial class TagDgvForm : DockContent
    {
        List<DbTdcTag> originalDataSource;
        string formText = "TDC Tags";
        public TagDgvForm()
        {
            InitializeComponent();
            using var db = new TdcContext();
            originalDataSource = db.TdcTags.OrderBy(x => x.Name).ToList();
            UpdateDataSource(originalDataSource);
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
            AdvancedDataGridView.SetFilterChecklistEnabled(nameDataGridViewTextBoxColumn, false);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(DgvHelper<DbTdcTag>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(DgvHelper<DbTdcTag>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        private void UpdateDataSource(List<DbTdcTag> datasource)
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
