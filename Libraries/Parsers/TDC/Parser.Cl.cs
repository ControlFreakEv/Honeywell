using System.Text.RegularExpressions;
using Honeywell.TDC;
using Honeywell.Parsers.TDC.Logic;

namespace Honeywell.Parsers.TDC
{
    public static partial class Parser
    {
        private static readonly Regex regexClExternal = new(@"(?<!(--.*))\bEXTERNAL\b[^\S\r\n]+((?<tag>('|\$)?\w+(\\\w+)?)(([^\S\r\n]*,[^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexClLocal = new(@"(?<!(--.*))\bLOCAL\b\s+((?<name>\w+)(\s+AT\s+(?<type>\w+(\(\d+\))?))?((\s*,[^\S\r\n]*(--.*)?((\n*|\r*)+\s*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+(\s*:\s*('|\$)?(?<type>.*))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexClTag = new(@"(?<!(--.*))(?<=^([^""]|""[^""]*"")*)\bPOINT\b\s*(?<tag>\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled); //not in comment and not in quotes
        private static readonly Regex regexClName = new Regex(@"(?<!(--.*))((\bBLOCK\b|\bSEQUENCE\b)\s+(?<label>\w+))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex regexClEnum = new(@"(?<!(--.*))(\bENUMERATION\b\s*(?<name>\w+)\s*=\s*(?<value>[\w\d/'$]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex regexClParamList = new(@"((?<!(--.*))(\bPARAM_LIST\b\s*(?<paramlist>\w+)).*[\s\r\n]+)?(?<!(--.*))\bPARAMETER\b[^\S\r\n]+((?<parameter>\w+)(([^\S\r\n]*,[^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+(\s*:\s*?(?<type>('|\$)?.*))?([\s\r\n]+\bEND\b\s+(?<paramlistEnd>\w+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexClHgTagParameter = new(@"(?<!(--.*))((\$HY(?<HWY>\d+)B(?<Box>\d+)\.)?\b(?<parameter>NN|FL|LB|TM|LP|AI|DI|DO|CI)\b\((?<index>\d+)\))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline); //todo: AO/DO?

        //check CDS for $reg_ctl, these are parameter connects to packaged point
        private static void CLParseContent(string clFileContents, string clFileName)
        {
            var clNameMatch = regexClName.Match(clFileContents);
            var clName = clNameMatch.Success ? clNameMatch.Groups["label"].Value.ToUpper() : null;

            var cl = new TdcCL(clFileName, clName, clFileContents);
            TdcCL.Add(cl);

            #region Tag CL is attached to
            var pointMatch = regexClTag.Match(cl.Content);
            if (pointMatch.Success)
            {
                var tagname = pointMatch.Groups["tag"].Value.Trim().ToUpper();
                cl.TagsCLIsAttachedTo.Add(tagname);

                #region ProcModHG
                if (TdcTags.TryGetValue(tagname, out TdcTag? procModHg) && procModHg.PointType == "PRCMODHG")
                {
                    var newExternalTags = new List<string>();
                    var hgMatches = regexClHgTagParameter.Matches(cl.Content);
                    for (int i = hgMatches.Count - 1; i >= 0; i--)
                    {
                        var match = hgMatches[i];
                        if (!match.Success)
                            continue;

                        var hwyText = match.Groups["HWY"].Success ? match.Groups["HWY"].Value : procModHg.HWYNUM;
                        var boxText = match.Groups["Box"].Success ? match.Groups["Box"].Value : procModHg.BOXNUM;
                        if (hwyText == null || boxText == null) //no box address to resolve this reference against
                            continue;

                        var hwy = int.Parse(hwyText).ToString();
                        var box = int.Parse(boxText).ToString();
                        (TdcTag? tag, string? tagParam) = ParseHgBoxAddress(match, hwy, box);

                        hwy = hwy.Length > 1 ? hwy : "0" + hwy;
                        box = box.Length > 1 ? box : "0" + box;
                        var boxTag = $"$HY{hwy}B{box}.";

                        if (tag != null)
                            AddCL(cl, tag.Name);
                        else
                        {
                            var boxParam = match.Value;
                            if (match.Value.Contains('.'))
                                boxParam = match.Value.Split('.')[1];

                            var boxTagname = $"$HY{hwy}B{box}.{boxParam}".ToUpper();
                            if (TdcTags.TryGetValue(boxTagname, out TdcTag? boxTdcTag))
                            {
                                if (!boxTdcTag.CL.Any(x => x.FileName == clFileName))
                                    AddCL(cl, boxTagname);
                            }
                            else
                            {
                                AddBoxInternalTag(boxTagname, clFileName, hwy, box);
                                AddCL(cl, boxTagname);
                            }
                        }

                        if (tagParam != null)
                            tagParam = $".{tagParam} ";

                        if (tag != null)
                        {
                            //                                                                                                 grab evertything berfore this/     /and everything after this
                            cl.Content = cl.Content[..match.Index] + tag.Name + tagParam + cl.Content[(match.Index + match.Length)..]; //e.g. replace Read FL(123) From AC452.BADPVFL

                            var externalTagname = tag.Name;
                            if (externalTagname.Contains('.'))
                                externalTagname = externalTagname.Split('.')[0]; // '.' breaks external regex

                            if (!newExternalTags.Contains(externalTagname))
                                newExternalTags.Add(externalTagname);
                        }
                        else if (!match.Groups["HWY"].Success)
                        {
                            //                                                                                     grab everything before this/and everything after this                                      
                            cl.Content = cl.Content[..match.Index] + boxTag + cl.Content[match.Index..]; // e.g.replace Read FL(123) From AC452.BADPVFL

                            if (!newExternalTags.Contains(boxTag.Trim('.'))) 
                                newExternalTags.Add(boxTag.Trim('.'));
                        }
                    }

                    if (newExternalTags.Any())
                    {
                        var externalStatement = Environment.NewLine + "\tEXTERNAL " + string.Join(", ", newExternalTags) + " --added by Parser";
                        var externalIndex = cl.Content.IndexOf(Environment.NewLine, pointMatch.Index + pointMatch.Length);
                        cl.Content = cl.Content.Insert(externalIndex, externalStatement);
                    }
                }
                #endregion

                AddCL(cl, tagname);
            }
            else
            {
                var packageName = clFileName.Replace(".CL", null);
                var blockAndPackageTags = TdcTags.Values.Where(x => (x.BlockExists && clName != null && x.Blocks.Contains(clName)) || (x.PackageExists && x.Packages.Contains(packageName)));
                foreach (var tag in blockAndPackageTags)
                {
                    cl.TagsCLIsAttachedTo.Add(tag.Name);
                    AddCL(cl, tag.Name);
                }
            }
            #endregion

            #region External Refs
            MatchCollection matchCollection = regexClExternal.Matches(cl.Content);
            foreach (Match match in matchCollection)
            {
                var tagGroup = match.Groups["tag"];
                foreach (Capture capture in tagGroup.Captures)
                {
                    var tagname = capture.Value.Trim().ToUpper();
                    AddCL(cl, tagname);
                }
               
            }
            #endregion

            FindCDS(cl);
        }

        private static TdcTag AddBoxInternalTag(string boxTagname, string file, string hwy, string box)
        {
            var tag = new TdcTag(boxTagname, "BOX (Internal)", file) { HWYNUM = hwy.TrimStart('0'), BOXNUM = box.TrimStart('0') };
            TdcTags.Add(boxTagname, tag);
            AddParameter(tag, "HWYNUM", tag.HWYNUM);
            AddParameter(tag, "BOXNUM", tag.BOXNUM);
            return tag;
        }

        /// <summary>
        /// A PARAMETER of a custom type holds the name of another tag, set per tag in custom data segments.
        /// Each one found connects this CL to that tag.
        /// </summary>
        private static void FindCDS(TdcCL cl)
        {
            foreach (Match match in regexClParamList.Matches(cl.Content))
            {
                var typeCaptures = match.Groups["type"].Captures;
                if (!typeCaptures.Any()) // no type means a number
                    continue;
                if (typeCaptures.Count > 1)
                    throw new Exception("Multiple types not accounted for");

                var paramType = typeCaptures.First().Value.ToUpper();
                if (paramType.Contains('"'))
                    paramType = paramType[..paramType.IndexOf('"')].Trim();

                var holdsTagName = paramType == "$REG_CTL" || (paramType.IndexOf("ARRAY") != 0 && !paramType.Contains("NUMBER") && !paramType.Contains("TIME") && !paramType.Contains("LOGICAL") && !paramType.Contains("STRING") && !paramType.Contains('/'));
                if (!holdsTagName)
                    continue;

                var names = ParameterNames(match.Groups["parameter"].Captures, paramType).ToList();
                foreach (var tagname in cl.TagsCLIsAttachedTo)
                {
                    if (!TdcTags.TryGetValue(tagname, out TdcTag? tag))
                        continue;

                    foreach (var name in names)
                    {
                        var value = tag.Params.FirstOrDefault(x => x.Name == name);
                        if (value?.Value != null && TdcTags.TryGetValue(value.Value, out TdcTag? cdsTag))
                        {
                            value.CdsTag = cdsTag;
                            AddCL(cl, cdsTag.Name);
                        }
                    }
                }
            }
        }

        /// <summary>The declared names, expanded to NAME(low) to NAME(high) when the type is an array.</summary>
        private static IEnumerable<string> ParameterNames(CaptureCollection declared, string paramType)
        {
            var arrayIndex = paramType.IndexOf("ARRAY");
            foreach (Capture capture in declared)
            {
                var name = capture.Value.ToUpper();
                if (arrayIndex < 0)
                {
                    yield return name;
                    continue;
                }

                var bounds = paramType[(paramType.IndexOf('(', arrayIndex) + 1)..paramType.IndexOf(')', arrayIndex)].Split("..");
                for (int i = int.Parse(bounds[0]); i <= int.Parse(bounds[1]); i++)
                    yield return $"{name}({i})";
            }
        }

        private static TdcTag? FindTag(Func<TdcTag, bool> predicate) => TdcTags.Values.FirstOrDefault(predicate);

        /// <summary>Decodes a highway gateway box reference such as $HY01B09.NN(123) to the tag at that address.</summary>
        private static (TdcTag?, string?) ParseHgBoxAddress(Match match, string defaultHwy, string defaultBox)
        {
            var hwy = int.Parse(match.Groups["HWY"].Success ? match.Groups["HWY"].Value : defaultHwy).ToString();
            var box = int.Parse(match.Groups["Box"].Success ? match.Groups["Box"].Value : defaultBox).ToString();
            var param = match.Groups["parameter"].Value.ToUpper();
            var index = int.Parse(match.Groups["index"].Value);

            TdcTag? tag = null;
            string? tagParam = null;
            if (param == "NN") //numeric
            {
                tag = FindTag(x => x.PointType == "NUMERCHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.INTVARNM == index.ToString());
                tagParam = "PV";
            }
            else if (param == "FL") //flag
            {
                tag = FindTag(x => x.PointType == "FLAGHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.INTVARNM == index.ToString());
                tagParam = "PVFL";
            }
            else if (param == "TM") //timer
                tag = FindTag(x => x.PointType == "TIMERHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.INTVARNM == index.ToString());
            else if (param == "LP") //regctl
                tag = FindTag(x => x.PointType == "REGHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == index.ToString());
            else if (param == "LB") //logicblock (.js file)
            {
                tag = FindTag(x => x.PointType == "LOGICHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.Params.FirstOrDefault(x => x.Name == "Gate")?.Value == index.ToString() && x.Logic is LogicHgGate { SpecialGate: false });
                tagParam = "OUT";
            }
            else if (param == "AI") //analog input
            {
                if (index <= 8)
                    tag = FindTag(x => x.PointType == "ANLINHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == "7" && x.INPTSSLT == index.ToString());
                else
                    tag = FindTag(x => x.PointType == "ANLINHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == "8" && x.INPTSSLT == (index - 8).ToString());
            }
            else if (param == "DI") //dig in
            {
                var channel = int.Parse(index.ToString()[^2..]).ToString();
                var slot = int.Parse(index.ToString()[..^2]).ToString();
                tag = FindTag(x => x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == slot && x.INPTSSLT == channel);
            }
            else if (param == "DO") //dig out
            {
                var channel = int.Parse(index.ToString()[^1..]).ToString();
                var slot = int.Parse(index.ToString()[..^1]).ToString();
                tag = FindTag(x => x.HWYNUM == hwy && x.OUTBOXNM == box && x.OUTSLTNM == slot && x.OUTSSLT == channel);
            }
            else if (param == "CI") //counter
            {
                var channel = int.Parse(index.ToString()[^1..]).ToString();
                var slot = int.Parse(index.ToString()[..^1]).ToString();
                tag = FindTag(x => x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == slot && x.INPTSSLT == channel);
            }

            return (tag, tagParam);
        }

        private static void AddCL(TdcCL? cl, string tagname)
        {
            if (cl == null)
                return;

            if (TdcTags.TryGetValue(tagname, out TdcTag? tag))
            {
                if (!tag.CL.Any(x => x.Id == cl.Id))
                    tag.CL.Add(cl);

                if (!cl.TdcTags.Any(x => x.Id == tag.Id))
                    cl.TdcTags.Add(tag);
            }
        }
    }
}
