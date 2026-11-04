using Microsoft.Msagl.Drawing;
using WeifenLuo.WinFormsUI.Docking;
using Honeywell.Database;
using Honeywell.GUI.Mapper.ScintillaExt;
using Honeywell.GUI.Mapper.Properties;
using Honeywell.GUI.Mapper.Forms;
using System.Diagnostics;
using Mapper.Samples;

namespace Honeywell.GUI.Mapper
{
    //todo: hide tabs depending on map or CL selection
    public partial class MainForm : Form
    {
        private TagDgvForm mainTagForm = new();
        public ModuleDgvForm MainModuleForm = new();
        private TagsOnMapDgvForm mainTagsOnMapForm = new();
        public ParameterDgvForm MainParameterForm = new();
        public CLOnTagDgvForm MainClOnTagForm = new();
        private CLAllDgvForm mainClAllForm = new();
        private GraphForm mainGraphForm = new();
        private CrossCommsDgvForm mainCrossCommsForm = new();
        public TagsInCLDgvForm MainTagsOnClForm = new();
        public FileRefsDgvForm MainFileRefsDgvForm = new();
        public TagConnectionsDgvForm MainTagConnectionsDgvForm = new();

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool SaveNodesEnabled { get; set; }

        DbTdcTag? primaryTagName = null;
        DbTdcModule? primaryModuleName = null;
        public MainForm()
        {
            InitializeComponent();

            dockPanel.Theme = new VS2015DarkTheme();
            SetupDockContent();
        }

        private void SetupDockContent()
        {
            

            mainTagForm.Show(dockPanel, DockState.DockLeftAutoHide);
            MainModuleForm.Show(dockPanel, DockState.DockLeftAutoHide);
            mainCrossCommsForm.Show(dockPanel, DockState.DockLeftAutoHide);
            mainClAllForm.Show(dockPanel, DockState.DockLeftAutoHide);

            mainTagsOnMapForm.Show(dockPanel, DockState.DockRightAutoHide);
            MainParameterForm.Show(dockPanel, DockState.DockRightAutoHide);
            MainTagsOnClForm.Show(dockPanel, DockState.DockRightAutoHide);

            
            MainClOnTagForm.Show(dockPanel, DockState.DockBottomAutoHide);
            MainTagConnectionsDgvForm.Show(dockPanel, DockState.DockBottomAutoHide);
            MainFileRefsDgvForm.Show(dockPanel, DockState.DockBottomAutoHide);

            mainGraphForm.Show(dockPanel, DockState.Document);

            mainTagForm.AdvancedDataGridView.CellClick += new DataGridViewCellEventHandler(TagAdvancedDataGridView_CellClick);
            mainGraphForm.gViewer.GraphSavingStarted += new EventHandler(GraphViewer_GraphSavingStarted);
            mainGraphForm.gViewer.Click += new EventHandler(GraphViewer_Click);
            
            mainGraphForm.DirectConnectionsCheckBox.CheckedChanged += DirectConnectionsCheckBox_CheckedChanged;
            mainTagsOnMapForm.AdvancedDataGridView.CellClick += new DataGridViewCellEventHandler(TagsOnMapAdvancedDataGridView_CellClick);
            MainModuleForm.AdvancedDataGridView.CellClick += new DataGridViewCellEventHandler(ModuleAdvancedDataGridView_CellClick);
            mainCrossCommsForm.AdvancedDataGridView.CellClick += new DataGridViewCellEventHandler(CrossCommAdvancedDataGridView_CellClick);
            mainClAllForm.AdvancedDataGridView.CellClick += new DataGridViewCellEventHandler(CLAllAdvancedDataGridView_CellClick);
            MainClOnTagForm.AdvancedDataGridView.CellClick += new DataGridViewCellEventHandler(ClOnTagAdvancedDataGridView_CellClick);
            MainTagsOnClForm.AdvancedDataGridView.CellClick += TagsOnCLAdvancedDataGridView_CellClick;

            using var db = new TdcContext();
            CLLexer.TdcTags = db.TdcTags.Select(x => x.Name).ToList();
            CLFunctions.LoadFunctions();
        }



