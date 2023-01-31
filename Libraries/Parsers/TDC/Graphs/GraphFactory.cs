using Honeywell.Parsers.TDC.Logic;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
//using Database;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Honeywell.TDC;
using Honeywell.Database;
using NPOI.OpenXml4Net.OPC;

namespace Honeywell.Parsers.TDC.Graphs
{
    public static class GraphFactory
    {
        public static Graph Forest { get; set; } = new();
        public static List<Graph> Trees { get; set; } = new();

        /// <summary>
        /// TDC tag mapped to graph
        /// </summary>
        public static Dictionary<string, List<Graph?>> TreeDict { get; set; } = new();
        
        private static void CreateForest()
        {
            #region Add nodes to graph
            foreach (var tag in Parser.TdcTags.Values)
                NodeFactory.CreateOrGetNode(tag, null);

            //break up logic blocks
            foreach (var tag in Parser.TdcTags.Select(x => x.Value).Where(x => x.PointType == "LOGICNIM"))
            {
                var logic = new Logicnim(tag);
                logic.AddInternalLogicNodes();
            }
            #endregion

            #region Add edges to graph after all nodes are built
            //add edges to logc blocks
            foreach (var tdcTag in Parser.TdcTags.Select(x => x.Value).Where(x => x.PointType == "LOGICNIM"))
                (tdcTag.Logic as Logicnim)?.AddEdges();

            //parameter refrences
            var referencesParams = new List<string>() { "CISRC", "CCSRC", "CODSTN", "DISRC", "DODSTN", "PISRC", "LISRC", "LMSRC", "LODSTN", "OISRC", "PVSLTSRC", "SPSLTSRC", "GISRC", "GIENBL", "GOSRC", "GODSTN", "GOENBL", "OPHISRC", "OPLOSRC", "HGSRC", "HGDSTN" }; //"HGSRC", "HGDSTN" are made up parameters
            var futureReferences = new List<string>() { "SVRC", "GIDSTN", "CIDSTN", "PGDSTN", "SGDSTN", "SIDSTN", "PIDSTN", "LOWRDSTN", "RAISDSTN" };

            foreach (var referenceParam in Parser.TdcParameters)
            {
                if (referenceParam.Value == null || referenceParam.Value == "0") //default value for no connection
                    continue;
                else if (referenceParam.ParentTag.PointType == "LOGICNIM") //no logic blocks
                    continue;
                else
                {
                    var param = referenceParam.Name.Contains('(') ? referenceParam.Name[..referenceParam.Name.IndexOf('(')] : referenceParam.Name;
                    if (!referencesParams.Contains(param)) //not a TDC parameter connection
                        continue;
                }

                TdcTag? sourceTag = null, targetTag = null;
                TdcParameter? sourceParameter = null, targetParameter = null;
                string? sourceParamName = null, targetParamName = null;

                if (referenceParam.Name.Contains("DSTN")) //write
                {
                    sourceTag = referenceParam.ParentTag;
                    sourceParameter = referenceParam;
                    sourceParamName = sourceParameter.Name;
                    targetParamName = sourceParameter.Value;

                    if (referenceParam.Value != null && referenceParam.Value.Contains('.'))
                    {
                        targetTag = referenceParam.ConnectedTag;
                        targetParamName = referenceParam.ConnectedParameter;
                    }

                }
                else if (referenceParam.Name.Contains("SRC")) //read
                {
                    targetTag = referenceParam.ParentTag;
                    targetParameter = referenceParam;
                    targetParamName = targetParameter.Name;
                    sourceParamName = targetParameter.Value;

                    if (referenceParam.Value != null && referenceParam.Value.Contains('.'))
                    {
                        sourceTag = referenceParam.ConnectedTag;
                        sourceParamName = referenceParam.ConnectedParameter;
                    }
                }
                else
                    continue;

                if (sourceTag?.PointType == "LOGICNIM" || targetTag?.PointType == "LOGICNIM")
                    continue;

                Node? sourceNode = null, targetNode = null;

                if (targetParamName == "PVSLTSRC")
                    sourceParamName += $".{targetTag.Params.FirstOrDefault(x => x.Name == "PVSIGNAL")?.Value}";
                else if (targetParamName == "SPSLTSRC")
                    sourceParamName += $".{targetTag.Params.FirstOrDefault(x => x.Name == "SPSIGNAL")?.Value}";

                if (sourceNode == null)
                    sourceNode = NodeFactory.CreateOrGetNode(sourceTag, sourceParamName) ?? NodeFactory.CreateOrGetMissingNode(targetParameter);
                if (targetNode == null)
                    targetNode = NodeFactory.CreateOrGetNode(targetTag, targetParamName) ?? NodeFactory.CreateOrGetMissingNode(sourceParameter);

                if (sourceNode != null && targetNode != null)
                    EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
            }
            #endregion

            #region Add edges to CDS
            foreach (var cds in Parser.TdcParameters.Where(x => x.CdsTag != null))
                EdgeFactory.AddEdge(cds.CdsTag.Name, $"{cds.CdsTag.Name} -> {cds.Name}", cds.ParentTag.Name);
            #endregion
        }

