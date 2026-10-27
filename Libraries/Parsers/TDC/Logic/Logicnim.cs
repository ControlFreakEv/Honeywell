using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static Honeywell.Parsers.TDC.Logic.LogicNimTypes;
using Microsoft.Msagl.Drawing;
using Honeywell.Parsers.TDC.Graphs;
using Honeywell.TDC;
//using Microsoft.Msagl.Core.Layout;

namespace Honeywell.Parsers.TDC.Logic
{
    //todo: issue with logicnim writing to itself
    //todo: numerics/flags/etc if they are referenced in CL they should appear as ovals. Lexer should also be update cl on tag and parameters based off of the internal tag
    internal class Logicnim
    {
        #region Properties
        public string Name { get; set; }
        public TdcTag Tag { get; set; }

        //numerics
        public float? NN1 { get; set; }
        public float? NN2 { get; set; }
        public float? NN3 { get; set; }
        public float? NN4 { get; set; }
        public float? NN5 { get; set; }
        public float? NN6 { get; set; }
        public float? NN7 { get; set; }
        public float? NN8 { get; set; }

        //misc
        public List<LogicGate> LogicGateConnections { get; set; } = new List<LogicGate>(); //inner workings of blocks
        #endregion

        #region Contructor
        public Logicnim(TdcTag tdcTag)
        {
            tdcTag.Logic = this;
            UpdateTdcConnections(tdcTag);

            Tag = tdcTag;
            Name = tdcTag.Name;

            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(1)")?.Value, out float _NN1))
                NN1 = _NN1;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(2)")?.Value, out float _NN2))
                NN2 = _NN2;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(3)")?.Value, out float _NN3))
                NN3 = _NN3;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(4)")?.Value, out float _NN4))
                NN4 = _NN4;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(5)")?.Value, out float _NN5))
                NN5 = _NN5;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(6)")?.Value, out float _NN6))
                NN6 = _NN6;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(7)")?.Value, out float _NN7))
                NN7 = _NN7;
            if (float.TryParse(tdcTag.Params.FirstOrDefault(x => x.Name == "NN(8)")?.Value, out float _NN8))
                NN8 = _NN8;

            for (int i = 1; i < 25; i++)
            {
                var logicType = GetLogicType(tdcTag.Params.FirstOrDefault(x => x.Name == $"LOGALGID({i})")?.Value);

                if (logicType == LogicNimBlockTypes.NULL)
                    continue;

                LogicGateConnections.Add(new LogicGate(this, i, logicType, tdcTag));
            }
        }

