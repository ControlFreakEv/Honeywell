using System.Globalization;
using System.Text.RegularExpressions;
using Honeywell.TDC;
using Honeywell.Parsers.TDC.Logic;

namespace Honeywell.Parsers.TDC
{
    public static partial class Parser
    {
        private static readonly Regex regexEb = new(@"&(T|X|M)\s(?<pointType>\S+)\s&N\s(?<name>\S+)\s((?<param>\S+)\s=\s(?<value>("".*?""|\S+))\s?)+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex regexEbComments = new(@"\{.*\}", RegexOptions.Compiled);
        private static readonly Regex regexEbQuotes = new("(“|”)", RegexOptions.Compiled);
        private static readonly Regex regexEbNewLine = new("(\n|\r)", RegexOptions.Compiled);
        private static readonly Regex regexEbSpace = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex regexEbParameterSpace = new(@"\s*=\s*", RegexOptions.Compiled);
        private static readonly Regex regexLeadingZero = new(@"\(0+", RegexOptions.Compiled);


        /// <summary>Flattens an EB file onto one line with uniform quotes and spacing.</summary>
        private static string FormatEbFiles(string ebFileContents)
        {
            ebFileContents = regexEbComments.Replace(ebFileContents, ""); //removes comments e.g. {ignore}
            ebFileContents = regexEbQuotes.Replace(ebFileContents, @""""); //makes all double quotes the same
            ebFileContents = regexEbNewLine.Replace(ebFileContents, @" "); //remove carraige return and new lines (could be within quotations)
            ebFileContents = regexEbSpace.Replace(ebFileContents, @" "); //remove extra space 
            ebFileContents = regexEbParameterSpace.Replace(ebFileContents, @" = ").Trim(); //remove extra space between param and value
            return ebFileContents;
        }

        /// <summary>A parameter's value without quotes or placeholders, with numbers in a standard form.</summary>
        private static string? CleanValue(string rawValue)
        {
            var value = rawValue.Trim('"').Trim();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            foreach (var filler in new[] { '-', '_' }) // ------- or --.--
                if (value.Contains(filler) && (string.IsNullOrWhiteSpace(value.Trim(filler)) || value.Trim(filler) == "."))
                    return null;

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                return number.ToString(CultureInfo.InvariantCulture); // removes trailing 0's

            return value;
        }

        private static void EbParseGroups(string ebFileContents, string ebFileName, List<TdcGroup> groups)
        {
            foreach (Match match in regexEb.Matches(FormatEbFiles(ebFileContents)))
            {
                if (!match.Groups["pointType"].Value.Trim().ToUpper().Contains("GROUP"))
                    continue;

                var group = new TdcGroup { EbFile = ebFileName };
                var names = match.Groups["param"].Captures;
                var values = match.Groups["value"].Captures;
                for (int i = 0; i < names.Count; i++)
                {
                    var value = CleanValue(values[i].Value);
                    if (value != null) // a blank value leaves the group's default
                        SetGroupValue(group, names[i].Value.ToUpper(), value);
                }
                groups.Add(group);
            }
        }

        private static void SetGroupValue(TdcGroup group, string name, string value)
        {
            int index = 0;
            if (name.Contains('('))
                int.TryParse(name[(name.IndexOf('(') + 1)..name.IndexOf(')')], out index);

            if (name == "OENTNUMP")
                group.OENTNUMP = int.Parse(value);
            else if (name == "TITLE")
                group.TITLE = value;
            else if (name.Contains("EXTIDLST"))
                group.EXTIDLST[index - TdcGroup.OffsetEXTIDLST] = value;
            else if (name.Contains("TRNDSET"))
                group.TRNDSET[index - TdcGroup.OffsetTRNDSET] = value;
            else if (name == "TRNDBASE")
                group.TRNDBASE = value;
            else if (name.Contains("SCREENUM"))
                group.SCREENUM[index - TdcGroup.OffsetSCREENUM] = value;
            else if (name.Contains("DISPID"))
                group.DISPID[index - TdcGroup.OffsetDISPID] = value;
        }

        private static void EbParseContents(string ebFileContents, string ebFileName)
        {
            foreach (Match match in regexEb.Matches(FormatEbFiles(ebFileContents)))
            {
                var tagname = match.Groups["name"].Value.Trim().ToUpper();
                var pointType = match.Groups["pointType"].Value.Trim().ToUpper();
                if (pointType.Contains("GROUP") || pointType == "HM_HIST")
                    continue;

                if (TdcTags.TryGetValue(tagname, out TdcTag? tag))
                    tag.EbFile += $"\n{ebFileName}";
                else
                    tag = new TdcTag(tagname, pointType, ebFileName);

                var names = match.Groups["param"].Captures;
                var values = match.Groups["value"].Captures;
                for (int i = 0; i < names.Count; i++)
                    ReadParameter(tag, WithoutLeadingZeros(names[i].Value.ToUpper()), values[i].Value);

                TdcTags.TryAdd(tag.Name, tag);
            }
        }

        /// <summary>FL(001) becomes FL(1).</summary>
        private static string WithoutLeadingZeros(string name)
        {
            if (!regexLeadingZero.IsMatch(name))
                return name;

            var start = name.IndexOf('(');
            var index = name[(start + 1)..name.IndexOf(')')];
            return index == "0" ? name : $"{name[..start]}({int.Parse(index)})";
        }

        /// <summary>The first non-blank value for a parameter wins when a tag appears in more than one EB file.</summary>
        private static void ReadParameter(TdcTag tag, string name, string rawValue)
        {
            var value = CleanValue(rawValue);
            var existing = tag.Params.FirstOrDefault(x => x.Name == name);
            if (existing == null)
            {
                var parameter = new TdcParameter(tag, name, value, rawValue);
                tag.Params.Add(parameter);
                TdcParameters.Add(parameter);
            }
            else if (value != null && string.IsNullOrWhiteSpace(existing.Value))
            {
                existing.Value = value;
                existing.RawValue = rawValue;
            }
            else
            {
                return;
            }

            MapKnownParameter(tag, name, value);

            if (value != null && name.Contains("PKGNAME"))
            {
                if (!tag.PackageExists)
                {
                    tag.PackageExists = true;
                    tag.Packages = new List<string>();
                }
                tag.Packages.Add(value.ToUpper().Trim('"').Trim());
            }
            else if (value != null && name.Contains("BLKNAME"))
            {
                AddBlock(tag, value.ToUpper().Trim());
            }
        }

        private static void AddBlock(TdcTag tag, string block)
        {
            if (!tag.BlockExists)
            {
                tag.BlockExists = true;
                tag.Blocks = new List<string>();
            }
            tag.Blocks.Add(block);
        }

        /// <summary>Parameters used often enough to have their own property on the tag.</summary>
        private static void MapKnownParameter(TdcTag tag, string name, string? value)
        {
            switch (name)
            {
                case "PTDESC": tag.Desc = value; break;
                case "NTWKNUM": tag.NTWKNUM = value; break;
                case "NODENUM": tag.NODENUM = value; break;
                case "SLOTNUM": tag.SLOTNUM = value; break;
                case "MODNUM": tag.MODNUM = value; break;
                case "NODETYP": tag.NODETYP = value; break;
                case "PVALGID": tag.PVALGID = value; break;
                case "CTLALGID": tag.CTLALGID = value; break;
                case "HWYNUM": tag.HWYNUM = value; break;
                case "BOXNUM": tag.BOXNUM = value; break;
                case "NTWKNUM-AM": tag.AM = value; break;
                case "INPTSSLT": tag.INPTSSLT = value; break;
                case "INTVARNM": tag.INTVARNM = value; break;
                case "OUTBOXNM": tag.OUTBOXNM = value; break;
                case "OUTSLTNM": tag.OUTSLTNM = value; break;
                case "OUTSSLT": tag.OUTSSLT = value; break;
                case "ALGIDDAC": tag.ALGIDDAC = value; break;
                case "PNTBOXTY": tag.PNTBOXTY = value; break;
            }
        }

        /// <summary>Adds the blocks listed in a CL slots report that the EB files didn't mention.</summary>
        private static void ApplyClSlots(List<CLSlot> clSlots)
        {
            foreach (var slot in clSlots)
            {
                if (!TdcTags.TryGetValue(slot.Tag, out TdcTag? tag) || tag.Params.Any(x => x.Value == slot.Block && x.Name.Contains("BLKNAME")))
                    continue;

                var nextIndex = 0;
                if (tag.BlockExists)
                {
                    var lastBlock = tag.Params.Where(x => x.Name.Contains("BLKNAME")).OrderBy(x => x.Name).Last().Name;
                    nextIndex = int.Parse(lastBlock[8..^1]) + 1;
                }

                AddParameter(tag, $"BLKNAME({nextIndex})", slot.Block);
                AddBlock(tag, slot.Block);
            }
        }

        /// <summary>Follows references such as LISRC(1) = LOGIC.LISRC(2) to the value they lead to.</summary>
        private static void ResolveSourceParameter(TdcParameter parameter)
        {
            while (parameter.Value != null && (Logicnim.IsLISRC(parameter.Value) || Logicnim.IsLOSRC(parameter.Value) || Logicnim.IsLOENBL(parameter.Value)))
            {
                var parts = parameter.Value.Split('.');
                TdcTags.TryGetValue(parts[0], out TdcTag? connectedTag);
                parameter.Value = connectedTag?.Params.FirstOrDefault(x => x.Name == parts[1])?.Value;
            }
        }
    }
}
