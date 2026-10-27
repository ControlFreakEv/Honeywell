using Honeywell.TDC;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Honeywell.Database
{
    public class Helper
    {
        public enum FileType { FFL, Schematic, Group }
        public static string GetLcnAddress(DbTdcTag? tag)
        {
            if (tag == null)
                return string.Empty;

            string address = string.Empty;
            if (tag.PointType[^3..] == "NIM" || tag.PointType == "ARRAY" || tag.PointType == "DEVCTL")
                address = $"UCN{tag.NTWKNUM}\\{tag.NODETYP ?? "NODE"}{tag.NODENUM}";
            else if (tag.PointType[^2..] == "AM")
                address = $"AM{tag.AM}";
            else if (tag.PointType[^2..] == "HG" || tag.PointType.Contains("BOX"))
                address = $"HWY{tag.HWYNUM}\\{tag.PNTBOXTY ?? "BOX"}{tag.BOXNUM ?? tag.OUTBOXNM}";


            return address;
        }

        public static string GetLcnAddress(TdcTag? tag)
        {
            return GetLcnAddress(new DbTdcTag(tag));
        }

        public static string GetLcnAddressForNodeLabel(DbTdcTag? tag)
        {
            if (tag == null)
                return string.Empty;

            string address = string.Empty;
            if (tag.PointType[^3..] == "NIM" || tag.PointType == "ARRAY" || tag.PointType == "DEVCTL")
            {
                string? module = null, slot = null;
                if (tag.MODNUM != null && tag.MODNUM != "0")
                    module = "\\MODULE" + tag.MODNUM;

                if (tag.SLOTNUM != null && tag.SLOTNUM != "0")
                    slot = "\\SLOT" + tag.SLOTNUM;

                var extra = module + slot;
                if (extra != null)
                    extra = "\n" + extra.TrimStart('\\');

                address = $"UCN{tag.NTWKNUM}\\{tag.NODETYP ?? "NODE"}{tag.NODENUM}{extra}";
            }
            else if (tag.PointType[^2..] == "AM")
                address = $"AM{tag.AM}";
            else if (tag.PointType[^2..] == "HG" || tag.PointType.Contains("BOX"))
            {
                string? slotOut = tag.OUTSLTNM;
                if (slotOut != null)
                    slotOut = $"\\SLOT{slotOut}";

                string? subSlotOut = tag.OUTSSLT;
                if (subSlotOut != null)
                    subSlotOut = $"\\SUBSLOT{subSlotOut}";

                string? slotIn = tag.SLOTNUM;
                if (slotIn != null)
                    slotIn = $"\\SLOT{slotIn}";

                string? subSlotIn = tag.INPTSSLT;
                if (subSlotIn != null)
                    subSlotIn = $"\\SUBSLOT{subSlotIn}";

                string? intvar = tag.INTVARNM;
                if (intvar != null)
                    intvar = $"\\\nINDEX{intvar}";

                var outAddress = slotOut + subSlotOut;
                if (string.IsNullOrWhiteSpace(outAddress))
                    outAddress = null;

                var inAddress = slotIn + subSlotIn;
                if (string.IsNullOrWhiteSpace(inAddress))
                    inAddress = null;

                string? extra = null;
                if (outAddress != null && inAddress != null && outAddress != inAddress)
                    extra = $"\nIN-{inAddress.TrimStart('\\')}\nOUT-{outAddress.TrimStart('\\')}";
                else if (outAddress != null)
                    extra = "\n" + outAddress.TrimStart('\\');
                else if (inAddress != null)
                    extra = "\n" + inAddress.TrimStart('\\');

                address = $"HWY{tag.HWYNUM}\\{tag.PNTBOXTY ?? "BOX"}{tag.BOXNUM ?? tag.OUTBOXNM}{extra}{intvar}";
            }


            return address;
        }

        public static string GetLcnAddressForNodeLabel(TdcTag? tag)
        {
            return GetLcnAddressForNodeLabel(new DbTdcTag(tag));
        }
        public static Subgraph CreateSubgraph(string moduleId, string moduleName)
        {
            var subgraph = new Subgraph(moduleId) { LabelText = moduleName };
            subgraph.Attr.Color = Color.White;
            subgraph.Attr.FillColor = Color.Black;
            subgraph.Label.FontColor = Color.White;
            return subgraph;
        }

        public static string GetSubgraphId(string moduleName)
        {
            return $"Module:{moduleName}";
        }

        public static string GetModuleNameFromSubgraph(Subgraph subgraph)
        {
            var split = subgraph.Id.Split(':');
            return split[1];
        }

        public static DbTdcNode GetNode(Node node)
        {
            using var db = new TdcContext();

            var dbNode = db.TdcNodes.First(x => x.NodeId == node.Id); ;

            return dbNode;
        }

        public static Subgraph CreateSubgraph(string moduleName)
        {
            return CreateSubgraph(GetSubgraphId(moduleName), moduleName);
        }

        public static Graph ReadGraphFromXML(string xml)
        {
            //read graph from xml
            using var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream);
            streamWriter.Write(xml);
            streamWriter.Flush();//otherwise you are risking empty stream
            stream.Position = 0;
            var graph = Graph.ReadGraphFromStream(stream);

            //bug in library that ignores subgraph if there is only 1
            var delete = graph.RootSubgraph.Subgraphs.Where(x => x.Id == "delete").FirstOrDefault();
            if (delete != null)
                graph.RootSubgraph.RemoveSubgraph(delete);

            return graph;
        }

        public static string WriteGraphToXML(Graph graph)
        {
            graph.GeometryGraph = null;

            //bug in library that ignores subgraph if there is only 1
            Subgraph? subgraph = null;
            if (graph.RootSubgraph.Subgraphs.Count() == 1)
            {
                subgraph = CreateSubgraph("delete", "delete");
                graph.RootSubgraph.AddSubgraph(subgraph);
            }

            //write graph to xml
            string xml;
            using var stream1 = new MemoryStream();
            graph.WriteToStream(stream1);
            stream1.Position = 0;
            using var streamReader = new StreamReader(stream1);
            xml = streamReader.ReadToEnd();

            if (subgraph != null)
                graph.RootSubgraph.RemoveSubgraph(subgraph);

            return xml;
        }


        private static List<Graph> GetGraphsFromXml(IQueryable<DbTdcGraph> tdcGraphs)
        {
            var graphList = new List<Graph>();
            foreach (var tdcGraph in tdcGraphs)
                graphList.Add(ReadGraphFromXML(tdcGraph.Xml));

            return graphList;
        }

        //todo: BFS doesn't work on tags that have multiple graphs i.e. logicnims
        public static Graph? GetGraph(DbTdcTag dbTag, bool directConnections = false, int? maxNodes = null)
        {
            using var db = new TdcContext();
            var tdcGraphs = db.TdcNodes.Where(x => x.Tag != null && x.Tag.Id == dbTag.Id).Select(x => x.Graph).Distinct();
            var graphList = GetGraphsFromXml(tdcGraphs);
            return GetGraph(graphList, dbTag.Name, directConnections, maxNodes);
        }

        public static Graph? GetGraph(DbTdcModule dbModule, int? maxNodes = null)
        {
            using var db = new TdcContext();
            var tdcGraphs = db.TdcNodes.Where(x => x.Module != null && x.Module.Id == dbModule.Id).Select(x => x.Graph).Distinct();
            var graphList = GetGraphsFromXml(tdcGraphs);
            return GetGraph(graphList, dbModule.Nodes.FirstOrDefault()?.NodeId, false, maxNodes);
        }

        private static Graph? GetGraph(List<Graph> graphList, string? bfsTag, bool directConnections, int? maxNodes = null)
        {
            Graph masterGraph = new();

            if (graphList.Any())
            {
                #region Add nodes to graph
                if (graphList.Count == 1)
                    masterGraph = graphList[0];
                else
                {
                    foreach (Graph graph in graphList)
                    {
                        Subgraph? logicSubgraph = null;

                        logicSubgraph = new Subgraph(graph.Label.Text) { LabelText = $"Nodes: {graph.NodeCount}" };

                        foreach (var node in graph.Nodes)
                        {
                            masterGraph.AddNode(node);
                            logicSubgraph?.AddNode(node);
                        }

                        masterGraph.RootSubgraph.AddSubgraph(logicSubgraph);

                        foreach (var edge in graph.Edges)
                        {
                            if (!masterGraph.Edges.Contains(edge))
                                masterGraph.AddEdge(edge.Source, edge.Label.Text, edge.Target).Label.FontSize = edge.Label.FontSize;
                        }
                    }
                }
                #endregion

                #region Remove extra nodes
                masterGraph.CreateGeometryGraph();
                masterGraph.GeometryGraph.UpdateBoundingBox();

                var startNode = masterGraph?.Nodes.FirstOrDefault(node => node.Id == bfsTag);
                if (directConnections)
                {
                    var directNodes = new List<Node>();
                    directNodes.AddRange(startNode.InEdges.Select(x => x.SourceNode));
                    directNodes.AddRange(startNode.OutEdges.Select(x => x.TargetNode));
                    directNodes.Add(startNode);
                    maxNodes = directNodes.Distinct().Count();
                }

                if (maxNodes != null)
                {
                    if (startNode != null && maxNodes < masterGraph.Nodes.Count())
                    {
                        List<Node> nodesToRemove = new();
                        if (masterGraph.NodeCount > 1)
                            nodesToRemove = BFS(startNode, masterGraph).TakeLast(masterGraph.NodeCount - (int)maxNodes).ToList();

                        foreach (var node in masterGraph.Nodes.ToList())
                        {
                            if (node.Edges.Any(x => nodesToRemove.Contains(x.SourceNode) || nodesToRemove.Contains(x.TargetNode)))
                            {
                                var nodeWithMissingConnections = masterGraph.FindNode(node.Id);
                                nodeWithMissingConnections.Attr.Color = Color.Red;
                                nodeWithMissingConnections.UserData = "Hidden Connections";
                            }
                        }

                        foreach (var subgraph in masterGraph.RootSubgraph.Subgraphs)
                        {
                            //remove subgraph from BFS
                            foreach (var node in nodesToRemove)
                            {
                                if (subgraph.Nodes.Contains(node))
                                    subgraph.RemoveNode(node);

                                if (subgraph.Nodes.Count() == 0)
                                    masterGraph.RootSubgraph.RemoveSubgraph(subgraph);
                            }
                        }

                        //remove nodes from BFS
                        foreach (var node in nodesToRemove)
                        {
                            if (masterGraph.Nodes.Contains(node))
                                masterGraph.RemoveNode(node);
                        }

                        //remove edges from BFS
                        foreach (var edge in masterGraph.Edges.ToList())
                        {
                            if (edge.SourceNode == null || edge.TargetNode == null)
                                masterGraph.RemoveEdge(edge);
                        }

                        masterGraph.LayoutAlgorithmSettings = new SugiyamaLayoutSettings();
                    }
                }
                #endregion

                //todo: issue with adding subgraph to another subgraph

            }

            masterGraph.Attr.BackgroundColor = new Color(42, 33, 28);
            return masterGraph;
        }

        //todo: bfs should update tags on map
        private static List<Node> BFS(Node startNode, Graph graph)
        {
            var bfsNodes = new List<Node>();
            var visited = new Dictionary<string, bool>();
            foreach (var node in graph.Nodes)
                visited.Add(node.Id, false);

            var queue = new Queue<Node>();

            CheckBFSNode(startNode, visited, queue);

            while (queue.Any())
            {
                var node = queue.Dequeue();
                bfsNodes.Add(node);
                
                foreach (var edge in node.Edges)
                {
                    CheckBFSNode(edge.SourceNode, visited, queue);
                    CheckBFSNode(edge.TargetNode, visited, queue);
                }
            }
            return bfsNodes;
        }

        private static void CheckBFSNode(Node node, Dictionary<string, bool> visited, Queue<Node> queue)
        {
            if (!visited[node.Id])
            {
                visited[node.Id] = true;
                queue.Enqueue(node);
            }
        }

        //todo: see NULL_SW around 16/17 nodes.
        private static List<Node> BFS2(Node startNode, Graph graph)
        {
            var bfsNodes = new List<Node>();
            var visited = new Dictionary<Node, int?>();
            foreach (var node in graph.Nodes)
                visited.Add(node, null);

            var queue = new Queue<Node>();

            CheckBFSNode2(startNode, visited, queue, 0);

            while (queue.Any())
            {
                var node = queue.Dequeue();

                int depth = (int)visited[node] + 1;
                foreach (var edge in node.Edges)
                {
                    CheckBFSNode2(edge.SourceNode, visited, queue, depth);
                    CheckBFSNode2(edge.TargetNode, visited, queue, depth);
                }
            }

            foreach (var kv in visited.OrderBy(x => x.Value))
                bfsNodes.Add(kv.Key);
            return bfsNodes;
        }

        private static void CheckBFSNode2(Node node, Dictionary<Node, int?> visited, Queue<Node> queue, int depth)
        {
            if (visited[node] != null)
            {
                visited[node] = depth;
                queue.Enqueue(node);
            }
        }

        /// <summary>
        /// Updates all modules in graphs.
        /// </summary>
        public static void RefreshModulesInGraphs()
        {
            var db = new TdcContext();

            var saveChanges = false;
            foreach (var dbGraph in db.TdcGraphs)
            {
                var graph = ReadGraphFromXML(dbGraph.Xml);

                var nodesInModules = dbGraph.Nodes.Where(x => x.Module != null).ToList();
                var modules = nodesInModules.Select(x => x.Module.Name).Distinct().ToList();
                var moduleIds = modules.ToList();
                for (int i = 0; i < moduleIds.Count(); i++)
                    moduleIds[i] = GetSubgraphId(moduleIds[i]);

                var nodesInSubGraphs = graph.RootSubgraph.Subgraphs.SelectMany(x => x.Nodes).ToList();
                var subgraphs = graph.RootSubgraph.Subgraphs.Select(x => x.Id);

                var graphChanged = false;

                #region Check correct modules exist
                foreach (var subgraphName in subgraphs) //remove old subgraphs
                {
                    if (!moduleIds.Contains(subgraphName))
                    {
                        graphChanged = true;
                        var oldSubgraph = graph.RootSubgraph.Subgraphs.FirstOrDefault(x => x.Id == subgraphName);
                        graph.RootSubgraph.RemoveSubgraph(oldSubgraph);
                    }
                }

                foreach (var moduleName in modules) //add missing modules
                {
                    var newSubgraph = graph.RootSubgraph.Subgraphs.FirstOrDefault(x => x.Id == GetSubgraphId(moduleName));
                    if (newSubgraph == null)
                    {
                        graphChanged = true;
                        newSubgraph = CreateSubgraph(moduleName);
                        graph.RootSubgraph.AddSubgraph(newSubgraph);
                    }

                    var nodesInThisModule = nodesInModules.Where(x => x.Module.Name == moduleName);

                    foreach (var node in newSubgraph.Nodes) //check if nodes in subgraph are correct
                    {
                        if (!nodesInThisModule.Any(x => x.NodeId == node.Id)) //not in correct module
                        {
                            graphChanged = true;
                            newSubgraph.RemoveNode(node);
                        }
                    }

                    foreach (var dbNode in nodesInThisModule) //add missing nodes to subgraph
                    {
                        if (!newSubgraph.Nodes.Any(x => x.Id == dbNode.NodeId))
                        {
                            graphChanged = true;
                            var node = graph.FindNode(dbNode.NodeId);
                            newSubgraph.AddNode(node);
                        }
                    }
                }
                #endregion

                if (graphChanged)
                {
                    var xml = WriteGraphToXML(graph);
                    dbGraph.Xml = xml;
                    saveChanges = true;
                }
            }

            if (saveChanges)
                db.SaveChanges();
        }
    }
}