        private void UpdateTdcConnections(TdcTag tdcTag)
        {
            foreach (var tdcParameter in tdcTag.Params)
            {
                var paramName = tdcParameter.Name;
                var paramValue = tdcParameter.Value;

                if (paramName.IndexOf("LOSRC(") == 0 || paramName.IndexOf("LISRC(") == 0 || paramName.IndexOf("LODSTN(") == 0 || paramName.IndexOf("LOENBL(") == 0 || paramName.IndexOf("PRMDESC(") == 0 ||
               paramName.IndexOf("R1(") == 0 || paramName.IndexOf("R2(") == 0 || paramName.IndexOf("S1(") == 0 || paramName.IndexOf("S2(") == 0 || paramName.IndexOf("S3(") == 0 || paramName.IndexOf("S4(") == 0)
                {
                    if (paramValue == null)
                        continue;

                    if (paramValue.IndexOf("L") == 0)
                    {
                        if (paramValue == "L1")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(1)")?.Value;
                        else if (paramValue == "L2")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(2)")?.Value;
                        else if (paramValue == "L3")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(3)")?.Value;
                        else if (paramValue == "L4")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(4)")?.Value;
                        else if (paramValue == "L5")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(5)")?.Value;
                        else if (paramValue == "L6")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(6)")?.Value;
                        else if (paramValue == "L7")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(7)")?.Value;
                        else if (paramValue == "L8")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(8)")?.Value;
                        else if (paramValue == "L9")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(9)")?.Value;
                        else if (paramValue == "L10")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(10)")?.Value;
                        else if (paramValue == "L11")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(11)")?.Value;
                        else if (paramValue == "L12")
                            tdcParameter.Value = tdcTag.Params.FirstOrDefault(x => x.Name == "LISRC(12)")?.Value;
                    }
                    else if (paramValue.IndexOf("FL") == 0)
                    {
                        if (paramValue == "FL1")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(1)";
                        else if (paramValue == "FL2")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(2)";
                        else if (paramValue == "FL3")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(3)";
                        else if (paramValue == "FL4")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(4)";
                        else if (paramValue == "FL5")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(5)";
                        else if (paramValue == "FL6")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(6)";
                        else if (paramValue == "FL7")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(7)";
                        else if (paramValue == "FL8")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(8)";
                        else if (paramValue == "FL9")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(9)";
                        else if (paramValue == "FL10")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(10)";
                        else if (paramValue == "FL11")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(11)";
                        else if (paramValue == "FL12")
                            tdcParameter.Value = $"{tdcTag.Name}.FL(12)";
                    }
                    else if (paramValue.IndexOf("NN") == 0)
                    {
                        if (paramValue == "NN1")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(1)";
                        else if (paramValue == "NN2")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(2)";
                        else if (paramValue == "NN3")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(3)";
                        else if (paramValue == "NN4")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(4)";
                        else if (paramValue == "NN5")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(5)";
                        else if (paramValue == "NN6")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(6)";
                        else if (paramValue == "NN7")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(7)";
                        else if (paramValue == "NN8")
                            tdcParameter.Value = $"{tdcTag.Name}.NN(8)";
                    }
                    else if (paramValue.IndexOf("SO") == 0)
                    {
                        if (paramValue == "SO1")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(1)";
                        else if (paramValue == "SO2")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(2)";
                        else if (paramValue == "SO3")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(3)";
                        else if (paramValue == "SO4")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(4)";
                        else if (paramValue == "SO5")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(5)";
                        else if (paramValue == "SO6")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(6)";
                        else if (paramValue == "SO7")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(7)";
                        else if (paramValue == "SO8")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(8)";
                        else if (paramValue == "SO9")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(9)";
                        else if (paramValue == "SO10")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(10)";
                        else if (paramValue == "SO11")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(11)";
                        else if (paramValue == "SO12")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(12)";
                        else if (paramValue == "SO13")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(13)";
                        else if (paramValue == "SO14")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(14)";
                        else if (paramValue == "SO15")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(15)";
                        else if (paramValue == "SO16")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(16)";
                        else if (paramValue == "SO17")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(17)";
                        else if (paramValue == "SO18")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(18)";
                        else if (paramValue == "SO19")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(19)";
                        else if (paramValue == "SO20")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(20)";
                        else if (paramValue == "SO21")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(21)";
                        else if (paramValue == "SO22")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(22)";
                        else if (paramValue == "SO23")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(23)";
                        else if (paramValue == "SO24")
                            tdcParameter.Value = $"{tdcTag.Name}.SO(24)";
                    }
                    else if (paramValue == "0")
                        tdcParameter.Value = null;
                }
            }
        }
        #endregion

        #region Is parameter
        /// <summary>
        /// id contains the parameter "FL(" or "NN("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsFlagOrNumeric(string? id)
        {
            return IsFlag(id) || IsNumeric(id);
        }

        /// <summary>
        /// id contains the parameter "DEADBAND("  or "DLYTIME("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsDeadbandOrDelay(string? id)
        {
            return IsDeadband(id) || IsDelay(id);
        }

        /// <summary>
        /// id contains the parameter "DEADBAND("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsDeadband(string? id)
        {
            return IsParameter(id, "DEADBAND(");
        }

