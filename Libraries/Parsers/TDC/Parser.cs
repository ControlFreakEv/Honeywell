using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using Honeywell.Parsers.TDC.Logic;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Honeywell.Parsers.TDC.Graphs;
using Honeywell.TDC;
using System.Data;
using System.Data.OleDb;
using Honeywell.Parsers.TDC.Logic;
using Honeywell.Database;
using Honeywell.Parsers.TDC;
//using Microsoft.VisualBasic.FileIO;
//using NPOI.SS.Formula.Functions;
//using Database;

namespace Honeywell.Parsers.TDC
{
    //                                                                                  FFL      Point    Parameter
    //todo: parse FFL.ZF file for free format refs (separated by spaces) e.g. NET>FFL   ANALRPT3 UY5825RP.Y_TIME(15) 
    //todo: parse LVR log
    //todo: am node should be inferred from eb file name unless file is named "FILE_xxx".
    //todo: break up devctl
    //todo: break up array
    public static class Parser
    {
        public static List<TdcModule> Modules { get; set; } = new();
        public static List<TdcParameter> TdcParameters { get; set; } = new(100000);
        public static List<TdcCL> TdcCL { get; set; } = new(100);
        public static Dictionary<string, TdcTag> TdcTags { get; set; } = new();

        private static readonly Regex regexEb = new(@"&(T|X|M)\s(?<pointType>\S+)\s&N\s(?<name>\S+)\s((?<param>\S+)\s=\s(?<value>("".*?""|\S+))\s?)+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private static readonly Regex regexClExternal = new(@"(?<!(--.*))\bEXTERNAL\b[^\S\r\n]+((?<tag>('|\$)?\w+(\\\w+)?)(([^\S\r\n]*,[^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexClLocal = new(@"(?<!(--.*))\bLOCAL\b\s+((?<name>\w+)(\s+AT\s+(?<type>\w+(\(\d+\))?))?((\s*,[^\S\r\n]*(--.*)?((\n*|\r*)+\s*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+(\s*:\s*('|\$)?(?<type>.*))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexClTag = new(@"(?<!(--.*))(?<=^([^""]|""[^""]*"")*)\bPOINT\b\s*(?<tag>\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled); //not in comment and not in quotes
        private static readonly Regex regexClName = new Regex(@"(?<!(--.*))((\bBLOCK\b|\bSEQUENCE\b)\s+(?<label>\w+))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex regexClEnum = new(@"(?<!(--.*))(\bENUMERATION\b\s*(?<name>\w+)\s*=\s*(?<value>[\w\d/'$]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex regexClParamList = new(@"((?<!(--.*))(\bPARAM_LIST\b\s*(?<paramlist>\w+)).*[\s\r\n]+)?(?<!(--.*))\bPARAMETER\b[^\S\r\n]+((?<parameter>\w+)(([^\S\r\n]*,[^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+(\s*:\s*?(?<type>('|\$)?.*))?([\s\r\n]+\bEND\b\s+(?<paramlistEnd>\w+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexClHgTagParameter = new(@"(?<!(--.*))((\$HY(?<HWY>\d+)B(?<Box>\d+)\.)?\b(?<parameter>NN|FL|LB|TM|LP|AI|DI|DO|CI)\b\((?<index>\d+)\))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline); //todo: AO/DO?

        private static readonly Regex regexEbComments = new(@"\{.*\}", RegexOptions.Compiled);
        private static readonly Regex regexEbQuotes = new("(“|”)", RegexOptions.Compiled);
        private static readonly Regex regexEbNewLine = new("(\n|\r)", RegexOptions.Compiled);
        private static readonly Regex regexEbSpace = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex regexEbParameterSpace = new(@"\s*=\s*", RegexOptions.Compiled);
        private static readonly Regex regexLeadingZero = new(@"\(0+", RegexOptions.Compiled);

        private static readonly Regex regexJs = new(@"(?<!(--.*))()", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        public static string GroupFilePath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Groups.xlsx");
        public static string ModulesFilePath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Modules.xlsx");
        public static string D3kEbFilePath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Mapper.eb");

        //may need to repair "Microsoft Access Database Engine 2010" in add/remove programs
        public static bool AddCdsFromD3kQuery(string password) //Z90
        {
            try
            {
                var connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\ebmiller\Desktop\101.mdb;Jet OLEDB:Database Password={password};";
                //SELECT [cds data].Entity, [cds parameters].Parameter, [cds data].Value FROM[cds parameters] INNER JOIN[cds data] ON[cds parameters].ID = [cds data].[CDS Parameter ID];

                using var conection = new OleDbConnection(connectionString);
                conection.Open();

                var testQuery = "SELECT [cds data].Entity, [cds parameters].Parameter, [cds data].Value FROM[cds parameters] INNER JOIN[cds data] ON [cds parameters].ID = [cds data].[CDS Parameter ID];";
                using var tagCDSDa = new OleDbDataAdapter(testQuery, conection);
                var tagCDS = new DataTable();
                tagCDSDa.Fill(tagCDS);

                //MessageBox.Show(password);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static void ImportFileRefsFromD3k(string filePath)
        {
            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Persist Security Info=False;";
            //connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};User Id=admin;Password=admin;";

            using var conection = new OleDbConnection(connectionString);
            conection.Open();

            var query = "Select [Name], [FFL References], [Group Display References], [Schematic References] From Tags"; //, [CL], [LinksCL]
            using var tagDa = new OleDbDataAdapter(query, conection);
            var tagDt = new DataTable();
            tagDa.Fill(tagDt);

            using var db = new TdcContext();
            var tags = db.TdcTags.ToList();

            var id = 0;
            var fileRefs = new List<DbTdcFileRef>();
            foreach (DataRow dr in tagDt.Rows)
            {
                var tagName = dr["Name"].ToString();
                var tagId = tags.FirstOrDefault(x => x.Name == tagName)?.Id;
                if (tagId == null)
                    continue;

                var fflAll = dr["FFL References"].ToString();
                var groupAll = dr["Group Display References"].ToString();
                var gfxAll = dr["Schematic References"].ToString();
                //var clAll = dr["CL"].ToString();
                //var linksCLAll = dr["LinksCL"].ToString();

                if (!string.IsNullOrWhiteSpace(fflAll))
                {
                    foreach (var ffl in fflAll.Split(','))
                        if (!string.IsNullOrWhiteSpace(ffl))
                            fileRefs.Add(new(id++, (int)tagId, Helper.FileType.FFL, ffl));
                }
                    

                if (!string.IsNullOrWhiteSpace(groupAll))
                {
                    foreach (var group in groupAll.Split(','))
                        if (!string.IsNullOrWhiteSpace(group))
                            fileRefs.Add(new(id++, (int)tagId, Helper.FileType.Group, group));
                } 

                if (!string.IsNullOrWhiteSpace(gfxAll))
                {
                    foreach (var gfx in gfxAll.Split(','))
                        if (!string.IsNullOrWhiteSpace(gfx))
                            fileRefs.Add(new(id++, (int)tagId, Helper.FileType.Schematic, gfx));
                }

                //if (!string.IsNullOrWhiteSpace(clAll))
                //{
                //    foreach (var cl in clAll.Split(','))
                //    {

                //    }
                //}

                //if (!string.IsNullOrWhiteSpace(linksCLAll))
                //{
                //    foreach (var link in linksCLAll.Split(','))
                //    {

                //    }
                //}

            }
            db.TdcFileRefs.AddRange(fileRefs);
            db.SaveChanges();
        }

        //todo: suck out CDS and CL? tables may be locked
        public static void CreateEBsFromD3K(string mdbFilePath, string? cdsCsvFilePath = null, string? hgIOCsvFilePath = null)
        {
            Dictionary<string,List<Tuple<string, string>>> cdsDict = new();
            if (cdsCsvFilePath != null)
            {
                using Microsoft.VisualBasic.FileIO.TextFieldParser csvParser = new(cdsCsvFilePath);
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string tag = fields[0];
                    string parameter = fields[1];
                    string value = fields[2];

                    if (!cdsDict.ContainsKey(tag))
                        cdsDict[tag] = new();
                    cdsDict[tag].Add(new Tuple<string, string>(parameter, value));
                }
            }

            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={mdbFilePath};Persist Security Info=False;"; 
            //connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};User Id=admin;Password=admin;";

            using var conection = new OleDbConnection(connectionString);
            conection.Open();

            var query = "Select * From Tags";
            using var tagDa = new OleDbDataAdapter(query, conection);
            var tagDt = new DataTable();
            tagDa.Fill(tagDt);

            var sb = new StringBuilder();
            var pointTypeList = tagDt.AsEnumerable().Select(x => x.Field<string>("Type")).Distinct();
            var amTags = tagDt.AsEnumerable().Where(x => x.Field<string>("Type")[^2..] == "AM").Select(x => new { Name = x.Field<string>("Name") , AM = x.Field<string>("Network").Replace("AM ", null) });
            foreach (var pointType in pointTypeList)
            {
                query = $"Select * From [{pointType}] order by [name]";
                using var parameterDa = new OleDbDataAdapter(query, conection);
                var parameterDt = new DataTable();

                try
                {
                    parameterDa.Fill(parameterDt);
                }
                catch (OleDbException ex)
                {
                    continue; //spare points, not table for point type
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                DataTable? logicDt = null;
                if (pointType == "LOGICNIM")
                {
                    query = "SELECT * FROM LOGICNIM_GATES order by [name]";
                    using var logicDa = new OleDbDataAdapter(query, conection);
                    logicDt = new DataTable();
                    logicDa.Fill(logicDt);
                }

                for (int k = 0; k < parameterDt.Rows.Count; k++)
                {
                    var paramDr = parameterDt.Rows[k];
                    var logicDr = logicDt?.Rows[k];

                    if (pointType == "HLAI" || pointType == "LLMUX" || pointType == "STIM")
                        sb.AppendLine($"&T ANINNIM"); 
                    else
                        sb.AppendLine($"&T {pointType}");

                    string? dictKey = null;
                    for (int i = 0; i < parameterDt.Columns.Count; i++)
                    {
                        var paramDc = parameterDt.Columns[i];
                        var logicDc = i < logicDt?.Columns.Count ? logicDt?.Columns[i] : null;

                        var paramName = paramDc.ColumnName;
                        if (paramName != "LogicBlockID")
                        {
                            var paramValue = paramDr[paramDc].ToString();

                            if (i == 0)
                            {
                                dictKey = paramValue;
                                sb.AppendLine($"&N {paramValue}");
                                if (pointType[^2..] == "AM")
                                    sb.AppendLine($"NTWKNUM-AM = \"{amTags.Where(x => x.Name == paramValue).Select(x => x.AM).FirstOrDefault()}\"");
                            }
                            else if (!string.IsNullOrWhiteSpace(paramValue))
                                sb.AppendLine($"{paramName} = \"{paramValue}\"");

                        }

                        var logicName = logicDc?.ColumnName;
                        if (i > 0 && logicDc != null) //skips name
                        {
                            var logicValue = logicDr[logicDc].ToString();
                            if (!string.IsNullOrWhiteSpace(logicValue))
                                sb.AppendLine($"{logicName} = \"{logicValue}\"");
                        }
                    }

                    if (dictKey != null && cdsDict.TryGetValue(dictKey, out List<Tuple<string, string>> cdsList))
                    {
                        foreach (var cds in cdsList)
                            sb.AppendLine($"{cds.Item1} = \"{cds.Item2}\"");
                    }
                }
            }

            if (hgIOCsvFilePath != null)
            {
                using Microsoft.VisualBasic.FileIO.TextFieldParser csvParser = new(hgIOCsvFilePath);
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string tag = fields[0];
                    string pointType = fields[1];
                    string hwy = fields[2];
                    string boxIn = fields[3];
                    string slotIn = fields[4];
                    string boxOut = fields[5];
                    string slotOut = fields[6];

                    sb.AppendLine($"&T {pointType}");
                    sb.AppendLine($"&N {tag}");
                    sb.AppendLine($"PTDESC = \"Not real TDC tag. Imported into Mapper\"");
                    sb.AppendLine($"HWYNUM = \"{hwy}\"");

                    if (!string.IsNullOrWhiteSpace(boxIn))
                    {
                        sb.AppendLine($"BOXNUM = \"{boxIn}\"");
                        sb.AppendLine($"SLOTNUM = \"{slotIn}\"");
                    }
                    else
                    {
                        sb.AppendLine($"OUTBOXNM = \"{boxOut}\"");
                        sb.AppendLine($"OUTSLTNM = \"{slotOut}\"");
                    }
                  
                }
            }

            File.WriteAllText(D3kEbFilePath, sb.ToString());
        }

        /// <summary>
        /// format eb file for easier parsing
        /// </summary>
        private static string FormatEbFiles(string ebFileContents)
        {
            //format eb file for easier parsing
            ebFileContents = regexEbComments.Replace(ebFileContents, ""); //removes comments e.g. {ignore}
            ebFileContents = regexEbQuotes.Replace(ebFileContents, @""""); //makes all double quotes the same
            ebFileContents = regexEbNewLine.Replace(ebFileContents, @" "); //remove carraige return and new lines (could be within quotations)
            ebFileContents = regexEbSpace.Replace(ebFileContents, @" "); //remove extra space 
            ebFileContents = regexEbParameterSpace.Replace(ebFileContents, @" = ").Trim(); //remove extra space between param and value
            return ebFileContents;
        }
        private static void EbParseGroups(string ebFileContents, string ebFileName, List<TdcGroup> TdcGroups)
        {
            //format eb file for easier parsing
            ebFileContents = FormatEbFiles(ebFileContents);

            MatchCollection matchCollection = regexEb.Matches(ebFileContents);
            for (int k = 0; k < matchCollection.Count; k++)
            {
                var match = matchCollection[k];
                string tagname = match.Groups["name"].Value.Trim().ToUpper();
                string pointType = match.Groups["pointType"].Value.Trim().ToUpper();

                if (pointType.Contains("GROUP"))
                {
                    var group = new TdcGroup();
                    group.EbFile = ebFileName;

                    CaptureCollection paramCaptures = match.Groups["param"].Captures;
                    CaptureCollection valueCaptures = match.Groups["value"].Captures;
                    for (int i = 0; i < paramCaptures.Count; i++)
                    {
                        string paramName = paramCaptures[i].Value.ToUpper();

                        int index = 0;
                        if (paramName.Contains('('))
                        {
                            var startIndex = paramName.IndexOf('(');
                            var endIndex = paramName.IndexOf(')');
                            int.TryParse(paramName[(startIndex + 1)..endIndex], out index);
                        }

                        string? paramValue = valueCaptures[i].Value.Trim('"').Trim();
                        if (string.IsNullOrWhiteSpace(paramValue))
                            paramValue = null;
                        else if (paramValue.Contains('-') && (string.IsNullOrWhiteSpace(paramValue.Trim('-')) || paramValue.Trim('-') == ".")) //------- or --.--
                            paramValue = null;
                        else if (paramValue.Contains('_') && (string.IsNullOrWhiteSpace(paramValue.Trim('_')) || paramValue.Trim('_') == ".")) //_______ or ____.____
                            paramValue = null;
                        else if (double.TryParse(paramValue, out double number))
                            paramValue = number.ToString(); //removes trailing 0's

                        if (paramName == "OENTNUMP")
                            group.OENTNUMP = int.Parse(paramValue);
                        else if (paramName == "TITLE")
                            group.TITLE = paramValue;
                        else if (paramName.Contains("EXTIDLST"))
                            group.EXTIDLST[index - TdcGroup.OffsetEXTIDLST] = paramValue;
                        else if (paramName.Contains("TRNDSET"))
                            group.TRNDSET[index - TdcGroup.OffsetTRNDSET] = paramValue;
                        else if (paramName == "TRNDBASE")
                            group.TRNDBASE = paramValue;
                        else if (paramName.Contains("SCREENUM"))
                            group.SCREENUM[index - TdcGroup.OffsetSCREENUM] = paramValue;
                        else if (paramName.Contains("DISPID"))
                            group.DISPID[index - TdcGroup.OffsetDISPID] = paramValue;

                        TdcGroups.Add(group);
                    }
                }
            }
        }
        private static void EbParseContents(string ebFileContents, string ebFileName, bool createGraphs, List<CLSlot>? clSlotsList)
        {
            //format eb file for easier parsing
            ebFileContents = FormatEbFiles(ebFileContents);


            MatchCollection matchCollection = regexEb.Matches(ebFileContents);
            for (int k = 0; k < matchCollection.Count; k++)
            {
                var match = matchCollection[k];
                string tagname = match.Groups["name"].Value.Trim().ToUpper();
                string pointType = match.Groups["pointType"].Value.Trim().ToUpper();

                if (!pointType.Contains("GROUP") && pointType != "HM_HIST") //not a group or history
                {
                    TdcTags.TryGetValue(tagname, out TdcTag? tdcTag);
                    if (tdcTag == null)
                        tdcTag = new TdcTag(tagname, pointType, ebFileName);
                    else
                        tdcTag.EbFile += $"\n{ebFileName}";

                    CaptureCollection paramCaptures = match.Groups["param"].Captures;
                    CaptureCollection valueCaptures = match.Groups["value"].Captures;
                    for (int i = 0; i < paramCaptures.Count; i++)
                    {
                        string paramName = paramCaptures[i].Value.ToUpper();
                        if (regexLeadingZero.IsMatch(paramName))
                        {
                            var startIndex = paramName.IndexOf('(');
                            var endIndex = paramName.IndexOf(')');
                            var index = paramName[(startIndex + 1)..endIndex];
                            if (index != "0")
                                paramName = $"{paramName[..startIndex]}({int.Parse(index)})";
                            //paramName = regexLeadingZero.Replace(paramName, "(");
                        }
                            

                        string? paramValue = valueCaptures[i].Value.Trim('"').Trim();
                        if (string.IsNullOrWhiteSpace(paramValue))
                            paramValue = null;
                        else if (paramValue.Contains('-') && (string.IsNullOrWhiteSpace(paramValue.Trim('-')) || paramValue.Trim('-') == ".")) //------- or --.--
                            paramValue = null;
                        else if (paramValue.Contains('_') && (string.IsNullOrWhiteSpace(paramValue.Trim('_')) || paramValue.Trim('_') == ".")) //_______ or ____.____
                            paramValue = null;
                        else if (double.TryParse(paramValue, out double number))
                            paramValue = number.ToString(); //removes trailing 0's

                        TdcParameter tdcParam = new(tdcTag, paramName, paramValue, valueCaptures[i].Value);
                        if (!tdcTag.Params.Where(x => x.Name == tdcParam.Name).Any())
                        {
                            tdcTag.Params.Add(tdcParam);
                            TdcParameters.Add(tdcParam);
                        }
                        else if (tdcParam.Value != null && tdcTag.Params.Where(x => x.Name == tdcParam.Name && string.IsNullOrWhiteSpace(x.Value)).Any())
                        {
                            var oldTdcParameter = tdcTag.Params.Where(x => x.Name == tdcParam.Name).First();
                            oldTdcParameter.Value = tdcParam.Value;
                            oldTdcParameter.RawValue = tdcParam.RawValue;
                        }
                        else
                            continue;

                        #region parameters used often that are useful to map to class properties
                        if (paramName == "PTDESC")
                            tdcTag.Desc = paramValue;
                        else if (paramName == "NTWKNUM")
                            tdcTag.NTWKNUM = paramValue;
                        else if(paramName == "NODENUM")
                            tdcTag.NODENUM = paramValue;
                        else if(paramName == "SLOTNUM")
                            tdcTag.SLOTNUM = paramValue;
                        else if(paramName == "MODNUM")
                            tdcTag.MODNUM = paramValue;
                        else if(paramName == "NODETYP")
                            tdcTag.NODETYP = paramValue;
                        else if(paramName == "PVALGID")
                            tdcTag.PVALGID = paramValue;
                        else if(paramName == "CTLALGID")
                            tdcTag.CTLALGID = paramValue;
                        else if (paramName == "HWYNUM")
                            tdcTag.HWYNUM = paramValue;
                        else if (paramName == "BOXNUM")
                            tdcTag.BOXNUM = paramValue;
                        else if (paramName == "NTWKNUM-AM")
                            tdcTag.AM = paramValue;
                        else if (paramName == "INPTSSLT")
                            tdcTag.INPTSSLT = paramValue;
                        else if (paramName == "INTVARNM")
                            tdcTag.INTVARNM = paramValue;
                        else if (paramName == "OUTBOXNM")
                            tdcTag.OUTBOXNM = paramValue;
                        else if (paramName == "OUTSLTNM")
                            tdcTag.OUTSLTNM = paramValue;
                        else if (paramName == "OUTSSLT")
                            tdcTag.OUTSSLT = paramValue;
                        else if (paramName == "ALGIDDAC")
                            tdcTag.ALGIDDAC = paramValue;
                        else if (paramName == "PNTBOXTY")
                            tdcTag.PNTBOXTY = paramValue;
                        #endregion

                        #region Update packages attached to tag
                        if (paramName.Contains("PKGNAME"))
                        {
                            if (!tdcTag.PackageExists)
                            {
                                tdcTag.PackageExists = true;
                                tdcTag.Packages = new List<string>();
                            }

                            tdcTag.Packages.Add(paramValue.ToUpper().Trim('"').Trim());
                        }
                        else if (paramName.Contains("BLKNAME"))
                        {
                            if (!tdcTag.BlockExists)
                            {
                                tdcTag.BlockExists = true;
                                tdcTag.Blocks = new List<string>();
                            }

                            tdcTag.Blocks.Add(paramValue.ToUpper().Trim());
                        }
                        #endregion
                    }

                    if (!TdcTags.ContainsKey(tdcTag.Name))
                        TdcTags.Add(tdcTag.Name, tdcTag);
                }
                else if (pointType.Contains("GROUP"))
                {

                }
            }

            if (clSlotsList != null)
            {
                foreach (CLSlot clSlot in clSlotsList)
                {
                    if (TdcTags.TryGetValue(clSlot.Tag, out TdcTag tdcTag))
                    {
                        var lastBlockIndex = 0;
                        if (tdcTag.BlockExists)
                        {
                            var lastBlockParam = tdcTag.Params.Where(x => x.Name.Contains("BLKNAME")).OrderBy(x => x.Name).Last().Name;
                            var index = lastBlockParam[8..^1];
                            lastBlockIndex = int.Parse(index) + 1;
                        }

                        if (!tdcTag.Params.Any(x => x.Value == clSlot.Block && x.Name.Contains("BLKNAME")))
                        {
                            if (!tdcTag.BlockExists)
                            {
                                tdcTag.BlockExists = true;
                                tdcTag.Blocks = new List<string>();
                            }

                            var newParam = new TdcParameter(tdcTag, $"BLKNAME({lastBlockIndex})", clSlot.Block, clSlot.Block);
                            tdcTag.Params.Add(newParam);
                            TdcParameters.Add(newParam);
                            tdcTag.Blocks.Add(clSlot.Block);
                        }
                    }
                }
            }

            TdcParameter.TdcTagDict = TdcTags;

            //recursive search to find source parameter e.g. LISRC(1) = LOGIC.LISRC(2)
            foreach (var tdcParameter in TdcParameters)
                GetSourceParameter(tdcParameter);
        }

        private static void GetSourceParameter(TdcParameter tdcParameter)
        {
            if (tdcParameter.Value == null)
                return;

            if (Logicnim.IsLISRC(tdcParameter.Value) || Logicnim.IsLOSRC(tdcParameter.Value) || Logicnim.IsLOENBL(tdcParameter.Value))
            {
                var tagParam = tdcParameter.Value.Split('.');
                var tagName = tagParam[0];
                var paramName = tagParam[1];

                TdcTags.TryGetValue(tagName, out TdcTag? connectedTag);
                var newParamValue = connectedTag?.Params.FirstOrDefault(x => x.Name == paramName)?.Value;
                tdcParameter.Value = newParamValue;
                GetSourceParameter(tdcParameter); //calls it again in case new param needs to be updated
            }
        }

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

                        var hwy = int.Parse(match.Groups["HWY"].Success ? match.Groups["HWY"].Value : procModHg.HWYNUM).ToString();
                        var box = int.Parse(match.Groups["Box"].Success ? match.Groups["Box"].Value : procModHg.BOXNUM).ToString();
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
                        var externalStatement = Environment.NewLine + "\tEXTERNAL " + newExternalTags.Aggregate((x, y) => x + ", " + y) + " --added by Parser";
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
                var blockAndPackageTags = TdcTags.Values.Where(x => (x.BlockExists && x.Blocks.Contains(clName)) || (x.PackageExists && x.Packages.Contains(packageName)));
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

        private static void AddBoxInternalTag(string boxTagname, string file, string hwy, string box)
        {
            var tag = new TdcTag(boxTagname, "BOX (Internal)", file) { HWYNUM = hwy.TrimStart('0'), BOXNUM = box.TrimStart('0') };
            TdcTags.Add(boxTagname, tag); //todo: make this a method (1 other spot uses same code) and add in important params such as hwy, box slot etc
            
            var hwyParam = new TdcParameter(tag, "HWYNUM", tag.HWYNUM, tag.HWYNUM);
            tag.Params.Add(hwyParam);
            TdcParameters.Add(hwyParam);

            var boxParam = new TdcParameter(tag, "BOXNUM", tag.BOXNUM, tag.BOXNUM);
            tag.Params.Add(boxParam);
            TdcParameters.Add(boxParam);
        }

        private static void FindCDS(TdcCL cl)
        {
            var matchCollection = regexClParamList.Matches(cl.Content);
            if (matchCollection.Any())
            {
                foreach (Match match in matchCollection)
                {
                    CaptureCollection paramCaptures = match.Groups["parameter"].Captures;
                    CaptureCollection typeCaptures = match.Groups["type"].Captures;

                    if (typeCaptures.Any()) //no type defined is a number
                    {
                        if (typeCaptures.Count > 1)
                            throw new Exception("Multiple types not accounted for");

                        var paramType = typeCaptures.First().Value.ToUpper();
                        if (paramType.Contains('"'))
                            paramType = paramType[..paramType.IndexOf('"')].Trim();
                        if (paramType == "$REG_CTL" || (paramType.IndexOf("ARRAY") != 0 && !paramType.Contains("NUMBER") && !paramType.Contains("TIME") && !paramType.Contains("LOGICAL") && !paramType.Contains("STRING") && !paramType.Contains('/')))
                        {
                            for (int i = 0; i < paramCaptures.Count; i++)
                            {
                                foreach (var tagname in cl.TagsCLIsAttachedTo)
                                {
                                    int lowIndex = 0;
                                    int highIndex = 0;
                                    var isArray = false;
                                    if (paramType.Contains("ARRAY"))
                                    {
                                        var arrayIndex = paramType.IndexOf("ARRAY");
                                        var array = paramType[(paramType.IndexOf('(', arrayIndex) + 1)..paramType.IndexOf(')', arrayIndex)];
                                        var split = array.Split("..");
                                        lowIndex = int.Parse(split[0]);
                                        highIndex = int.Parse(split[1]);
                                        isArray = true;
                                    }

                                    var index = lowIndex;
                                    while (index <= highIndex)
                                    {
                                        if (TdcTags.TryGetValue(tagname, out TdcTag tag))
                                        {
                                            var parameterName = paramCaptures[i].Value.ToUpper();
                                            if (isArray)
                                                parameterName += $"({index})";

                                            var cdsParam = tag.Params.FirstOrDefault(x => x.Name == parameterName);
                                            if (cdsParam != null && cdsParam.Value != null)
                                            {
                                                if (TdcTags.TryGetValue(cdsParam.Value, out TdcTag cdsTag))
                                                {
                                                    cdsParam.CdsTag = cdsTag;
                                                    AddCL(cl, cdsTag.Name);
                                                }
                                            }
                                        }

                                        index++;
                                    }


                                }
                            }
                        }
                    }
                }
            }
        }

        private static (TdcTag?, string?) ParseHgBoxAddress(Match match, string? defaultHwy = null, string? defaultBox = null)
        {
            var hwy = int.Parse(match.Groups["HWY"].Success ? match.Groups["HWY"].Value : defaultHwy).ToString();
            var box = int.Parse(match.Groups["Box"].Success ? match.Groups["Box"].Value : defaultBox).ToString();
            var param = match.Groups["parameter"].Value.ToUpper();
            var index = int.Parse(match.Groups["index"].Value);

            TdcTag? tag = null;
            string? tagParam = null;
            if (param == "NN") //numeric
            {
                tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "NUMERCHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.INTVARNM == index.ToString());
                tagParam = "PV";
            }
            else if (param == "FL") //flag
            {
                tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "FLAGHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.INTVARNM == index.ToString());
                tagParam = "PVFL";
            }
            else if (param == "TM") //timer
                tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "TIMERHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.INTVARNM == index.ToString());
            else if (param == "LP") //regctl
                tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "REGHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == index.ToString());
            else if (param == "LB") //logicblock (.js file)
            {
                tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "LOGICHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.Params.First(x => x.Name == "Gate").Value == index.ToString() && !(x.Logic as LogicHgGate).SpecialGate);
                tagParam = "OUT";
            }
            else if (param == "AI") //analog input
            {
                if (index <= 8)
                    tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "ANLINHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == "7" && x.INPTSSLT == index.ToString());
                else
                    tag = TdcTags.Values.FirstOrDefault(x => x.PointType == "ANLINHG" && x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == "8" && x.INPTSSLT == (index - 8).ToString());
            }
            else if (param == "DI") //dig in
            {
                var channel = int.Parse(index.ToString()[^2..]).ToString();
                var slot = int.Parse(index.ToString()[..^2]).ToString();
                tag = TdcTags.Values.FirstOrDefault(x => x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == slot && x.INPTSSLT == channel);
            }
            else if (param == "DO") //dig out
            {
                var channel = int.Parse(index.ToString()[^1..]).ToString();
                var slot = int.Parse(index.ToString()[..^1]).ToString();
                tag = TdcTags.Values.FirstOrDefault(x => x.HWYNUM == hwy && x.OUTBOXNM == box && x.OUTSLTNM == slot && x.OUTSSLT == channel);
            }
            else if (param == "CI") //counter
            {
                var channel = int.Parse(index.ToString()[^1..]).ToString();
                var slot = int.Parse(index.ToString()[..^1]).ToString();
                tag = TdcTags.Values.FirstOrDefault(x => x.HWYNUM == hwy && x.BOXNUM == box && x.SLOTNUM == slot && x.INPTSSLT == channel);
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

        private static void AddRegDigHgConnections()
        {
            var regHgTags = TdcTags.Values.Where(x => x.PointType == "REGHG");
            foreach (var parentTag in regHgTags)
            {
                #region Analog inputs
                var childTag = TdcTags.Values.FirstOrDefault(x => x.PointType == "ANLINHG" && x.HWYNUM == parentTag.HWYNUM && x.BOXNUM == parentTag.BOXNUM && x.SLOTNUM == parentTag.SLOTNUM && x.INPTSSLT == null);
                if (childTag != null)
                {
                    var param = new TdcParameter(parentTag, $"HGSRC(1)", $"{childTag.Name}.PV", $"{childTag.Name}.PV");
                    TdcParameters.Add(param);
                    parentTag.Params.Add(param);
                }
                #endregion
                
                #region Analog outputs
                childTag = TdcTags.Values.FirstOrDefault(x => x.PointType == "ANLOUTHG" && x.HWYNUM == parentTag.HWYNUM && x.OUTBOXNM == parentTag.BOXNUM && x.OUTSLTNM == parentTag.SLOTNUM && x.OUTSSLT == null);
                if (childTag != null)
                {
                    var param = new TdcParameter(parentTag, $"HGDSTN(1)", $"{childTag.Name}.OP", $"{childTag.Name}.OP");
                    TdcParameters.Add(param);
                    parentTag.Params.Add(param);
                }
                #endregion
            }

            var digHgTags = TdcTags.Values.Where(x => x.PointType == "DIGCMPHG");
            foreach (var parentTag in digHgTags)
            {
                #region Digital inputs
                if (int.TryParse(parentTag.Params.FirstOrDefault(x => x.Name == "NMBRINPT")?.Value, out int diCount))
                {
                    for (int i = 0; i < diCount; i++)
                    {
                        var childTag = TdcTags.Values.FirstOrDefault(x => x.PointType == "DIGINHG" && x.HWYNUM == parentTag.HWYNUM && x.BOXNUM == parentTag.BOXNUM && x.SLOTNUM == parentTag.SLOTNUM && x.INPTSSLT == (int.Parse(parentTag.INPTSSLT) + i).ToString());
                        if (childTag != null)
                        {
                            var param = new TdcParameter(parentTag, $"HGSRC({i + 1})", $"{childTag.Name}.PVFL", $"{childTag.Name}.PVFL");
                            TdcParameters.Add(param);
                            parentTag.Params.Add(param);
                        }
                    }
                }
                #endregion

                #region Digital outputs
                if (int.TryParse(parentTag.Params.FirstOrDefault(x => x.Name == "NMBROUT")?.Value, out int doCount))
                {
                    for (int i = 0; i < doCount; i++)
                    {
                        var childTag = TdcTags.Values.FirstOrDefault(x => x.PointType == "DIGOUTHG" && x.HWYNUM == parentTag.HWYNUM && x.OUTBOXNM == parentTag.OUTBOXNM && x.OUTSLTNM == parentTag.OUTSLTNM && x.OUTSSLT == (int.Parse(parentTag.OUTSSLT) + i).ToString());
                        if (childTag != null)
                        {
                            var param = new TdcParameter(parentTag, $"HGDSTN({i + 1})", $"{childTag.Name}.OP", $"{childTag.Name}.OP");
                            TdcParameters.Add(param);
                            parentTag.Params.Add(param);
                        }
                    }
                }
                #endregion
            }
        }

        public static List<TdcTag> ParseTdcFiles(string ebFolderPath, string? clFolderPath, string? jsFolderPath, string? customConnectionsPath, string? clSlotsConnectionPath, bool createGraphs, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            TdcTags.Clear();

            if (createGraphs)
                GraphFactory.Clear();

            #region CL Slots
            List<CLSlot>? clSlotsList = null;
            if (clSlotsConnectionPath != null)
            {
                clSlotsList = new();
                if (!File.Exists(clSlotsConnectionPath))
                    throw new Exception("Invalid CL Slots File Path");

                var file = File.ReadAllLines(clSlotsConnectionPath);
                foreach (var line in file)
                {

                    var trimmedLine = regexEbSpace.Replace(line, @" ").Trim(); //remove extra space 
                    var fields = trimmedLine.Split(' ');
                    if (fields.Length == 4)
                    {
                        if (fields[0] == "MEDIA" || fields[0] == "---------")
                            continue;
                        var block = fields[3].Trim().ToUpper();
                        var tag = fields[2].Trim().ToUpper();
                        clSlotsList.Add(new(block, tag));
                    }
                }
            }
            #endregion

            #region EB
            if (!Directory.Exists(ebFolderPath))
                throw new Exception("Invalid EB File Path");

            var directoryInfo = new DirectoryInfo(ebFolderPath);
            foreach (var file in directoryInfo.GetFiles("*.eb", searchOption))
                EbParseContents(File.ReadAllText(file.FullName), file.Name, createGraphs, clSlotsList);

            AddRegDigHgConnections();
            #endregion

            #region JS
            if (jsFolderPath != null)
            {
                if (!Directory.Exists(jsFolderPath))
                    throw new Exception("Invalid JS File Path");

                List<LogicHgGate> hgLogicList = new();
                directoryInfo = new DirectoryInfo(jsFolderPath);
                foreach (var file in directoryInfo.GetFiles("*.js", searchOption))
                    JSParseContent(File.ReadAllLines(file.FullName), file.Name, hgLogicList);

                foreach (var hgLogic in hgLogicList)
                {
                    AddConnectionsToLogicHg(hgLogic, hgLogic.Input1, "HGSRC(1)");
                    AddConnectionsToLogicHg(hgLogic, hgLogic.Input2, "HGSRC(2)");
                    AddConnectionsToLogicHg(hgLogic, hgLogic.Input3, "HGSRC(3)");
                    AddConnectionsToLogicHg(hgLogic, hgLogic.Output1, "HGDSTN(1)");
                    AddConnectionsToLogicHg(hgLogic, hgLogic.Output2, "HGDSTN(2)");
                }
            }
            #endregion

            #region CL
            if (clFolderPath != null)
            {
                if (!Directory.Exists(clFolderPath))
                    throw new Exception("Invalid CL File Path");

                directoryInfo = new DirectoryInfo(clFolderPath);
                foreach (var file in directoryInfo.GetFiles("*.cl", searchOption))
                    CLParseContent(File.ReadAllText(file.FullName), file.Name);

                //var packages = TdcParameters.Where(x => x.Name.Contains("PKGNAME("));
                //foreach (var package in packages)
                //{
                //    TdcCL? cl = TdcCL.FirstOrDefault(x => x.FileName.ToUpper().Replace(".CL", null) == package.Value.ToUpper());
                //    AddCL(cl, package.ParentTag.Name);
                //    FindCDS(cl);
                //}
            }
            #endregion

            if (createGraphs)
            {
                foreach (var tag in TdcTags.Values)
                    GraphFactory.TreeDict.TryAdd(tag.Name, new List<Graph?>());

                GraphFactory.CreateGraphs(customConnectionsPath);
            }


            return TdcTags.Select(x => x.Value).ToList();
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
                        AddBoxInternalTag(boxTagname, hgLogic.TdcTag.EbFile, hwy, box);
                        tag = TdcTags[boxTagname];
                    }
                }
                hgLogic.AddTdcParam(paramName, $"{tag.Name}.{hgParam ?? param}");
            }
            else //references tag in TDC dict
                hgLogic.AddTdcParam(paramName, logicConnectionName);
        }

        public static void JSParseContent(string[] fileContents, string fileName, List<LogicHgGate> hgLogicList)
        {
            //LB010901
            //01234567
            var t = fileName[2..4];
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

        public static List<TdcTag> GetModules(string ebFolderPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return GetModules(ParseTdcFiles(ebFolderPath, null, null, null, null, false, searchOption));
        }

        public static List<TdcTag> GetModules()
        {
            return GetModules(TdcTags.Values.ToList());
        }

        public static List<TdcTag> GetModules(List<TdcTag> tdcTags)
        {
            var parentModuleTypes = new List<string>() { "REGCLNIM", "DICMPNIM", "DEVCTL", "ANLCMPHG", "DIGCMPHG", "REGAM", "REGHG" };
            var singleModuleTypes = new List<string>() { "ANINNIM", "ANLINHG", "ANOUTNIM", "ANLOUTHG", "DIINNIM", "DIGINHG", "DIOUTNIM", "DIGOUTHG", "REGPVNIM", "PRMODNIM", "PRCMODHG", "CUSTOMAM", "FLAGNIM", "FLAGAM" };
            var ParentConnectionTypes = new List<string>() { "CISRC", "CODSTN", "DISRC", "DODSTN", "HGSRC", "HGDSTN" };

            var parentTags = tdcTags.Where(x => parentModuleTypes.Contains(x.PointType));

            foreach (var parentTag in parentTags)
            {
                var module = new TdcModule(parentTag.Name, parentTag.Desc);
                module.TdcTags.Add(parentTag);
                parentTag.Module = module;

                foreach (var parameter in parentTag.Params)
                {
                    foreach (var connectionType in ParentConnectionTypes)
                    {
                        var index = parameter.Name.IndexOf(connectionType);
                        if (index == 0)
                    {
                            var tagParam = parameter.Value?.Split('.');
                            if (tagParam?.Length > 0)
                            {
                                var childTagName = tagParam[0].ToUpper();
                                var childTag = tdcTags.FirstOrDefault(x => x.Name == childTagName);
                                if (childTag != null && !parentModuleTypes.Contains(childTag.PointType))
                                {
                                    childTag.Module = module;
                                    module.TdcTags.Add(childTag);
                                }
                            }
                        }
                    }                  
                }

                Modules.Add(module);
            }

            var otherTags = tdcTags.Where(x => x.Module == null);
            foreach (var tag in otherTags)
            {
                if (singleModuleTypes.Contains(tag.PointType))
                {
                    var module = new TdcModule(tag.Name, tag.Desc);
                    tag.Module = module;
                    Modules.Add(module);
                }
                    
            }

            return tdcTags; //todo: sort tdc tags, modules
        }

        public static void ExportModules(string ebFolderPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var modules = GetModules(ebFolderPath, searchOption);
            ExportModules(modules, ModulesFilePath);
        }

        public static void ExportModules(List<TdcTag> tdcTags, string savePath)
        {
            Export.ExportTdcToXlsx(new List<TdcTag>(tdcTags), savePath); //need to get tdc tags
        }

       public static void ExportGroups(string ebFolderPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(ebFolderPath))
                throw new Exception("Invalid EB File Path");

            List<TdcGroup> tdcGroups = new(1000);
            var directoryInfo = new DirectoryInfo(ebFolderPath);
            foreach (var file in directoryInfo.GetFiles("*.eb", searchOption))
                EbParseGroups(File.ReadAllText(file.FullName), file.Name, tdcGroups);

            Export.ExportGroups(tdcGroups, GroupFilePath);
        }
    }
}