using Honeywell.Database;
using Microsoft.Msagl.Core.Geometry.Curves;
//using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace Honeywell.GUI.Mapper
{
    public partial class GraphForm : DockContent
    {
        readonly ToolTip selectedObjectToolTip = new();
        public ToolStripTextBox MaxNodes = new() { Text = "10", ToolTipText = "Hidden nodes have red border" };
        public CheckBox DirectConnectionsCheckBox;
        ToolStripButton saveButton;
        object? selectedObject;
        //AttributeBase? selectedObjectAttr;
        readonly Dictionary<object, Microsoft.Msagl.Drawing.Color> draggedObjectOriginalColors = new();
        bool isZoomRectangle = false;
        bool isMouseWheelPan = false;
        bool isMouseScreenDrag = false;
        Rectangle screenDragRectangle = new Rectangle(new Point(0, 0), new Size(0, 0));
        Point startScreenPoint;
        System.Drawing.Point mouseDownPoint;
        PlaneTransformation mouseDownTransform;
        List<Node> dragNodes = new();
        List<GraphModuleUpdates> moduleUpdates = new();
        string? graphId = null;
        public GraphForm()
        {
            InitializeComponent();
            var toolbar = (ToolStrip)gViewer.Controls["toolbar"];

            //GraphViewer.AsyncLayout = true;
            gViewer.MouseWheel += GraphViewer_MouseWheel;
            gViewer.LayoutEditor.DecorateObjectForDragging = SetDragDecorator;
            gViewer.LayoutEditor.RemoveObjDraggingDecorations = RemoveDragDecorator;

            gViewer.SaveButtonVisible = true;
            gViewer.SaveAsImageEnabled = false;
            gViewer.SaveAsMsaglEnabled = false;
            gViewer.SaveInVectorFormatEnabled = false;
            gViewer.SaveGraphButtonVisible = true;

            gViewer.EdgeInsertButtonVisible = true;

            selectedObjectToolTip.Active = true;
            selectedObjectToolTip.AutoPopDelay = 5000;
            selectedObjectToolTip.InitialDelay = 1000;
            selectedObjectToolTip.ReshowDelay = 500;

            #region toolbar
            saveButton = (ToolStripButton)toolbar.Items["saveButton"];
            saveButton.ToolTipText = "Ctrl+S";
            saveButton.Click += SaveButton_Click;

            ((ToolStripButton)toolbar.Items["zoomin"]).ToolTipText = "Scroll Wheel Up";

            ((ToolStripButton)toolbar.Items["zoomout"]).ToolTipText = "Scroll Wheel Down";

            ((ToolStripButton)toolbar.Items["panButton"]).ToolTipText = "Scroll Wheel Click";

            toolbar.Items.RemoveByKey("openButton");

            var redrawNodesBtn = new ToolStripButton("Redraw Nodes");
            redrawNodesBtn.Click += RedrawNodesBtn_Click;
            toolbar.Items.Add(redrawNodesBtn);

            toolbar.Items.Add(new ToolStripSeparator());
            var redrawLinesBtn = new ToolStripButton("Redraw Lines");
            redrawLinesBtn.Click += RedrawLinesBtn_Click; ;
            toolbar.Items.Add(redrawLinesBtn);

            toolbar.Items.Add(new ToolStripSeparator());
            var groupBtn = new ToolStripButton("Group") { ToolTipText = "Ctrl+G" };
            groupBtn.Click += GroupBtn_Click;
            toolbar.Items.Add(groupBtn);

            toolbar.Items.Add(new ToolStripSeparator());
            var unGroupBtn = new ToolStripButton("Ungroup") { ToolTipText = "Ctrl+U" };
            unGroupBtn.Click += UnGroupBtn_Click; ;
            toolbar.Items.Add(unGroupBtn);

            var tsLabel = new ToolStripLabel("Max Nodes") { ToolTipText = "Hidden nodes have red border" };
            toolbar.Items.Add(new ToolStripSeparator());
            toolbar.Items.Add(tsLabel);

            MaxNodes.LostFocus += MaxNodes_LostFocus; ; 
            MaxNodes.KeyPress += MaxNodes_KeyPress;
            toolbar.Items.Add(MaxNodes);

            toolbar.Items.Add(new ToolStripSeparator());
            DirectConnectionsCheckBox = new()
            {
                BackColor = tsLabel.BackColor,
                ForeColor = tsLabel.ForeColor,
                Text = "Direct Connections Only"
            };

            DirectConnectionsCheckBox.CheckedChanged += Cb_CheckedChanged;
            ToolStripControlHost host = new(DirectConnectionsCheckBox);
            toolbar.Items.Insert(toolbar.Items.Count, host);
            new ToolTip() { AutoPopDelay = 5000, InitialDelay = 5000, ReshowDelay = 1000, ShowAlways = true}.SetToolTip(DirectConnectionsCheckBox, "Hidden nodes have red border");
            #endregion

            //todo: only apply context menu to nodes/subgraphs.. can't tweak edges now
            #region Context Menu
            this.ContextMenuStrip = new();

            var unGroupContextMenu = new ToolStripMenuItem() { Text = "UnGroup | Ctrl+U"};
            unGroupContextMenu.Click += (sender, e) => { UnGroupNodes(); };
            this.ContextMenuStrip.Items.Add(unGroupContextMenu);

            var groupContextMenu = new ToolStripMenuItem() { Text = "Group | Ctrl+G" };
            groupContextMenu.Click += (sender, e) => { GroupNodes(); };
            this.ContextMenuStrip.Items.Add(groupContextMenu);

            var hideContextMenu = new ToolStripMenuItem() { Text = "Hide", Enabled = false }; //todo: add this
            hideContextMenu.Click += (sender, e) => {  };
            this.ContextMenuStrip.Items.Add(hideContextMenu);

            var unHideContextMenu = new ToolStripMenuItem() { Text = "UnHide", Enabled = false }; //todo: add this
            unHideContextMenu.Click += (sender, e) => { };
            this.ContextMenuStrip.Items.Add(unHideContextMenu);
            #endregion
        }

        private void MaxNodes_LostFocus(object? sender, EventArgs e)
        {
            DirectConnectionsCheckBox.Select();
            Program.MainForm.MaxNodesChanged();
        }

        private void MaxNodes_KeyPress(object? sender, KeyPressEventArgs e)
        {
            var key = (int)(e.KeyChar);
            if (key == (int)Keys.Enter)
            {
                DirectConnectionsCheckBox.Select();
                e.Handled = true; //suppresses beep
            }
        }

        private void Cb_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            var cb = (CheckBox)sender;
            if (cb.Checked)
                MaxNodes.Enabled = false;
            else
                MaxNodes.Enabled = true;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            using var db = new TdcContext();

            if (moduleUpdates.Any())
            {
                foreach (var moduleUpdate in moduleUpdates)
                {
                    var node = moduleUpdate.NodeInGraph;
                    var moduleName = moduleUpdate.ModuleName;

                    var dbNode = db.TdcNodes.FirstOrDefault(x => x.NodeId == node.Id);
                    if (dbNode == null)
                        continue;

                    if (moduleName == null)
                        dbNode.ModuleId = null;
                    else
                    {
                        var dbModule = db.TdcModules.FirstOrDefault(x => x.Name == moduleName);
                        if (dbModule == null) //add module
                        {
                            dbModule = new()
                            {
                                Id = db.TdcModules.Max(x => x.Id) + 1,
                                Name = moduleName
                            };
                            db.TdcModules.Add(dbModule);
                        }

                        dbNode.ModuleId = dbModule.Id;
                    }
                    db.SaveChanges();
                }

                var removeModules = db.TdcModules.Where(x => !x.Nodes.Any());
                if (removeModules.Any())
                    db.TdcModules.RemoveRange(removeModules);

                db.SaveChanges();
                moduleUpdates.Clear();
                MainForm.RefreshModules();
                saveButton.Text = "";
            }

            if (gViewer.Graph != null & Program.MainForm.SaveNodesEnabled)
            {
                var xml = Database.Helper.WriteGraphToXML(gViewer.Graph);
                var id = int.Parse(gViewer.Graph.UserData.ToString());

                var dbGraph = db.TdcGraphs.First(x => x.Id == id);
                dbGraph.Xml = xml;
                db.SaveChanges();

                gViewer.Graph = gViewer.Graph;
            }
        }



        private void GraphViewer_GraphChanged(object sender, EventArgs e)
        {
            draggedObjectOriginalColors.Clear();
            selectedObject = null;

            Program.MainForm.EnableOrDisableSave();

            var userdata = gViewer?.Graph?.UserData?.ToString();
            if (userdata != graphId)
            {
                moduleUpdates.Clear();
                saveButton.Text = "";
                graphId = userdata;
            }                
        }

        void SetDragDecorator(IViewerObject obj)
        {
            var dNode = obj as DNode;
            if (dNode != null)
            {
                
                if (dNode.Node.UserData != null && dNode.Node.UserData.ToString() == "Hidden Connections")
                    draggedObjectOriginalColors[dNode] = Microsoft.Msagl.Drawing.Color.Red;
                else
                    draggedObjectOriginalColors[dNode] = Microsoft.Msagl.Drawing.Color.Black;

                dNode.DrawingNode.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;
                gViewer.Invalidate(obj);
            }
        }

        void RemoveDragDecorator(IViewerObject obj)
        {
            var dNode = obj as DNode;
            if (dNode != null && draggedObjectOriginalColors.ContainsKey(obj))
            {
                dNode.DrawingNode.Attr.Color = draggedObjectOriginalColors[dNode];
                draggedObjectOriginalColors.Remove(obj);
                gViewer.Invalidate(obj);
            }
        }

        private void GraphViewer_MouseWheel(object? sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            if (delta != 0)
                gViewer.ZoomF *= delta < 0 ? 0.9 : 1.1;
        }

        private void RedrawLinesBtn_Click(object? sender, EventArgs e)
        {
            var edgeList = gViewer.Graph?.Edges.ToList();

            if (edgeList == null)
                return;

            for (int i = edgeList.Count - 1; i >= 0; i--)
                gViewer.Graph.RemoveEdge(edgeList[i]);

            for (int i = 0; i < edgeList.Count; i++)
                gViewer.Graph.AddEdge(edgeList[i].Source, edgeList[i].LabelText, edgeList[i].Target).Attr = edgeList[i].Attr;
        }

        private void RedrawNodesBtn_Click(object? sender, EventArgs e)
        {
            gViewer.NeedToCalculateLayout = true;
            gViewer.Graph = gViewer.Graph;
            gViewer.NeedToCalculateLayout = false;
            //GraphViewer.AsyncLayout = false; //todo: set this back to true?
            //GraphViewer.Graph = GraphViewer.Graph;
        }

        private void UnGroupBtn_Click(object? sender, EventArgs e)
        {
            UnGroupNodes();
        }

        private void GroupBtn_Click(object? sender, EventArgs e)
        {
            GroupNodes();
        }

        private void GraphForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void GraphViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.G)
                GroupNodes();
            else if (e.Control && e.KeyCode == Keys.U)
                UnGroupNodes();
        }

        private void UpdateModuleList(Node node, string? moduleName)
        {
            var update = moduleUpdates.FirstOrDefault(x => x.NodeInGraph.Id == node.Id);
            if (update != null)
                update.ModuleName = moduleName;
            else
            {
                moduleUpdates.Add(new(node, moduleName));
                saveButton.Text = "*";
            }
        }

        private void GroupNodes()
        {
            var selectedNodes = GetSelectedNodes();
            var selectedSubgraphs = GetSelectedSubGraphs();
            if (selectedNodes.Any())
            {
                Subgraph newSubgraph;
                if (selectedSubgraphs.Count() == 1)
                    newSubgraph = selectedSubgraphs[0];
                else
                {
                    var moduleName = Microsoft.VisualBasic.Interaction.InputBox("Enter Control Module Name", "CM Name", selectedNodes[0].Id);
                    if (string.IsNullOrWhiteSpace(moduleName))
                        return;

                    //todo: check if module is used on another graph and ask whether this is ok
                    newSubgraph = Database.Helper.CreateSubgraph(moduleName);
                    gViewer.Graph.RootSubgraph.AddSubgraph(newSubgraph);
                }

                foreach (var node in selectedNodes)
                {
                    var oldsubgraph = gViewer.Graph.RootSubgraph.Subgraphs.Where(x => x.Nodes.Contains(node)).FirstOrDefault();
                    if (oldsubgraph != null)
                    {
                        oldsubgraph.RemoveNode(node);
                        if (oldsubgraph.Nodes.Count() == 0)
                        {
                            gViewer.Graph.RootSubgraph.RemoveSubgraph(oldsubgraph);
                        }
                    }

                    newSubgraph.AddNode(node);
                    UpdateModuleList(node, Database.Helper.GetModuleNameFromSubgraph(newSubgraph));
                }
                gViewer.Graph.RootSubgraph.AddSubgraph(newSubgraph);
                gViewer.Graph = gViewer.Graph;
            }
        }

        private void UnGroupNodes()
        {
            var selectedNodes = GetSelectedNodes();
            var invalidate = false;
            foreach (var node in selectedNodes)
            {
                var oldsubgraph = gViewer.Graph.RootSubgraph.Subgraphs.Where(x => x.Nodes.Contains(node)).FirstOrDefault();
                if (oldsubgraph != null)
                {
                    oldsubgraph.RemoveNode(node);
                    if (oldsubgraph.Nodes.Count() == 0)
                        gViewer.Graph.RootSubgraph.RemoveSubgraph(oldsubgraph);
                    UpdateModuleList(node, null);
                    invalidate = true;
                }
               
            }
            
            if (invalidate)
                gViewer.Invalidate();
        }

        private List<Node> GetSelectedNodes()
        {
            var selectedObjects = new List<Node>();
            foreach (IViewerNode obj in gViewer.Entities.Where(x => x!= null && x is IViewerNode))
                if (obj.MarkedForDragging && !(obj.Node is Subgraph) && obj.Node is Node)
                    selectedObjects.Add(obj.Node);
            return selectedObjects;
        }

        private List<Subgraph> GetSelectedSubGraphs()
        {
            var selectedObjects = new List<Subgraph>();
            foreach (IViewerNode obj in gViewer.Entities.Where(x => x != null && x is IViewerNode))
                if (obj.MarkedForDragging && obj.Node is Subgraph subgraph)
                {
                    if (gViewer.Graph.RootSubgraph.Subgraphs.Any(x => x.Id == subgraph.Id))
                        selectedObjects.Add(subgraph);
                }
            return selectedObjects;
        }

        private void GraphViewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            selectedObject = e.OldObject != null ? e.OldObject.DrawingObject : null;

            if (selectedObject != null)
            {
                //restore selected object attr
                if (selectedObject is Edge edge)
                    edge.Attr.Color = Microsoft.Msagl.Drawing.Color.White;
                else if (selectedObject is Node node)
                {
                    var decoratedObject = draggedObjectOriginalColors.Keys.Select(x => x as DNode).Where(x => x.Node.Id == node.Id).FirstOrDefault();
                    if (decoratedObject == null || draggedObjectOriginalColors.Count() == 1)
                    {
                        node.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
                        if (node.UserData != null && node.UserData.ToString() == "Hidden Connections")
                            node.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                    }
                        
                }
                    

                gViewer.Invalidate(e.OldObject);
                selectedObject = null;
            }

            if (gViewer.ObjectUnderMouseCursor == null)
                gViewer.SetToolTip(selectedObjectToolTip, "");
            else
            {
                selectedObject = gViewer?.ObjectUnderMouseCursor?.DrawingObject;
                if (selectedObject is Edge selectedEdge)
                {
                    //selectedObjectAttr = selectedEdge.Attr.Clone();
                    selectedEdge.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;
                    gViewer.Invalidate(e.NewObject);
                    var parameters = selectedEdge.LabelText.Split("->");
                    gViewer.SetToolTip(selectedObjectToolTip, $"edge from {selectedEdge.Source}.{parameters[0].Trim()} to {selectedEdge.Target}.{parameters[1].Trim()}");
                }
                else if (selectedObject is Node selectedNode)
                {
                    //selectedObjectAttr = ((Node)GraphViewer.SelectedObject).Attr.Clone();
                    selectedNode.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;
                    gViewer.SetToolTip(selectedObjectToolTip, selectedNode.LabelText);
                    gViewer.Invalidate(e.NewObject);
                }
            }
        }

        private void GraphViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && gViewer.ObjectUnderMouseCursor == null && !gViewer.PanButtonPressed)
                isMouseScreenDrag = true;

            if (e.Button == System.Windows.Forms.MouseButtons.Middle && !gViewer.PanButtonPressed)
            {
                isZoomRectangle = gViewer.WindowZoomButtonPressed;
                isMouseWheelPan = true;
                gViewer.PanButtonPressed = true;
                mouseDownTransform = gViewer.Transform.Clone();

                //GraphViewer.Cursor = new Cursor(a.GetManifestResourceStream(a.GetManifestResourceNames().First(x => x.Contains("hmove.cur"))));

                mouseDownPoint = new Point(e.X, e.Y);
            }

            startScreenPoint = ((Control)sender).PointToScreen(e.Location);
        }

        private void GraphViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseScreenDrag)
            {
                ControlPaint.DrawReversibleFrame(screenDragRectangle, this.BackColor, FrameStyle.Dashed);

                Point endScreenPoint = ((Control)sender).PointToScreen(e.Location);

                var width = endScreenPoint.X - startScreenPoint.X;
                var height = endScreenPoint.Y - startScreenPoint.Y;
                screenDragRectangle = new Rectangle(startScreenPoint.X, startScreenPoint.Y, width, height);

                ControlPaint.DrawReversibleFrame(screenDragRectangle, this.BackColor, FrameStyle.Dashed);
            }
            else if (isMouseWheelPan)
            {
                if (mouseDownTransform != null)
                {
                    gViewer.Transform[0, 2] = mouseDownTransform[0, 2] + e.X - mouseDownPoint.X;
                    gViewer.Transform[1, 2] = mouseDownTransform[1, 2] + e.Y - mouseDownPoint.Y;
                }
                gViewer.Invalidate();
            }
        }

        private void GraphViewer_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseScreenDrag = false;

            if (isMouseWheelPan)
            {
                gViewer.WindowZoomButtonPressed = isZoomRectangle;
                isMouseWheelPan = false;
                gViewer.PanButtonPressed = false;
                gViewer.Cursor = Cursors.Default;
            }

            ControlPaint.DrawReversibleFrame(screenDragRectangle, this.BackColor, FrameStyle.Dashed);

            screenDragRectangle = new Rectangle(0, 0, 0, 0);
        }
    }
}