        /// <summary>
        /// id contains the parameter "DLYTIME("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsDelay(string? id)
        {
            return IsParameter(id, "DLYTIME(");
        }

        /// <summary>
        /// id contains the parameter "FL("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsFlag(string? id)
        {
            return IsParameter(id, "FL(");
        }

        /// <summary>
        /// id contains the parameter "NN("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsNumeric(string? id)
        {
            return IsParameter(id, "NN(");
        }

        /// <summary>
        /// id contains the parameter "SO("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsSO(string? id)
        {
            return IsParameter(id, "SO(");
        }

        /// <summary>
        /// id contains the parameter "FL(1)"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsOff(string? id)
        {
            return IsParameter(id, "FL(1)");
        }

        /// <summary>
        /// id contains the parameter "FL(2)"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsOn(string? id)
        {
            return IsParameter(id, "FL(2)");
        }

        /// <summary>
        /// id contains the parameter "LISRC("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsLISRC(string? id)
        {
            return IsParameter(id, "LISRC(");
        }

        /// <summary>
        /// id contains the parameter "LOSRC("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsLOSRC(string? id)
        {
            return IsParameter(id, "LOSRC(");
        }

        /// <summary>
        /// id contains the parameter "LODSTN("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsLODSTN(string? id)
        {
            return IsParameter(id, "LODSTN(");
        }

        /// <summary>
        /// id contains the parameter "LOENBL("
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsLOENBL(string? id)
        {
            return IsParameter(id, "LOENBL(");
        }

        /// <summary>
        /// returns <returns>true</returns> if "id" contains a parameter named "parameterToMatch"
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameterToMatch"></param>
        /// <returns></returns>
        private static bool IsParameter(string? id, string parameterToMatch)
        {
            if (id == null)
                return false;

            var parameter = id;
            if (id.Contains('.'))
            {
                var tagParam = id.Split('.');
                var tagName = tagParam[0];
                Parser.TdcTags.TryGetValue(tagName, out TdcTag? tdcTag);
                if (tdcTag?.PointType != "LOGICNIM")
                    return false;
                parameter = tagParam[1];
            }
                
            return parameter.IndexOf(parameterToMatch) == 0;
        }

        private bool IsLogicnim(string? pointType)
        {
            return pointType == "LOGICNIM";
        }
        #endregion

        #region Get ID
        private string GetIdLOSRC(int gate)
        {
            return GetIdLOSRC(Name, gate);
        }

        private string GetIdLOSRC(string name, int gate)
        {
            return $"{name}.LOSRC({gate})";
        }

        private string GetIdLOENBL(int gate)
        {
            return $"{Name}.LOENBL({gate})";
        }

        private string GetIdPush(int gate)
        {
            return GetIdPush(Name, gate);
        }

        private string GetIdPush(string name, int gate)
        {
            return $"{name}.PUSH({gate})";
        }

        private string? GetIdSO(TdcParameter tdcParameter)
        {
            if (tdcParameter.ConnectedTag == null || tdcParameter.Value == null)
                return null;

            return GetIdSO(tdcParameter.ConnectedTag.Name, GetGate(tdcParameter.Value));
        }

        private string GetIdSO(int gate)
        {
            return GetIdSO(Name, gate);
        }

        private string GetIdSO(string name, int gate)
        {
            return $"{name}.SO({gate})";
        }

        private string GetIdFlagOrNumeric(string name, string parameter)
        {
            return $"{name}.{parameter}";
        }

        private string GetIdFlagOrNumeric(string parameter)
        {
            return GetIdFlagOrNumeric(Name, parameter);
        }

        private string GetNotId(string parameter, int gate)
        {
            return GetNotId(Name, parameter, gate);
        }

        public static string GetNotId(string tagName, string parameter, int gate)
        {
            return $"{tagName}.NOT{parameter}({gate})";
        }

        public static string GetGateInputId(string id, int gate)
        {
            return $"{id}({gate})";
        }

