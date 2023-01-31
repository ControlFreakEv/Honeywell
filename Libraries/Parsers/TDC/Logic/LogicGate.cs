using Honeywell.TDC;
using Honeywell.Parsers.TDC.Graphs;
using Microsoft.Msagl.Drawing;
using static Honeywell.Parsers.TDC.Logic.LogicNimTypes;

namespace Honeywell.Parsers.TDC.Logic
{
    internal class LogicGate
    {
        public int Gate { get; set; }
        public string SO { get; set; }
        public string ID { get; set; }
        public string Label { get; set; }
        public LogicNimBlockTypes LOGALGID { get; set; }
        public string? R1 { get; set; }
        public string? R2 { get; set; }
        public string? S1 { get; set; }
        public string? S2 { get; set; }
        public string? S3 { get; set; }
        public string? S4 { get; set; }
        public bool S1REV { get; set; }
        public bool S2REV { get; set; }
        public bool S3REV { get; set; }
        public bool S4REV { get; set; }
        public int? DEADBAND { get; set; }
        public int? DLYTIME { get; set; }

        private readonly Logicnim _logicnim;

        public LogicGate(Logicnim logicnim, int gate, LogicNimBlockTypes logalgid, TdcTag tdcTag)
        {
            _logicnim = logicnim;
            ID = $"{logicnim.Name}.SO({gate})";
            Gate = gate;
            LOGALGID = logalgid;
            SO = $"SO{gate}";

            R1 = tdcTag.Params.FirstOrDefault(x => x.Name == $"R1({gate})")?.Value;
            R2 = tdcTag.Params.FirstOrDefault(x => x.Name == $"R2({gate})")?.Value;
            S1 = RemoveUnusedAndOrInputs(tdcTag.Params.FirstOrDefault(x => x.Name == $"S1({gate})")?.Value);
            S2 = RemoveUnusedAndOrInputs(tdcTag.Params.FirstOrDefault(x => x.Name == $"S2({gate})")?.Value);
            S3 = RemoveUnusedAndOrInputs(tdcTag.Params.FirstOrDefault(x => x.Name == $"S3({gate})")?.Value);
            S4 = tdcTag.Params.FirstOrDefault(x => x.Name == $"S4({gate})")?.Value;

            var s1rev = tdcTag.Params.FirstOrDefault(x => x.Name == $"S1REV({gate})")?.Value;
            var s2rev = tdcTag.Params.FirstOrDefault(x => x.Name == $"S2REV({gate})")?.Value;
            var s3rev = tdcTag.Params.FirstOrDefault(x => x.Name == $"S3REV({gate})")?.Value;
            var s4rev = tdcTag.Params.FirstOrDefault(x => x.Name == $"S4REV({gate})")?.Value;
            var deadband = tdcTag.Params.FirstOrDefault(x => x.Name == $"DEADBAND({gate})")?.Value;
            var dlytime = tdcTag.Params.FirstOrDefault(x => x.Name == $"DLYTIME({gate})")?.Value;

            if (bool.TryParse(s1rev, out bool _s1rev))
                if (S1REV = _s1rev)
                    NodeFactory.CreateOrGetLogicNode(logicnim.Tag, Logicnim.GetNotId(tdcTag.Name, "S1", gate));
            if (bool.TryParse(s2rev, out bool _s2rev))
                if (S2REV = _s2rev)
                    NodeFactory.CreateOrGetLogicNode(logicnim.Tag, Logicnim.GetNotId(tdcTag.Name, "S2", gate));
            if (bool.TryParse(s3rev, out bool _s3rev))
                if (S3REV = _s3rev)
                    NodeFactory.CreateOrGetLogicNode(logicnim.Tag, Logicnim.GetNotId(tdcTag.Name, "S3", gate));
            if (bool.TryParse(s4rev, out bool _s4rev))
                if (S4REV = _s4rev)
                    NodeFactory.CreateOrGetLogicNode(logicnim.Tag, Logicnim.GetNotId(tdcTag.Name, "S4", gate));

            if (int.TryParse(deadband, out int _deadband))
                DEADBAND = _deadband;

            if (int.TryParse(dlytime, out int _dlytime))
                DLYTIME = _dlytime;

            Label = GetLabel(logicnim.Name, logalgid, gate);
            if (DEADBAND != null && DEADBAND != 0)
                Label += $"\nDeadband = {DEADBAND}";
            if (DLYTIME != null && DLYTIME != 0)
                Label += $"\nDelay Time = {DLYTIME}";

            CreateNode(S1);
            CreateNode(S2);
            CreateNode(S3);
            CreateNode(S4);
            CreateNode(R1);
            CreateNode(R2);
        }

        private void CreateNode(string? input)
        {
            if (input != null)
            {
                if (Logicnim.IsFlagOrNumeric(input))
                    NodeFactory.CreateOrGetLogicNode(_logicnim.Tag, Logicnim.GetGateInputId(input, Gate), input);
                else
                {
                    var tagParam = input.Split('.');
                    string tag = tagParam[0];
                    string parameter = tagParam[1];
                    NodeFactory.CreateOrGetMissingNode(_logicnim.Tag, tag);
                }
            }
        }

        private static string GetLabel(string name, LogicNimBlockTypes logalgid, int gate)
        {
            return $"{name}.{Enum.GetName(typeof(LogicNimBlockTypes), logalgid)}{gate}";
        }

        public static Node? CreateNullGate(string? id)
        {
            if (id == null)
                return null;

            Node? node = null;
            if (Logicnim.IsSO(id))
            {
                var tagParam = id.Split('.');
                var tagName = tagParam[0];
                var parameter = tagParam[1];
                var gate = Logicnim.GetGate(id);

                Parser.TdcTags.TryGetValue(tagName, out TdcTag? tdcTag);
                var label = GetLabel(tagName, LogicNimBlockTypes.NULL, gate);
                node = NodeFactory.CreateOrGetLogicNode(tdcTag, id, label);
            }
            
            return node;
        }

        private string? RemoveUnusedAndOrInputs(string? input)
        {
            if (Logicnim.IsOff(input) && LOGALGID == LogicNimBlockTypes.OR)
                return null;
            else if (Logicnim.IsOn(input) && LOGALGID == LogicNimBlockTypes.AND)
                return null;

            return input;
        }
    }
}
