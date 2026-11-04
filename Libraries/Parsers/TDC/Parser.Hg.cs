using Honeywell.TDC;
using Honeywell.Parsers.TDC.Logic;

namespace Honeywell.Parsers.TDC
{
    public static partial class Parser
    {
        /// <summary>Connects each regulatory and digital composite HG point to the IO points at its address.</summary>
        private static void AddRegDigHgConnections()
        {
            foreach (var parent in TdcTags.Values.Where(x => x.PointType == "REGHG").ToList())
            {
                var input = FindTag(x => x.PointType == "ANLINHG" && x.HWYNUM == parent.HWYNUM && x.BOXNUM == parent.BOXNUM && x.SLOTNUM == parent.SLOTNUM && x.INPTSSLT == null);
                if (input != null)
                    AddParameter(parent, "HGSRC(1)", $"{input.Name}.PV");

                var output = FindTag(x => x.PointType == "ANLOUTHG" && x.HWYNUM == parent.HWYNUM && x.OUTBOXNM == parent.BOXNUM && x.OUTSLTNM == parent.SLOTNUM && x.OUTSSLT == null);
                if (output != null)
                    AddParameter(parent, "HGDSTN(1)", $"{output.Name}.OP");
            }

            foreach (var parent in TdcTags.Values.Where(x => x.PointType == "DIGCMPHG").ToList())
            {
                if (parent.INPTSSLT != null && int.TryParse(parent.Params.FirstOrDefault(x => x.Name == "NMBRINPT")?.Value, out int inputCount))
                {
                    for (int i = 0; i < inputCount; i++)
                    {
                        var channel = (int.Parse(parent.INPTSSLT) + i).ToString();
                        var input = FindTag(x => x.PointType == "DIGINHG" && x.HWYNUM == parent.HWYNUM && x.BOXNUM == parent.BOXNUM && x.SLOTNUM == parent.SLOTNUM && x.INPTSSLT == channel);
                        if (input != null)
                            AddParameter(parent, $"HGSRC({i + 1})", $"{input.Name}.PVFL");
                    }
                }

                if (parent.OUTSSLT != null && int.TryParse(parent.Params.FirstOrDefault(x => x.Name == "NMBROUT")?.Value, out int outputCount))
                {
                    for (int i = 0; i < outputCount; i++)
                    {
                        var channel = (int.Parse(parent.OUTSSLT) + i).ToString();
                        var output = FindTag(x => x.PointType == "DIGOUTHG" && x.HWYNUM == parent.HWYNUM && x.OUTBOXNM == parent.OUTBOXNM && x.OUTSLTNM == parent.OUTSLTNM && x.OUTSSLT == channel);
                        if (output != null)
                            AddParameter(parent, $"HGDSTN({i + 1})", $"{output.Name}.OP");
                    }
                }
            }
        }

        private static void AddConnectionsToLogicHg(LogicHgGate hgLogic, string? logicConnectionName, string paramName)
        {
            if (logicConnectionName == null)
                return;

            var match = regexClHgTagParameter.Match(logicConnectionName);
            if (match.Success) //indirect box/slot reference
            {
                var param = logicConnectionName.Split('.').LastOrDefault();
                var hwy = hgLogic.Hwy.ToString();
                var box = hgLogic.Box.ToString();

                (TdcTag? tag, string? hgParam) = ParseHgBoxAddress(match, hwy, box);
                if (tag == null)
                { 
                    hwy = hwy.Length > 1 ? hwy : "0" + hwy;
                    box = box.Length > 1 ? box : "0" + box;

                    var boxParam = match.Value;
                    if(match.Value.Contains('.'))
                        boxParam = match.Value.Split('.')[1];

                    var boxTagname = $"$HY{hwy}B{box}.{boxParam}".ToUpper();
                    if (!TdcTags.TryGetValue(boxTagname, out tag))
                    {
                        tag = AddBoxInternalTag(boxTagname, hgLogic.TdcTag.EbFile, hwy, box);
                    }
                }
                hgLogic.AddTdcParam(paramName, $"{tag.Name}.{hgParam ?? param}");
            }
            else //references tag in TDC dict
                hgLogic.AddTdcParam(paramName, logicConnectionName);
        }

        /// <summary>Reads a logic block file. The name holds its address: LB, highway, box and slot, as in LB010901.js.</summary>
        public static void JSParseContent(string[] fileContents, string fileName, List<LogicHgGate> hgLogicList)
        {
            var hwy = int.Parse(fileName[2..4]);
            var box = int.Parse(fileName[4..6]);
            var slot = int.Parse(fileName[6..8]);
            foreach (var line in fileContents)
            {
                var lb = LogicHgGate.ParseLine(line, hwy, box, slot, fileName);
                if (lb != null)
                {
                    hgLogicList.Add(lb);

                    if (lb.Input1Reverse)
                        hgLogicList.Add(lb.AddNotGate(1));

                    if (lb.Input2Reverse)
                        hgLogicList.Add(lb.AddNotGate(2));

                    if (lb.Input3Reverse)
                        hgLogicList.Add(lb.AddNotGate(3));

                    if (lb.Output1Momentary)
                        hgLogicList.Add(lb.AddMomentaryGate(1));

                    if (lb.Output2Momentary)
                        hgLogicList.Add(lb.AddMomentaryGate(2));
                }
            }
        }
    }
}
