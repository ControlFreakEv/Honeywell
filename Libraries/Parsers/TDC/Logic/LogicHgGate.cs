using Honeywell.Parsers.TDC.Logic;
using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Honeywell.Parsers.TDC.Logic.LogicHgTypes;

namespace Honeywell.Parsers.TDC.Logic
{
    public class LogicHgGate
    {
        public string Name { get; set; }
        public int GateIndex { get; set; }
        public int Hwy { get; set; }
        public int Box { get; set; }
        public int Slot { get; set; }
        public LogicHGBlockTypes GateType { get; set; }
        public string? Input1 { get; set; }
        public string? Input2 { get; set; }
        public string? Input3 { get; set; }
        public string? Output1 { get; set; }
        public string? Output2 { get; set; }
        public bool Input1Reverse { get; set; }
        public bool Input2Reverse { get; set; }
        public bool Input3Reverse { get; set; }
        public bool Output1Momentary { get; set; }
        public bool Output2Momentary { get; set; }
        public bool SpecialGate { get; set; } //not or momentary
        public string? Time { get; set; }
        public TdcTag TdcTag { get; set; }


        public LogicHgGate(int hwy, int box, int slot, int gateIndex, LogicHGBlockTypes gateType, string fileName, string gateSuffix = null)
        {
            Hwy = hwy;
            Box = box;
            Slot = slot;
            GateIndex = gateIndex;
            GateType = gateType;

            Name = $"$HY{hwy.ToString("00")}B{box.ToString("00")}.LB({gateIndex.ToString("000")}{gateSuffix})";

            TdcTag = new TdcTag(Name, "LOGICHG", fileName) { HWYNUM = hwy.ToString(), BOXNUM = box.ToString(), SLOTNUM = slot.ToString() };
            TdcTag.Logic = this;
            AddTdcParam("Gate", gateIndex.ToString());
            AddTdcParam("GateType", GateType.ToString());

            Honeywell.Parsers.TDC.Parser.TdcTags.Add(Name, TdcTag);
        }

        public void AddTdcParam(string name, string? value)
        {
            AddTdcParam(TdcTag, name, value);
        }

        private static void AddTdcParam(TdcTag tdcTag, string name, string? value)
        {
            if (value == null)
                return;

            var tdcParam = new TdcParameter(tdcTag, name, value, value);
            tdcTag.Params.Add(tdcParam);

            Honeywell.Parsers.TDC.Parser.TdcParameters.Add(tdcParam);
        }