        public static int GetGate(string id)
        {
            if (id.Contains('('))
                return int.Parse(id[(id.IndexOf("(") + 1)..^1]);
            throw new Exception($"{id} does not contain Gate #");
        }
        #endregion

        #region Nodes
        public void AddInternalLogicNodes()
        {
            #region create logic gates
            foreach (var logicGate in LogicGateConnections)
                NodeFactory.CreateOrGetLogicNode(Tag, logicGate.ID, logicGate.Label);
            #endregion

            #region add push blocks and its inputs (flags, numerics) 
            for (int i = 1; i < 13; i++)
            {
                var LODSTN = Tag.Params.FirstOrDefault(x => x.Name == $"LODSTN({i})")?.Value;
                var LOENBL = Tag.Params.FirstOrDefault(x => x.Name == $"LOENBL({i})")?.Value;
                var LOSRC = Tag.Params.FirstOrDefault(x => x.Name == $"LOSRC({i})")?.Value;

                if (LODSTN == null)
                    continue;

                //PUSH.ENABLE
                if (!IsOn(LOENBL)) //need push block (enable not always on)
                {
                    NodeFactory.CreateOrGetLogicNode(Tag, GetIdPush(i)); //creates push block as logic node

                    if (IsFlag(LOENBL))
                    {
                        if (IsOff(LOENBL))
                            NodeFactory.CreateOrGetLogicNode(Tag, GetIdLOENBL(i), $"{LOENBL}\n(if LODSTN is bool\n push on LOSRC state change\n else, always off)"); //creates flag that toggles push enable  as logic node
                        else
                            NodeFactory.CreateOrGetLogicNode(Tag, GetIdLOENBL(i), LOENBL); //creates flag that toggles push enable  as logic node
                    }
                       
                }

                //PUSH.IN or tag that directly connects to LODSTN
                if (IsFlagOrNumeric(LOSRC))
                    NodeFactory.CreateOrGetLogicNode(Tag, GetIdLOSRC(i), LOSRC); //creates flag/numeric input to push as logic node
                else //tag should already be created, but attempt to create new node just in case
                {
                    var source = LOSRC?.Split('.');
                    var sourceId = source?[0] ?? GetIdLOSRC(i);
                    NodeFactory.CreateOrGetMissingNode(Tag, sourceId); //creates tdc tag as input to push
                }
            }
            #endregion
        }

        private (Node?, string?) GetNodePush(int gate)
        {
            return GetNodePush(Tag, gate);
        }

        private (Node?, string?) GetNodePush(TdcTag? logicTag, int gate)
        {
            if (logicTag == null)
                return (null, null);

            var pushId = GetIdPush(logicTag.Name, gate);
            var pushNode = logicTag.Nodes.Where(x => x.Id == pushId).FirstOrDefault(); //gets push node
            string? pushParam = null;
            if (pushNode != null)
                pushParam = "OUT";

            return (pushNode, pushParam);
        }

        private (Node?, string?) GetNodeLOENBL(TdcParameter? LOENBL)
        {
            if (LOENBL?.Value == null)
                return (null, null);

            int gate = GetGate(LOENBL.Name);

            (var node, var enableParamName) = GetNodeSO(LOENBL);
            if (node == null)
            {
                if (IsFlag(LOENBL.Value))  //flag 
                {
                    var enableId = GetIdLOENBL(gate);
                    enableParamName = "PVFL";
                    node = GraphFactory.Forest.FindNode(enableId);
                }
                else if (IsNumeric(LOENBL.Value))  //numeric
                {
                    var enableId = GetIdLOENBL(gate);
                    enableParamName = "PV";
                    node = GraphFactory.Forest.FindNode(enableId);
                }
                else
                    (node, enableParamName) = GetNodeTag(LOENBL);
            }

            return (node, enableParamName);
        }

