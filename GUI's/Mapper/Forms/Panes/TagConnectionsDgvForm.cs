using WeifenLuo.WinFormsUI.Docking;

namespace Honeywell.GUI.Mapper
{
    public partial class TagConnectionsDgvForm : DockContent
    {
        List<TagConnections>? originalDataSource;
        string formText = "Connections";
        public TagConnectionsDgvForm()
        {
            InitializeComponent();
            advancedDataGridViewSearchToolBar1.SetColumns(AdvancedDataGridView.Columns);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            ShowRows(DgvHelper<TagConnections>.GetSortedDataSource(e, AdvancedDataGridView.DataSource));
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            ShowRows(DgvHelper<TagConnections>.GetFilteredDataSource(e, originalDataSource, AdvancedDataGridView.DataSource));
        }

        private void ShowRows(List<TagConnections> rows)
        {
            AdvancedDataGridView.DataSource = rows;
            this.Text = $"{formText} ({rows.Count})";
        }

        public void UpdateDataSource(List<TagConnections> datasource, bool reapplySortAndFilter = false)
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