        public static LogicHgGate? ParseLine(string line, int hwy, int box, int slot, string fileName)
        {
            LogicHgGate? logicHg = null;

            var logicLine = Regex.Replace(line.Trim(), @"\s+", " "); //replace extra spaces
            if (logicLine.Length == 0 || logicLine[..2] == "--")
                return logicHg;

            var logicArray = logicLine.Split(' ');

            var gateIndex = GetIndex(logicArray[0]);
            var gateType = GetLogicType(logicArray[1]);

            logicHg = new(hwy, box, slot, gateIndex, gateType, fileName);

            if (logicHg.GateType == LogicHGBlockTypes.NOP)
            {

            }
            else if (logicHg.GateType == LogicHGBlockTypes.AND || logicHg.GateType == LogicHGBlockTypes.OR || logicHg.GateType == LogicHGBlockTypes.XOR)
            {
                logicHg.Input1 = GetValueFromArray(logicArray, 2);
                logicHg.Input2 = GetValueFromArray(logicArray, 3);
                logicHg.Input3 = GetValueFromArray(logicArray, 4);
            }
            else if (logicHg.GateType == LogicHGBlockTypes.AND_OUT || logicHg.GateType == LogicHGBlockTypes.OR_OUT || logicHg.GateType == LogicHGBlockTypes.XOR_OUT || logicHg.GateType == LogicHGBlockTypes.FLIP_FLOP)
            {
                logicHg.Input1 = GetValueFromArray(logicArray, 2);
                logicHg.Input2 = GetValueFromArray(logicArray, 3);
                logicHg.Output1 = GetValueFromArray(logicArray, 4);
            }
            else if (logicHg.GateType == LogicHGBlockTypes.LINK)
            {
                logicHg.Input1 = GetValueFromArray(logicArray, 2);
                logicHg.Output1 = GetValueFromArray(logicArray, 3);
                logicHg.Output2 = GetValueFromArray(logicArray, 4);

            }
            else if (logicHg.GateType == LogicHGBlockTypes.ON_DELAY || logicHg.GateType == LogicHGBlockTypes.OFF_DELAY || logicHg.GateType == LogicHGBlockTypes.PULSE)
            {
                logicHg.Input1 = GetValueFromArray(logicArray, 2);
                logicHg.Time = GetValueFromArray(logicArray, 3);
            }
            else
            {
                var DEBUG = true;
            }

            logicHg.AddTdcParam("Input1", logicHg.Input1);
            logicHg.AddTdcParam("Input2", logicHg.Input2);
            logicHg.AddTdcParam("Input3", logicHg.Input3);
            logicHg.AddTdcParam("Output1", logicHg.Output1);
            logicHg.AddTdcParam("Output2", logicHg.Output2);
            logicHg.AddTdcParam("Output2", logicHg.Time);

            logicHg.Input1Reverse = (logicHg.Input1 != null && logicHg.Input1[0] == '-');
            logicHg.Input2Reverse = (logicHg.Input2 != null && logicHg.Input2[0] == '-');
            logicHg.Input3Reverse = (logicHg.Input3 != null && logicHg.Input3[0] == '-');
            logicHg.Output1Momentary = (logicHg.Output1 != null && logicHg.Output1[0] != '*');
            logicHg.Output2Momentary = (logicHg.Output2 != null && logicHg.Output2[0] != '*');

            logicHg.Input1 = logicHg.Input1?.Trim('-');
            logicHg.Input2 = logicHg.Input2?.Trim('-');
            logicHg.Input3 = logicHg.Input3?.Trim('-');
            logicHg.Output1 = logicHg.Output1?.Trim('*');
            logicHg.Output2 = logicHg.Output2?.Trim('*');

            return logicHg;
        }

        private static int GetIndex(string name)
        {
            int index;

            var number = name[(name.IndexOf('(') + 1)..name.IndexOf(')')];
            if (!int.TryParse(number, out index))
                index = -1;

            return index;
        }

        private static string? GetValueFromArray(string[] logicArray, int index)
        {
            string? value = null;
            if (logicArray.Length > index)
                value = logicArray[index];
            return value;
        }

        public LogicHgGate AddNotGate(int input)
        {
            LogicHgGate notLogicGate = new (Hwy, Box, Slot, GateIndex, LogicHGBlockTypes.NOT, TdcTag.EbFile, $"-{input}");
            notLogicGate.SpecialGate = true;

            var newInput = $"{notLogicGate.Name}.OUT";
            if (input == 1)
            {
                notLogicGate.Input1 = Input1;
                Input1 = newInput;
                notLogicGate.AddTdcParam("Input1", notLogicGate.Input1);
            }
            else if (input == 2)
            {
                notLogicGate.Input2 = Input2;
                Input2 = newInput;
                notLogicGate.AddTdcParam("Input2", notLogicGate.Input2);
            }
            else if (input == 3)
            {
                notLogicGate.Input3 = Input3;
                Input3 = newInput;
                notLogicGate.AddTdcParam("Input3", notLogicGate.Input3);
            }
            
            return notLogicGate;
        }

        public LogicHgGate AddMomentaryGate(int output)
        {
            LogicHgGate momentaryLogicGate = new(Hwy, Box, Slot, GateIndex, LogicHGBlockTypes.MOMENTARY, TdcTag.EbFile, $"*{output}");
            momentaryLogicGate.SpecialGate = true;

            var newoutput = $"{momentaryLogicGate.Name}.OUT";
            if (output == 1)
            {
                momentaryLogicGate.Output1 = Output1;
                Output1 = newoutput;
                momentaryLogicGate.AddTdcParam("Output1", momentaryLogicGate.Output1);
            }
            else if (output == 2)
            {
                momentaryLogicGate.Output2 = Output2;
                Output2 = newoutput;
                momentaryLogicGate.AddTdcParam("Output2", momentaryLogicGate.Output2);
            }

            return momentaryLogicGate;
        }
    }
}