        private (Node?, string?) GetNodeLOSRC(TdcParameter? LOSRC)
        {
            if (LOSRC?.Value == null)
                return (null, null); //no LOSRC defined, therefore no connection needed

            int gate = GetGate(LOSRC.Name);

            (var losrcNode, var losrcParamName) = GetNodeSO(LOSRC);
            if (losrcNode == null)
            {
                if (IsFlag(LOSRC.Value))
                {
                    var losrcId = GetIdLOSRC(LOSRC.ParentTag.Name, gate);
                    losrcNode = LOSRC.ParentTag.Nodes.Where(x => x.Id == losrcId).FirstOrDefault(); //get's LOSRC node if it's internal logic FL,NN
                    losrcParamName = "PVFL";
                } 
                else if (IsNumeric(LOSRC.Value))
                {
                    var losrcId = GetIdLOSRC(LOSRC.ParentTag.Name, gate);
                    losrcNode = LOSRC.ParentTag.Nodes.Where(x => x.Id == losrcId).FirstOrDefault(); //get's LOSRC node if it's internal logic FL,NN
                    losrcParamName = "PV";
                }
                else
                    (losrcNode, losrcParamName) = GetNodeTag(LOSRC);
            }
            return (losrcNode, losrcParamName);
        }

        private (List<Node?>, string?) GetNodeLODSTN(TdcParameter? LODSTN)
        {
            List<Node?> nodes = new(1);

            if (LODSTN?.Value == null)
                return (nodes, null);

            (var node, var targetParamName) = GetNodeSO(LODSTN);
            if (node == null)
            {
                if (IsFlag(LODSTN.Value))
                {
                    foreach (var node2 in GraphFactory.Forest.Nodes.Where(x => x.LabelText.IndexOf(LODSTN.Value) == 0))
                        nodes.Add(node2);
                    targetParamName = "PVFL";
                }
                else if (IsNumeric(LODSTN.Value))
                {
                    foreach (var node2 in GraphFactory.Forest.Nodes.Where(x => x.LabelText.IndexOf(LODSTN.Value) == 0))
                        nodes.Add(node2);
                    targetParamName = "PV";
                }
                else
                    (node, targetParamName) = GetNodeTag(LODSTN);
            }

            if (node != null)
                nodes.Add(node);

            return (nodes, targetParamName);
        }

        /// <summary>
        /// Returns SO, deadband, or delay node if it exists
        /// </summary>
        /// <param name="tdcParameter"></param>
        /// <returns></returns>
        private (Node?, string?) GetNodeSO(TdcParameter tdcParameter)
        {
            var id = tdcParameter.Value;

            if (id == null)
                return (null, null);

            string? soId = null, soParamName = null;
            Node? node = null;
            if (IsSO(id)) //logic gate
            {
                soId = id;
                soParamName = "OUT";
            }
            else if (IsDeadband(id)) //logic gate
            {
                soId = GetIdSO(tdcParameter);
                soParamName = "DEADBAND";
            }
            else if (IsDelay(id)) //logic gate
            {
                soId = GetIdSO(tdcParameter);
                soParamName = "DLYTIME";
            }

            if (soId != null)
                node = GraphFactory.Forest.FindNode(soId) ?? LogicGate.CreateNullGate(soId); //connects to a null gate

            return (node, soParamName);
        }

        private (Node?, string?) GetNodeTag(TdcParameter tdcParameter)
        {
            var id = tdcParameter?.Value;
            if (id == null)
                return (null, null);

            string? tagParamName = null;
            Node? node = null;
            if (id.Contains('.')) //TDC tag
            {
                var tagParam = id?.Split('.');
                var tagName = tagParam?[0];
                if (tagParam?.Length > 1)
                    tagParamName = tagParam?[1];
                node = NodeFactory.CreateOrGetNode(tagName, tagParamName) ?? NodeFactory.CreateOrGetMissingNode(tdcParameter);
            }
            else
                node = NodeFactory.CreateOrGetMissingNode(tdcParameter);

            return (node, tagParamName);
        }
        #endregion

