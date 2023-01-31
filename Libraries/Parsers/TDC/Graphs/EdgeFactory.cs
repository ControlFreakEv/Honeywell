using Honeywell.TDC;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Parsers.TDC.Graphs
{
    internal static class EdgeFactory
    {
        public static double FontSize { get; set; } = 4;
        public static Edge? AddEdge(string? sourceId, string label, string? targetId)
        {
            var parameters = SplitEdgeLabel(label);
            sourceId = UpdateNodeId(sourceId, parameters.Source);
            targetId = UpdateNodeId(targetId, parameters.Target);

            if (sourceId == null || targetId == null)
                return null;//throw new Exception($"Edge node is null");

            var edge = GraphFactory.Forest.Edges.Where(x => x.Source == sourceId && x.Target == targetId && x.LabelText == label).FirstOrDefault();
            if (edge == null)
                edge = GraphFactory.Forest.AddEdge(sourceId, label, targetId);
            edge.Label.FontSize = FontSize;
            edge.Attr.Color = Color.White;
            edge.Label.FontColor = Color.White;
            //if (edge.LabelText.Contains("LISRC") && edge.TargetNode.LabelText.Contains("LOGICNIM"))
            //    edge = edge;

            return edge;
        }

        private static string? UpdateNodeId(string? id, string? parameter)
        {
            if (id == null)
                return null;

            Parser.TdcTags.TryGetValue(id, out TdcTag? tag);
            if (NodeFactory.IsArrayOrProcmod(tag?.PointType))
                return NodeFactory.CreateOrGetNode(tag, parameter?.Trim())?.Id;

            return id;
        }

        public static (string Source, string Target) SplitEdgeLabel(Edge edge)
        {
            return SplitEdgeLabel(edge.LabelText);
        }

        public static (string Source, string Target) SplitEdgeLabel(string labelText)
        {
            var parameters = labelText.Split("->");
            return (parameters[0].Trim(), parameters[1].Trim());
        }
    }
}