        private static void CreateTrees()
        {
            Dictionary<Node, bool> visitedNode = new();
            Dictionary<Edge, bool> visitedEdge = new();

            foreach (var node in Forest.Nodes)
            {
                visitedNode.Add(node, false);
            }

            foreach (var edge in Forest.Edges)
            {
                visitedEdge.Add(edge, false);
            }

            var i = 0;
            foreach (var node in Forest.Nodes)
            {
                if (!visitedNode[node])
                {
                    var graph = new Graph((i++).ToString());
                    AddConnectedNodes(node, graph, visitedNode, visitedEdge);
                    Trees.Add(graph);
                }  
            }
        }

        private static void AddNodeToDict(Node node, Graph graph)
        {
            var key = (node.UserData as TdcTag)?.Name;
            if (key != null)
            {
                if (TreeDict.TryGetValue(key, out var graphs))
                {
                    if (graphs.Any(x => x?.Label.Text == graph.Label.Text)) //if graph is already associated with node don't add it again
                        return;
                }
                TreeDict[key].Add(graph);
            }   
        }

        private static void AddConnectedNodes(Node node, Graph graph, Dictionary<Node, bool> visitedNode, Dictionary<Edge, bool> visitedEdge)
        {
            if (!visitedNode[node])
            {
                graph.AddNode(node);
                AddNodeToDict(node, graph);

                visitedNode[node] = true;
                foreach (var edge in node.Edges)
                {
                    Node node2 = edge.TargetNode == node ? edge.SourceNode : edge.TargetNode;
                    if (!visitedNode[node2])
                        AddConnectedNodes(node2, graph, visitedNode, visitedEdge);

                    if (!visitedEdge.ContainsKey(edge))
                    {
                        EdgeFactory.AddEdge(edge.SourceNode.Id, edge.LabelText, edge.TargetNode.Id);
                        visitedEdge[edge] = true;
                    }

                }
            }

        }

        public static void Clear()
        {
            Forest = new Graph();
            TreeDict.Clear();
        }

        public static void CreateGraphs(string? customConnectionsPath = null)
        {
            CreateForest();
            #region Custom Connections
            if (customConnectionsPath != null)
            {
                if (!File.Exists(customConnectionsPath))
                    throw new Exception("Invalid Custom Connections File Path");

                using Microsoft.VisualBasic.FileIO.TextFieldParser csvParser = new(customConnectionsPath);
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    Parser.TdcTags.TryGetValue(fields[0], out var srcTag);
                    string srcParameter = fields[2];

                    Parser.TdcTags.TryGetValue(fields[1], out var dstnTag);
                    string dstnParameter = fields[3];

                    if (srcTag != null && dstnTag != null)
                        EdgeFactory.AddEdge(srcTag.Name, $"{srcParameter} -> {dstnParameter}", dstnTag.Name);
                }
            }
            #endregion
            CreateTrees();
            ExportToDb();

            OptimizeDebug();
        }

