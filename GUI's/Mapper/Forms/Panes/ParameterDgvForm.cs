using Honeywell.Database;
using Mapper.Samples;
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
    public partial class ParameterDgvForm : DockContent
    {
        List<DbTdcParameter> originalDataSource;
        static string formText = "TDC Parameters";
        public ParameterDgvForm()
        {
            InitializeComponent();
            advancedDataGridViewSearchToolBar1.SetColumns(ParameterAdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            UpdateDataSource(this, DgvHelper<DbTdcParameter>.GetSortedDataSource(e, ParameterAdvancedDataGridView.DataSource), false);
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            UpdateDataSource(this, DgvHelper<DbTdcParameter>.GetFilteredDataSource(e, originalDataSource, ParameterAdvancedDataGridView.DataSource), false);
        }

        public static void UpdateDataSource(ParameterDgvForm form, List<DbTdcParameter> datasource, bool updateOriginalDataSource, bool reapplySortAndFilter = false)
        {
            if (updateOriginalDataSource)
                form.originalDataSource = datasource.ToList();
            
            using var db = new TdcContext();
            var tagId = datasource.FirstOrDefault()?.TagId;

            var tag = db.TdcTags.FirstOrDefault(x => x.Id == tagId);
            if (tag != null && datasource.FirstOrDefault(x => x.Name == nameof(tag.PointType)) == null)
            {
                datasource.Insert(0, new DbTdcParameter() { Name = "Network", Value = Database.Helper.GetLcnAddress(tag), Tag = tag, TagId = tag.Id });
                datasource.Insert(0, new DbTdcParameter() { Name = nameof(tag.PointType), Value = tag.PointType, Tag = tag, TagId = tag.Id });
            }

            if (tagId < 0)//sample tag
                tag = SampleCl.SampleTags.FirstOrDefault(x => x.Id == tagId);

            form.ParameterAdvancedDataGridView.DataSource = datasource;
            form.Text = $"{formText} ({datasource.Count})";
            var tagname = tag.Name;
            form.nameDataGridViewTextBoxColumn.HeaderText = $"Name ({tagname})";

            if (reapplySortAndFilter)
            {
                form.ParameterAdvancedDataGridView.CleanFilter();
                form.ParameterAdvancedDataGridView.TriggerSortStringChanged();
            }

            #region Update file refs and CL on tag
            if (tag != null)
            {
                Program.MainForm.MainFileRefsDgvForm.UpdateDataSource(tag.TdcFileRefs.ToList(), true);

                var clrefs = db.TdcCLRefs.Where(x => x.TagId == tagId);
                var cl = clrefs.Select(x => x.CL).OrderBy(x => x.FileName).ToList() ?? new List<DbTdcCL>();
                CLOnTagDgvForm.UpdateDataSource(Program.MainForm.MainClOnTagForm, tag?.Name, cl, true);

                var tagConnections = TagConnections.GetConnections(tag.Nodes.ToList());
                Program.MainForm.MainTagConnectionsDgvForm.UpdateDataSource(tagConnections, true);
            }
            #endregion
        }

        private void advancedDataGridViewSearchToolBar1_Search(object sender, Zuby.ADGV.AdvancedDataGridViewSearchToolBarSearchEventArgs e)
        {
            ParameterAdvancedDataGridView.Search(e);
        }
    }
}