        #region Edges
        /// <summary>
        /// Adds edges from logic gates to other tags and to internal logic gates
        /// </summary>
        private void AddLogicGateEdges()
        {
            #region add edges to logic gates
            foreach (var logicGate in LogicGateConnections)
            {
                if (logicGate == null)
                    continue;

                AddInputEdgesToGates(logicGate, logicGate.S1, "S1", logicGate.S1REV);
                AddInputEdgesToGates(logicGate, logicGate.S2, "S2", logicGate.S2REV);
                AddInputEdgesToGates(logicGate, logicGate.S3, "S3", logicGate.S3REV);
                AddInputEdgesToGates(logicGate, logicGate.S4, "S4", logicGate.S4REV);
                AddInputEdgesToGates(logicGate, logicGate.R1, "R1");
                AddInputEdgesToGates(logicGate, logicGate.R2, "R2");
            }
            #endregion

            if (Name == "VL24003B")
                Name = Name;

            #region add edges to push blocks or directly to affected TDC tag
            for (int i = 1; i < 13; i++)
            {
                var LODSTN = Tag.Params.FirstOrDefault(x => x.Name == $"LODSTN({i})");
                var LOENBL = Tag.Params.FirstOrDefault(x => x.Name == $"LOENBL({i})");
                var LOSRC = Tag.Params.FirstOrDefault(x => x.Name == $"LOSRC({i})");

                if (LODSTN?.Value == null)
                    continue;

                //if (!IsOff(LOENBL?.Value))
                //{
                (var sourceNode, var sourceParamName) = GetNodeLOSRC(LOSRC);
                (var targetNodes, var targetParamName) = GetNodeLODSTN(LODSTN);

                foreach (var targetNode in targetNodes)
                {
                    if (IsOn(LOENBL?.Value) && sourceNode != null && targetNode != null) //always on i.e. no push block
                        EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
                    else //if (!IsOff(LOENBL?.Value)) //push block exists and can be enabled
                    {
                        (var enableNode, var enableParamName) = GetNodeLOENBL(LOENBL);

                        (var pushNode, var pushParamName) = GetNodePush(i);
                        if (sourceNode != null && pushNode != null)  //sometimes LOSRC?.Value = 0
                            EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> SOURCE", pushNode.Id); //source
                        if (enableNode != null && pushNode != null) //sometimes LOENBL?.Value = 0
                            EdgeFactory.AddEdge(enableNode.Id, $"{enableParamName} -> ENABLE", pushNode.Id); //enable
                        if (pushNode != null && targetNode != null)
                            EdgeFactory.AddEdge(pushNode.Id, $"{pushParamName}-> {targetParamName}", targetNode.Id); //push
                    }
                    //}
                }
            }
            #endregion
        }

        private void AddInputEdgesToGates(LogicGate logicGate, string? input, string parameter, bool reverse = false)
        {
            var gate = logicGate.Gate;
            var id = logicGate.ID;

            if (input != null)
            {
                string? sourceId = null, sourceParamName = null;
                if (IsFlagOrNumeric(input)) //flag or numeric
                {
                    sourceId = GetGateInputId(input, gate);
                    sourceParamName = IsNumeric(input) ? "PV" : "PVFL";
                }
                else if (IsDeadbandOrDelay(input)) //logic gate
                {
                    sourceId = GetIdSO(GetGate(input));
                    sourceParamName = IsDeadband(input) ? "DEADBAND" : "DLYTIME";
                }
                else if (IsSO(input)) //logic gate
                {
                    sourceId = input;
                    sourceParamName = "OUT";
                }
                else if (input.Contains('.')) //tdc tag
                {
                    var source = input.Split('.');
                    sourceId = source?[0];
                    sourceParamName = source?[1];
                    Parser.TdcTags.TryGetValue(sourceId, out TdcTag? sourceTag);
                    if (sourceTag != null && sourceParamName != null)
                        sourceId = NodeFactory.CreateOrGetNode(sourceTag, sourceParamName)?.Id;
                }
                else
                    sourceId = NodeFactory.CreateOrGetMissingNode(Tag, input)?.Id;

                if (!reverse)
                {
                    var label = $"{sourceParamName} -> ";
                    if (logicGate.LOGALGID == LogicNimBlockTypes.FLIPFLOP)
                    {
                        if (parameter == "S1")
                            label += "RESET";
                        else if (parameter == "S2")
                            label += "SET";
                        else if (parameter == "S3")
                            label += "R/S DOMINANT";
                    }
                    else
                        label += parameter;

                    EdgeFactory.AddEdge(sourceId, label, id);
                }
                else
                {
                    var notId = GetNotId(parameter, gate);
                    EdgeFactory.AddEdge(sourceId, $"{sourceParamName} -> IN", notId); // input -> not
                    EdgeFactory.AddEdge(notId, $"OUT -> {parameter}", id); //not -> gate
                }
            }
        }