        private static void AssignLogicBlockModule(Graph graph, List<DbTdcNode> dbNodes, TdcTag? tag)
        {
            if (tag == null)
                return;

            var dbNode = dbNodes.FirstOrDefault(x => x.TagId == tag.Id);
            if (dbNode == null)
                return;

            var node = graph.FindNode(dbNode.NodeId);
            
            if (dbNode.ModuleId == null && dbNode.NodeId.Contains('.') && tag.PointType == "LOGICNIM")
            {
                var targetNodes = node.OutEdges.Where(x => x.TargetNode.UserData is TdcTag).Select(x => x.TargetNode);
                var targetTagIdList = targetNodes.Select(x => ((TdcTag)x.UserData).Id).Distinct(); //tags logic block writes to
                var moduleIdList = Parser.TdcTags.Values.Where(x => x.Module != null && targetTagIdList.Contains(x.Id)).Select(x => x.Module.Id).Distinct(); //module of target tags
                if (moduleIdList.Count() == 0) //possibly connected to logic tag that is in module
                    moduleIdList = dbNodes.Where(x => targetNodes.Select(y => y.Id).Contains(x.NodeId)).Where(x => x.ModuleId != null).Select(x => (int)x.ModuleId).Distinct();

                if (moduleIdList.Count() == 1) //connected to a single tag that has module
                {
                    dbNode.ModuleId = moduleIdList.First();

                    var sourceTags = node.InEdges.Where(x => x.SourceNode.UserData is TdcTag).Select(x => x.SourceNode).Distinct(); //tags logic block reads from
                    foreach (var sourceTag in sourceTags)
                        AssignLogicBlockModule(graph, dbNodes, (TdcTag)sourceTag.UserData);
                }

            }
        }

        private static void ExportToDb()
        {
            using var db = new TdcContext(true);

            foreach (var tag in Parser.TdcTags.Values)
                db.TdcTags.Add(new DbTdcTag(tag));

            foreach (var param in Parser.TdcParameters)
                db.TdcParameters.Add(new DbTdcParameter(param));

            Parser.GetModules();
            foreach (var module in Parser.Modules)
                db.TdcModules.Add(new DbTdcModule(module));

            var clrefId = 0;
            foreach (var cl in Parser.TdcCL)
            {
                var dbCl = new DbTdcCL(cl);
                db.TdcCLs.Add(dbCl);

                foreach (var tag in cl.TdcTags)
                    db.TdcCLRefs.Add(new DbTdcCLRefs(++clrefId, tag, cl));
            }

            var nodePk = 0;
            var crossCommId = 0;
            var connectionId = 0;
            foreach (var graph in Trees)
            {
                var graphId = int.Parse(graph.Label.Text);

                #region Add Nodes to DB
                var dbNodes = new List<DbTdcNode>();
                foreach (var node in graph.Nodes)
                {
                    var userData = node.UserData as TdcTag;
                    var dbNode = new Database.DbTdcNode()
                    {
                        Id = ++nodePk,
                        NodeId = node.Id,
                        TagId = userData?.Id,
                        GraphId = graphId,
                        ModuleId = (userData == null) ? null : Parser.TdcTags[userData.Name].Module?.Id
                    };

                    //if broken up logic block with 1 target connection, move into taget module
                    AssignLogicBlockModule(graph, dbNodes, userData);
                    dbNodes.Add(dbNode);

                    #region Add Parameter Connections
                    var dbconnections = new List<DbTdcConnections>();
                    foreach (var edge in node.InEdges)
                    {
                        var parameters = EdgeFactory.SplitEdgeLabel(edge);
                        if (parameters.Source.IndexOf($"{edge.Source}.") == 0)
                            parameters.Source = parameters.Source[(parameters.Source.IndexOf('.') + 1)..];

                        if (parameters.Target.IndexOf($"{edge.Target}.") == 0)
                            parameters.Target = parameters.Target[(parameters.Target.IndexOf('.') + 1)..];

                        var connection = new DbTdcConnections()
                        {
                            Id = connectionId++,
                            NodeId = dbNode.Id, 
                            Parameter = parameters.Target, 
                            ConnectedNodeName = edge.Source, 
                            ConnectedNodeParameter = parameters.Source 
                        };
                        dbconnections.Add(connection);

                    }

                    foreach (var edge in node.OutEdges)
                    {
                        var parameters = EdgeFactory.SplitEdgeLabel(edge);
                        if (parameters.Source.IndexOf($"{edge.Source}.") == 0)
                            parameters.Source = parameters.Source[(parameters.Source.IndexOf('.') + 1)..];

                        if (parameters.Target.IndexOf($"{edge.Target}.") == 0)
                            parameters.Target = parameters.Target[(parameters.Target.IndexOf('.') + 1)..];

                        var connection = new DbTdcConnections()
                        {
                            Id = connectionId++,
                            NodeId = dbNode.Id,
                            Parameter = parameters.Source,
                            ConnectedNodeName = edge.Target,
                            ConnectedNodeParameter = parameters.Target
                        };
                        dbconnections.Add(connection);

                    }

                    db.ParameterConnections.AddRange(dbconnections);
                    #endregion
                }
                db.TdcNodes.AddRange(dbNodes);
                #endregion

                #region Add nodes to modules
                foreach (var node in graph.Nodes)
                {
                    var moduleName = dbNodes.FirstOrDefault(x => x.NodeId == node.Id)?.Module?.Name;
                    if (moduleName == null)
                        continue;

                    var moduleId = $"Module:{moduleName}";
                    Subgraph? subgraph = graph.RootSubgraph.Subgraphs.FirstOrDefault(x => x.Id == moduleId);
                    if (subgraph == null)
                    {
                        subgraph = Database.Helper.CreateSubgraph(moduleId, moduleName);
                        graph.RootSubgraph.AddSubgraph(subgraph);

                    }
                    subgraph.AddNode(node);
                }
                #endregion

                #region Write graph to DB

                //find cross comms
                foreach (var edge in graph.Edges)
                {
                    var sourceNode = edge.SourceNode;
                    var targetNode = edge.TargetNode;

                    if (sourceNode.UserData != null && targetNode.UserData != null) //both tdc tags
                    {
                        var sourceTag = (TdcTag)sourceNode.UserData;
                        var targetTag = (TdcTag)targetNode.UserData;

                        var sourceAddress = Database.Helper.GetLcnAddress(sourceTag);
                        var targetAddress = Database.Helper.GetLcnAddress(targetTag);

                        if (sourceAddress != targetAddress)
                        {
                            var sourceNodeId = dbNodes.FirstOrDefault(x => x.NodeId == sourceNode.Id).Id;
                            var targetNodeId = dbNodes.FirstOrDefault(x => x.NodeId == targetNode.Id).Id;

                            db.SourceTdcCrossComms.Add(new() { Id = ++crossCommId, NodeId = sourceNodeId }) ;
                            db.TargetTdcCrossComms.Add(new() { Id = crossCommId, NodeId = targetNodeId });
                        }
                    }
                }

                //remove user data
                foreach (var node in graph.Nodes)
                {
                    var node2 = graph.FindNode(node.Id);
                    if (node2.UserData != null)
                        node2.UserData = "TDC Tag";
                }

                graph.UserData = graphId;
                var dbGraph = new Database.DbTdcGraph()
                {
                    Id = graphId,
                    Xml = Helper.WriteGraphToXML(graph)
                };
                db.TdcGraphs.Add(dbGraph);
                #endregion
            }

            db.SaveChanges();
        }

