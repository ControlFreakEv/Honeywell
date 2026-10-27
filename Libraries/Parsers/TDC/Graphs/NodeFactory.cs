using Honeywell.Parsers.TDC.Logic;
using Honeywell.TDC;
using Microsoft.Msagl.Drawing;
using Honeywell.Parsers.TDC.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Parsers.TDC.Graphs
{
    internal static class NodeFactory
    {
        private readonly static List<string> io = new() { "ANINNIM", "ANLINHG", "ANOUTNIM", "ANLOUTHG", "DIINNIM", "DIGINHG", "DIOUTNIM", "DIGOUTHG" };
        private readonly static List<string> SplitArrayAndProcmod = new() { "FL(", "NN(", "STR8(", "STR16(", "STR32(", "STR64(", "TIME(" };
        private static int missingId = 0;
        public static Node? CreateOrGetNode(TdcTag? tdcTag, string? parameter)
        {
            if (tdcTag == null)
                return null;
            else if (tdcTag.Logic is LogicHgGate)
                return CreateOrGetLogicNode(tdcTag, tdcTag.Name);


            var id = tdcTag.Name;
            var id2 = SplitTag(tdcTag, parameter);
            if (id2 != null)
                id += $".{id2}"; //arrays, procmodnim, etc... reduces number of connected components in graph

            var ctlEqn = string.Empty;
            if (tdcTag.CTLALGID == "ORSEL")
                ctlEqn = tdcTag.Params.FirstOrDefault(x => x.Name == "CTLEQN")?.Value == "EQB" ? "-Low Select" : "-Hi Select";

            var label = $"{id}\n{tdcTag.PointType}";
            if (tdcTag.PVALGID != null && tdcTag.CTLALGID != null)
                label += $"\n{tdcTag.PVALGID}\\{tdcTag.CTLALGID}{ctlEqn}";
            else if (tdcTag.PVALGID != null)
                label += $"\n{tdcTag.PVALGID}";
            else if (tdcTag.CTLALGID != null)
                label += $"\n{tdcTag.CTLALGID}{ctlEqn}";
            else if (tdcTag.ALGIDDAC != null)
                label += $"\n{tdcTag.ALGIDDAC}";

            label += $"\n{Database.Helper.GetLcnAddressForNodeLabel(tdcTag)}";

            var fontColor = Color.Black;
            var blockColor = Color.Gray;
            if (io.Contains(tdcTag.PointType))
                blockColor = Color.LightBlue;

            var node = CreateNode(blockColor, fontColor, label, id, tdcTag);

            return AddNode(node, tdcTag);
        }

        public static Node? CreateOrGetNode(string? tagname, string? parameter)
        {
            if (tagname == null)
                return null;

            Parser.TdcTags.TryGetValue(tagname, out TdcTag? tdcTag);
            return CreateOrGetNode(tdcTag, parameter);
        }

        public static Node? CreateOrGetMissingNode(TdcParameter? tdcParameter)
        {
            if (tdcParameter == null)
                return null;
            return CreateOrGetMissingNode(tdcParameter.ParentTag, tdcParameter.Value);
        }
        /// <summary>
        /// Creates node for a reference to TDC tag that does not exist in EB files
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Node? CreateOrGetMissingNode(TdcTag connectingTag, string? id)
        {
            Node? node = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var fontColor = Color.Black;
                var blockColor = Color.Red;
                var label = $"{id}\nMissing";

                if (id[0] == '!')
                {
                    blockColor = Color.Green;
                    label = $"{id}\n{connectingTag.NODETYP} Address\n{Database.Helper.GetLcnAddressForNodeLabel(connectingTag)}";
                    id = label;
                }
                else if (string.IsNullOrWhiteSpace(id) || id.ToUpper() == "(MISSING)")
                    id = $"Missing{missingId++}";

                node = CreateNode(blockColor, fontColor, label, id, null);
                node = AddNode(node, null); //if node already exists it will return it here

                return node;
            }

            return node;
        }

        /// <summary>
        /// Create Logic Block
        /// </summary>
        /// <param name="tdcTag"></param>
        /// <param name="id"></param>
        /// <param name="label"></param>
        public static Node? CreateOrGetLogicNode(TdcTag? tdcTag, string? id, string? label)
        {
            if (tdcTag == null || id == null)
                return null;

            if (label == null)
                label = id;

            label += GetLogicNimGateLabel(tdcTag, label) ?? GetLogicHgGateLabel(tdcTag);

            var node = CreateNode(Color.Tan, Color.Black, label, id, tdcTag);
            return AddNode(node, tdcTag);
        }

        /// <summary>
        /// Creates logic block
        /// </summary>
        /// <param name="tdcTag"></param>
        /// <param name="id"></param>
        public static Node? CreateOrGetLogicNode(TdcTag? tdcTag, string? id)
        {
            return CreateOrGetLogicNode(tdcTag, id, id);
        }

        /// <summary>
        /// Adds node to tree if it doesn't exist
        /// </summary>
        /// <param name="node"></param>
        /// <param name="tdcTag"></param>
        /// <returns></returns>
        private static Node? AddNode(Node? node, TdcTag? tdcTag)
        {
            if (node == null)
                return null;

            var node2 = GraphFactory.Forest.FindNode(node.Id); //does node already exist?
            if (node2 == null) //if node doesn't exist
            {
                GraphFactory.Forest.AddNode(node);
                tdcTag?.Nodes.Add(node);
            }
            else
                node = node2;

            return node;
        }

        private static Node CreateNode(Color fillColor, Color fontColor, string label, string id, TdcTag? tdcTag)
        {
            NodeAttr attr = new()
            {
                FillColor = fillColor,
                Color = fontColor,
                Shape = Shape.Box

            };

            if (tdcTag != null && tdcTag.CL.Any())
                attr.Shape = Shape.Ellipse;

            var node = new Node(label)
            {
                UserData = tdcTag,
                Attr = attr,
                Id = id
            };

            node.Label.FontSize = 8;
            node.Label.FontColor = fontColor;

            return node;
        }

        public static string? GetLogicHgGateLabel(TdcTag tdcTag)
        {
            var logicHg = tdcTag.Logic as LogicHgGate;
            if (logicHg == null)
                return null;
            var label = $"\n{logicHg.GateType.ToString()}";

            if (logicHg.Time != null)
                label += $"\n(SECS={logicHg.Time})";
            return label;
        }

        public static string? GetLogicNimGateLabel(TdcTag tdcTag, string label)
        {
            var logicnim = tdcTag.Logic as Logicnim;
            if (logicnim == null)
                return null;

            var tagParamArray = label.Split('.');
            var parameter = tagParamArray.Length > 1 ? tagParamArray[1] : null;
            if (parameter == null || tagParamArray[1].Contains('\n'))
                return null;
            parameter = parameter.Contains(')') ? parameter[..(1 + parameter.IndexOf(')'))] : parameter;

            if (parameter.Length < 3)
                return null;

            if (parameter[..3] == "FL(")
            {
                if (parameter == "FL(1)")
                    return "\n(Always Off)";
                else if (parameter == "FL(2)")
                    return "\n(Always On)";
                else if (parameter == "FL(3)")
                    return "\n(Point Activation)";
                else if (parameter == "FL(4)")
                    return "\n(PMM Startup)";
                else if (parameter == "FL(5)")
                    return "\n(At least one bad LI)";
                else if (parameter == "FL(6)")
                    return "\n(Watchdog reset flag)";
            }
            else if (parameter[..3] == "NN(")
            {
                if (parameter == "NN(1)")
                    return $"\n(PV={logicnim.NN1})";
                else if (parameter == "NN(2)")
                    return $"\n(PV={logicnim.NN2})";
                else if (parameter == "NN(3)")
                    return $"\n(PV={logicnim.NN3})";
                else if (parameter == "NN(4)")
                    return $"\n(PV={logicnim.NN4})";
                else if (parameter == "NN(5)")
                    return $"\n(PV={logicnim.NN5})";
                else if (parameter == "NN(6)")
                    return $"\n(PV={logicnim.NN6})";
                else if (parameter == "NN(7)")
                    return $"\n(PV={logicnim.NN7})";
                else if (parameter == "NN(8)")
                    return $"\n(PV={logicnim.NN8})";
            }

            return null;
        }

        public static string? SplitTag(TdcTag? tdcTag, string? parameter)
        {
            if (parameter == null)
                return null;

            if (IsArrayOrProcmod(tdcTag?.PointType))
            {
                foreach (var param in SplitArrayAndProcmod)
                {
                    if (parameter.Contains(param))
                    {
                        return parameter;
                    }
                }
            }
            return null;
        }

        public static bool IsArrayOrProcmod(string? pointType)
        {
            return pointType == "ARRAY" || pointType == "PRMODNIM";
        }
    }
}