        /// <summary>
        /// Add edges to nodes that write to the DEADBAND, DLYTIME, NN, or FL parameters
        /// </summary>
        private void SourceWritesToLogicnim()
        {
            var refs = Parser.TdcParameters
                .Where(param => param.Value?.IndexOf($"{Name}.") == 0)//.Where(param => param.Value?.IndexOf($"{Name}.NN") == 0 || param.Value?.IndexOf($"{Name}.FL") == 0)
                .Where(param => param.Name.Contains("DSTN"))
                .Where(param => param.ParentTag.PointType != "LOGICNIM");
            foreach (var sourceParameter in refs) //loop through tags writing to this logic blocks flags/numerics
            {
                var sourceTag = sourceParameter.ParentTag; //this is the external tag writing to logicnim
                var sourceParamName = sourceParameter.Name; //this is the external tag parameter writing to logicnim
                var targetTagAndParam = sourceParameter.Value; //this is logicnim.parameter

                if (targetTagAndParam == null) //no parameter to write to
                    continue;
                else if (IsLISRC(targetTagAndParam) || IsLOSRC(targetTagAndParam) || IsLOENBL(targetTagAndParam) || IsLODSTN(targetTagAndParam)) //this logic block
                    continue;

                #region Tag writing to THIS logicnim (i.e source)
                var sourceNode = NodeFactory.CreateOrGetNode(sourceTag, sourceParamName);

                if (sourceNode == null) //LOSRC is null and nothing actually writes to tag
                    continue;
                #endregion

                #region THIS logicnim (i.e target)
                if (targetTagAndParam.Contains('.'))
                {
                    var pointParam = targetTagAndParam.Split('.');
                    var targetParamName = pointParam[1]; //this is logicnim's parameter that is being written to
                    if (IsFlagOrNumeric(targetTagAndParam)) //writes to internal flag/numeric
                    {
                        var targetNodes = Tag.Nodes.Where(x => x.LabelText.IndexOf(targetTagAndParam) == 0); //label contains logicnim.FL/NN. could be many nodes that contain same FL/NN
                        if (targetNodes.Any())
                        {
                            foreach (var targetNode in targetNodes)
                                EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
                        }
                        else //writes to a FL/NN that hasn't been created i.e. the FL/NN isn't used in normal logic but could be used in CL, gfx, LM, etc
                        {
                            var targetNode = NodeFactory.CreateOrGetLogicNode(Tag, GetIdFlagOrNumeric(targetParamName));
                            if (targetNode == null)
                                throw new Exception($"Could not locate node for {sourceTag.Name}.{sourceParamName}");
                            EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
                        }
     
                    }
                    else if (IsDeadbandOrDelay(targetTagAndParam))
                    {
                        var gate = GetGate(targetTagAndParam);
                        var targetNode = NodeFactory.CreateOrGetLogicNode(Tag, GetIdSO(gate));
                        EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
                    }
                    else //writes to any other parameter of logic block e.g. ptexecst
                    {
                        var targetNode = Tag.Nodes.Where(x => x.Id == Name).FirstOrDefault();
                        EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
                    }
                } 
                else //don't think this should ever happen
                    throw new Exception($"Could not locate node for {targetTagAndParam}");
                #endregion
            }
        }