        private static void OptimizeDebug()
        {
            Node? tdcTagMaxEdges = null;
            var tdcTagLargestGraph = string.Empty;
            int maxNodes = 0;
            int maxEdges = 0;
            foreach (var kv in TreeDict)
            {
                var graph = TreeDict[kv.Key].FirstOrDefault();
                if (graph != null)
                {
                    if (graph.NodeCount > maxNodes)
                    {
                        maxNodes = graph.NodeCount;
                        tdcTagLargestGraph = kv.Key;
                    }
                }
            }

            foreach (var node in Forest.Nodes)
            {
                var count = node.Edges.Count();
                if (count > maxEdges)
                {
                    maxEdges = count;
                    tdcTagMaxEdges = node;
                }
            }

            var debug1 = DebugNodes("ARRAY").ToString();
            var debug2 = DebugNodes("PRMODNIM").ToString();
            var debug3 = DebugNodes("LOGICNIM").ToString();
        }

        private static StringBuilder DebugNodes(string pointType)
        {
            StringBuilder sb = new();
            var debug = Forest.Nodes.Where(x => (x.UserData as TdcTag)?.PointType == pointType).ToList();
            var debug2 = debug.Where(x => !x.Id.Contains('.') && (x.InEdges.Any() || x.OutEdges.Any())).ToList();
            foreach (var node in debug2)
            {
                foreach (var edge in node.InEdges)
                {
                    sb.AppendLine($"{edge.SourceNode.Id} -> {edge.LabelText} -> {edge.TargetNode.Id}");
                }

                foreach (var edge in node.OutEdges)
                {
                    sb.AppendLine($"{edge.SourceNode.Id} -> {edge.LabelText} -> {edge.TargetNode.Id}");
                }
            }
            return sb;
        }
    }
}