        private void DirectConnectionsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            mainGraphForm.gViewer.Graph = GetGraphFromTagName();
        }

        private void TagsOnCLAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var tagOnCL = MainTagsOnClForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as TagsInCL;
            if (tagOnCL == null)
                return;

            using var db = new TdcContext();
            var tag = db.TdcTags.FirstOrDefault(x => x.Name == tagOnCL.Name);

            UpdateParameters(tag);

            MainTagsOnClForm.AdvancedDataGridView.Focus();
        }

        private void TagAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var tag = mainTagForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as DbTdcTag;
            if (tag == null || tag.Id == primaryTagName?.Id)
                return;

            primaryTagName = tag;
            primaryModuleName = null;

            mainGraphForm.gViewer.Graph = GetGraphFromTagName();

            UpdateParameters(tag);

            SelectMapOnDock();

            mainTagForm.AdvancedDataGridView.Focus();
        }

        private void UpdateParameters(DbTdcTag? tag)
        {
            if (tag == null)
                return;

            using var db = new TdcContext();
            tag = db.TdcTags.First(x => x.Id == tag.Id);
            var parameters = tag?.Params.OrderBy(x => x.SortName).ToList() ?? new List<DbTdcParameter>();
            ParameterDgvForm.UpdateDataSource(MainParameterForm, parameters, true, true, tag?.Id);
        }

        private void ModuleAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var module = MainModuleForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as DbTdcModule;
            if (module == null)
                return;

            using var db = new TdcContext();
            module = db.TdcModules.First(x => x.Id == module.Id);

            if (!module.Nodes.Any())
            {
                db.TdcModules.Remove(module);
                db.SaveChanges();
                RefreshModules();
                MainModuleForm.AdvancedDataGridView.FirstDisplayedScrollingRowIndex = e.RowIndex;

                return;
            }

            primaryModuleName = module;
            primaryTagName = module.Nodes.FirstOrDefault()?.Tag;
            mainGraphForm.gViewer.Graph = GetGraphFromModuleName();

            SelectMapOnDock();

            MainModuleForm.AdvancedDataGridView.Focus();
        }

        private void CrossCommAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var crossComm = mainCrossCommsForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as CrossComms;
            if (crossComm == null)
                return;

            using var db = new TdcContext();
            primaryTagName = db.TdcTags.FirstOrDefault(x => x.Name == crossComm.SourceTagName);
            primaryModuleName = null;
            mainGraphForm.gViewer.Graph = GetGraphFromTagName();

            SelectMapOnDock();

            mainCrossCommsForm.AdvancedDataGridView.Focus();
        }

        private void GraphViewer_GraphSavingStarted(object? sender, EventArgs e)
        {
            //todo: save graph to db
        }

        private void GraphViewer_Click(object? sender, EventArgs e)
        {
            var node = mainGraphForm.gViewer.SelectedObject as Node;
            if (node != null)
            {
                using var db = new TdcContext();
                var dbNode = db.TdcNodes.FirstOrDefault(x => x.NodeId == node.Id);
                if (dbNode != null)
                    primaryTagName = dbNode.Tag;

                UpdateParameters(dbNode?.Tag);
            }
        }

        public void MaxNodesChanged()
        {
            var currentNodeIdList = mainGraphForm.gViewer.Graph?.Nodes.Select(x => x.Id);
            if (currentNodeIdList != null && currentNodeIdList.Any() && int.TryParse(mainGraphForm.MaxNodes.Text, out int maxNodes))
            {
                Graph? newGraph;
                if (primaryModuleName != null)
                    newGraph = GetGraphFromModuleName();
                else
                    newGraph = GetGraphFromTagName();

                var newNodeIdList = newGraph?.Nodes.Select(x => x.Id);
                if (newNodeIdList != null && (newNodeIdList.Count() != currentNodeIdList.Count() || newNodeIdList.Any(x => !currentNodeIdList.Contains(x))))
                    mainGraphForm.gViewer.Graph = newGraph;

            }

        }

        private void TagsOnMapAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var zoomNode = mainTagsOnMapForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as TagsOnMap;
            if (zoomNode == null)
                return;

            using var db = new TdcContext();
            var dbTag = db.TdcTags.FirstOrDefault(x => x.Name == zoomNode.Name);
            if (dbTag != null)
                primaryTagName = dbTag;

            ZoomToNode(zoomNode.Node.NodeId);

            UpdateParameters(zoomNode.Node.Tag);

            SelectMapOnDock();

            mainTagsOnMapForm.AdvancedDataGridView.Focus();
        }

        private void SelectMapOnDock()
        {
            var doc = dockPanel.Documents.FirstOrDefault() as DockContent;
            if (doc != null)
                doc.Activate();
        }

        private void ClOnTagAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var cl = MainClOnTagForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as DbTdcCL;
            if (cl == null)
                return;

            var doc = dockPanel.Documents.FirstOrDefault(x => (x as DockContent)?.Text == cl.FileName) as DockContent;
            
            if (doc != null)
                doc.Activate();
            else
            {
                var clViewerForm = new CLViewerForm(cl);
                CLViewerForm.MyFindReplace.Scintilla = clViewerForm.ScintillaTextEditorCL;
                clViewerForm.Show(dockPanel, DockState.Document);
            }

            MainClOnTagForm.AdvancedDataGridView.Focus();
        }

        private void CLAllAdvancedDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; //filter clicked

            var cl = mainClAllForm.AdvancedDataGridView.Rows[e.RowIndex].DataBoundItem as CLAll;
            if (cl == null)
                return;

            var doc = dockPanel.Documents.FirstOrDefault(x => (x as DockContent)?.Text == cl.CL.FileName) as DockContent;

            if (doc != null)
                doc.Activate();
            else
            {
                string? packageAttachment = null;
                if (cl.FileName.Contains("Package"))
                    packageAttachment = cl.TagName;

                var clViewerForm = new CLViewerForm(cl.CL, packageAttachment);
                CLViewerForm.MyFindReplace.Scintilla = clViewerForm.ScintillaTextEditorCL;
                clViewerForm.Show(dockPanel, DockState.Document);
            }

            mainClAllForm.AdvancedDataGridView.Focus();
        }

        private void ZoomToNode(string nodeId)
        {
            var centerNode = mainGraphForm.gViewer.Graph.FindNode(nodeId);
            ZoomToNode(centerNode);
        }

        private void ZoomToNode(Node centerNode)
        {
            mainGraphForm.gViewer.ShowBBox(centerNode.BoundingBox);
        }

        private Graph? GetGraphFromTagName()
        {
            if (primaryTagName == null)
                return mainGraphForm.gViewer.Graph;

            Graph? graph;
            if (!mainGraphForm.MaxNodes.Enabled)
                graph = Helper.GetGraph(primaryTagName, true);
            else if (int.TryParse(mainGraphForm.MaxNodes.Text, out var maxNodes))
                graph = Helper.GetGraph(primaryTagName, false, maxNodes);
            else
                graph = Helper.GetGraph(primaryTagName);

            if (graph == null)
                return null;

            var nodeIdOnGraph = graph.Nodes.Select(x => x.Id);

            using var db = new TdcContext();
            primaryTagName = db.TdcTags.First(x => x.Id == primaryTagName.Id);

            var dbnodes = primaryTagName.Nodes.FirstOrDefault()?.Graph?.Nodes?.OrderBy(x => x.NodeId);
            var nodesOnGraph = dbnodes?.Where(x => nodeIdOnGraph.Contains(x.NodeId)).ToList() ?? new List<DbTdcNode>();
            var nodes = TagsOnMap.GetZoomNodes(nodesOnGraph);
            
            if (nodes != null)
                mainTagsOnMapForm.UpdateDataSource(nodes, true);

            return graph;

        }

        public void EnableOrDisableSave()
        {
            Graph? graph = null;
            if (primaryTagName != null)
                graph = Helper.GetGraph(primaryTagName);
            else if (primaryModuleName != null)
                graph = Helper.GetGraph(primaryModuleName);

            if (graph != null && mainGraphForm.gViewer.Graph != null)
            {
                var actualNodeCount = graph.NodeCount;
                var currentNodeCount = mainGraphForm.gViewer.Graph.NodeCount;

                SaveNodesEnabled = (actualNodeCount == currentNodeCount);
            }
        }

        private Graph? GetGraphFromModuleName()
        {
            if (primaryModuleName == null)
                return mainGraphForm.gViewer.Graph;

            Graph? graph;
            if (int.TryParse(mainGraphForm.MaxNodes.Text, out var maxNodes))
                graph = Helper.GetGraph(primaryModuleName, maxNodes);
            else
                graph = Helper.GetGraph(primaryModuleName);

            var nodes = TagsOnMap.GetZoomNodes(primaryModuleName.Nodes.FirstOrDefault()?.Graph?.Nodes?.OrderBy(x => x.NodeId).ToList() ?? new List<DbTdcNode>());
            if (nodes != null)
                mainTagsOnMapForm.UpdateDataSource(nodes, true);

            return graph;
        }

        private void dockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
            var panelName = (dockPanel.ActiveDocument as DockContent)?.Text;

            if (panelName != null && panelName.Contains(".CL"))
            {
             
                var clViewerForm = dockPanel.ActiveContent as CLViewerForm ?? dockPanel.ActiveDocument as CLViewerForm;
                if (clViewerForm != null)
                    CLViewerForm.MyFindReplace.Scintilla = clViewerForm.ScintillaTextEditorCL;

                var tags = TagsInCL.GetCLTags(panelName, clViewerForm?.PackageAttachment);
                MainTagsOnClForm.UpdateDataSource(tags, true);
            }
            else
                MainTagsOnClForm.UpdateDataSource(new List<TagsInCL>(), true);
        }

        private void connectLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

        }

        private void connectServerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ConnectLocalToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog() { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory, Filter = "Mapper DB (*.DB)|*.DB", RestoreDirectory = true };
            if (fd.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.Server = false;
                Settings.Default.ConnectionString = fd.FileName;
                Settings.Default.Save();

                Application.Restart();
                Environment.Exit(0);
            }
        }

        private void ConnectServerToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Settings.Default.Server = true;
            Settings.Default.ConnectionString = null;
            Settings.Default.Save();

            Application.Restart();
            Environment.Exit(0);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Open Excel File";
            fd.Filter = "Excel files|*.xlsx";
            fd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            fd.Multiselect = false;
            fd.RestoreDirectory = true;
            fd.CheckFileExists = true;
            fd.CheckPathExists = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                var filePath = fd.FileName;
                try
                {
                    if (DatabaseWorkbook.Import(filePath))
                    {
                        Helper.RefreshModulesInGraphs();
                        RefreshModules();
                        OfferToRemoveUnusedModules();
                    }
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (IOException)
                {
                    MessageBox.Show($"Close {filePath} and try again");
                }
            }
        }

        private static void OfferToRemoveUnusedModules()
        {
            using var db = new TdcContext();
            var unusedModules = db.TdcModules.Where(x => !x.Nodes.Any()).ToList();
            if (unusedModules.Count == 0)
                return;

            var result = MessageBox.Show("Remove Unused Modules?", "Remove Unused Modules?", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                db.TdcModules.RemoveRange(unusedModules);
                db.SaveChanges();
                RefreshModules();
            }
        }

        public static void RefreshModules()
        {
            using var db = new TdcContext();
            Program.MainForm.MainModuleForm.UpdateDataSource(db.TdcModules.OrderBy(x => x.Name).ToList());
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exportForm = new ExportForm();
            exportForm.ShowDialog();
        }

        private void localDBFromServerDBToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            var t = e.KeyCode;
            var t2 = e.Control;
        }

        private void bulkBuildToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bulkEditToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void mapTDCToExperionParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void templatesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void MapHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var readMe = AppDomain.CurrentDomain.BaseDirectory + "ReadMe.pdf";

            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C start \"\" /max \"{readMe}\""
            };
            Process process = new() { StartInfo = startInfo };
            process.Start();
        }

        private void ClHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cl = SampleCl.GetSampleCL();
            var clViewerForm = new CLViewerForm(cl);
            CLViewerForm.MyFindReplace.Scintilla = clViewerForm.ScintillaTextEditorCL;
            clViewerForm.Show(dockPanel, DockState.Document);
        }
    }
}