        //todo: add deadband and delay to AddLogicnimToTargetEdges() and AddSourceToLogicnimEdges()
        /// <summary>
        /// Add edges to nodes that read the DEADBAND, DLYTIME, NN, FL, or SO parameters
        /// </summary>
        private void TargetReadsFromLogicnim()
        {
            var refs = Parser.TdcParameters
                .Where(x => x.Value?.IndexOf($"{Name}.") == 0)//.Where(x => x.Value?.IndexOf($"{Name}.NN(") == 0 || x.Value?.IndexOf($"{Name}.FL") == 0 || x.Value?.IndexOf($"{Name}.SO") == 0) //this logic block
                .Where(x => x.Name.Contains("SRC")); //another tag reads logic
            foreach (var targetParameter in refs)
            {
                var targetTag = targetParameter.ParentTag; //this is the external tag that is reading LOGICNIM parameter
                var targetParamName = targetParameter.Name; //this is the external tag's parameter 
                var sourceTagAndParam = targetParameter.Value; //this is the LOGICNIM in this class

                if (sourceTagAndParam == null)
                    continue; //this should never happen
                else if (IsLISRC(sourceTagAndParam) || IsLOSRC(sourceTagAndParam) || IsLOENBL(sourceTagAndParam) || IsLODSTN(sourceTagAndParam)) //this logic block
                    continue;
                else if (targetTag.PointType == "LOGICNIM" && (IsLOSRC(targetParamName) || IsLISRC(targetParamName))) //different logic block
                    continue;

                #region Tag reading THIS logicnim (i.e target)
                var targetNode = NodeFactory.CreateOrGetNode(targetTag, targetParamName);
                #endregion

                #region THIS logicnim (i.e source)
                Node? sourceNode = null;
                if (sourceTagAndParam.Contains('.')) //this is this logicnim
                {
                    var pointParam = sourceTagAndParam.Split('.');
                    var sourceParamName = pointParam[1];

                    if (IsFlagOrNumeric(sourceTagAndParam))
                        sourceNode = NodeFactory.CreateOrGetLogicNode(Tag, GetIdFlagOrNumeric(sourceParamName));
                    else if (IsSO(sourceTagAndParam))
                    {
                        var gate = GetGate(sourceParamName); //SO(xx)
                        sourceNode = NodeFactory.CreateOrGetLogicNode(Tag, GetIdSO(gate));
                    }
                    else if (IsDeadbandOrDelay(sourceTagAndParam))
                    {
                        var gate = GetGate(sourceTagAndParam);
                        sourceNode = NodeFactory.CreateOrGetLogicNode(Tag, GetIdSO(gate));
                    }
                    else
                        sourceNode = NodeFactory.CreateOrGetNode(Tag, sourceParamName);

                    if (sourceNode == null)
                        throw new Exception($"Unknown parameter reference on {targetTag}.{targetParamName} = {sourceTagAndParam}");

                    EdgeFactory.AddEdge(sourceNode.Id, $"{sourceParamName} -> {targetParamName}", targetNode.Id);
                }
                else
                    throw new Exception($"Unknown parameter reference on {targetTag}.{targetParamName} = {sourceTagAndParam}");
                #endregion
            }
        }

        /// <summary>
        /// Adds edges to/from components of LOGICNIM
        /// </summary>
        public void AddEdges()
        {
            AddLogicGateEdges();
            TargetReadsFromLogicnim();
            SourceWritesToLogicnim();
        }
        #endregion
    }
}
