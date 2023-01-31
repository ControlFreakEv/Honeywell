using Mapper.Samples;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//todo: convert regex to GeneratedRegex() attribute
//todo: test the following line -> BLOCK Level (GENERIC $reg_ctl; AT PV_ALG)
//todo: test the following line " = $HY02B22"
//todo: time could be local variable but keyword will override
namespace Honeywell.GUI.Mapper.ScintillaExt
{    public class CLLexer
    {
        public const int Default = 0;
        public const int Keyword = 1;
        public const int Property = 2;
        public const int Number = 3;
        public const int String = 4;
        public const int Comment = 5;
        public const int Symbol = 6;
        public const int Local = 7;
        public const int External = 8;
        //public const int Parameter { get; } = 9;
        public const int Label = 10;
        public const int Type = 11;
        public const int Function = 12;
        public const int Directive = 13; 
        public const int ParameterList = 14; 
        public const int Enumeration = 15; 
        public const int ExternalMissing = 16;
        public const int CustomDataSegment = 17;

        private const int STATE_UNKNOWN = 0;
        private const int STATE_PROPERTY = 1;
        private const int STATE_NUMBER = 2;
        private const int STATE_STRING = 3;
        private const int STATE_COMMENT = 4;
        private const int STATE_DIRECTIVE = 5;

        public string AttachedPoint { get; set; }
        public static List<string> TdcTags { get; set; } = new List<string>();
        public static List<string> SampleTdcTags { get; set; } = new List<string>();

        private static readonly List<string> keywordList = new List<string>()
            {
                //keywords
                "ABORT",
                "ACCESS",
                "ALARM",
                "AND",
                "ANY_ENUMERATION",
                "ARRAY",
                "AT",
                "BLD_VISIBLE",
                "BLOCK",
                "CALL",
                "CLASS",
                "CUSTOM",
                "DATA_POINT_ID",
                "DAYS",
                "DEFINE",
                "ELSE",
                "EMERGENCY",
                "END",
                "ENUMERATION",
                "ERROR",
                "EU",
                "EXIT",
                "EXTERNAL",
                "FAIL",
                "FOR",
                "FROM",
                "FUNCTION",
                "GENERIC",
                "GOTO",
                "HANDLER",
                "HELP",
                "HOLD",
                "HOURS",
                "IF",
                "IN",
                "INITIATE",
                "LOCAL",
                "LOOP",
                "MINS",
                "MOD",
                "NAME",
                "NOT",
                "OR",
                "OTHERS",
                "OUT",
                "PACKAGE",
                "PARALLEL",
                "PARAM_LIST",
                "PARAMETER",
                "PAUSE",
                "PHASE",
                "POINT",
                "RANGE",
                "READ",
                "REFERENCE",
                "REFERENCE_N",
                "RELAX",
                "REPEAT",
                "RESTART",
                "RESUME",
                "SECS",
                "SEND",
                "SEQUENCE",
                "SET",
                "SHUTDOWN",
                "STEP",
                "SUBROUTINE",
                "THEN",
                "UNIT",
                "VALUE",
                "WAIT",
                "WHEN",
                "WRITE",
                "XOR",
                "Engineer",
                //access lock
                "Entity_Bldr",
                "Operator",
                "Program",
                "Supervisor",
                //insertion point
                "General",
                "Pre_CtAg",
                "Pre_CtPr",
                "Pre_GI",
                "Pre_PVa",
                "Pre_PVAg",
                "Pre_PVpr",
                "Pre_SP",
                "Pst_CtAg",
                "Pst_CtPr",
                "Pst_GO",
                "Pst_PVAg",
                "Pst_PVFL",
                "Pst_PVPr",
                "PV_Alg",
                "Backgrnd",
                "Ctl_Alg",
                //logical
                "Off",
                "On",
                //idr
                "MC",
                "PM",
                //type
                "Logical",
                "Number",
                "String",
                "Time" 
                };
        private readonly List<string> keyWordExceptions = new List<string>() { "PROGRAM", "Operator", "OFF", "ON"};
        private readonly List<string> typeExceptions = new List<string>() { "Logical","Number","String", "Time"  };
        private readonly List<string> labelExceptions = new List<string>() { "PARAMETER", "LOCAL" };
        private readonly List<string> externalList = new List<string>();
        public readonly List<CLParameter> LocalList = new List<CLParameter>(); 
        private readonly List<string> labelList = new List<string>();
        public readonly List<CLParameter> ClParameterList = new List<CLParameter>() { new CLParameter() { Name = string.Empty, ParamList = "$REG_CTL", Type = "Pre-defined" } };
        public readonly List<CLEnumeration> EnumerationList = new List<CLEnumeration>();
        private static readonly List<string> labelKeywords = new List<string>() { "STEP", "PHASE", "SEQUENCE", "END", "BLOCK", "REPEAT", "GOTO", "SUBROUTINE", "CALL" };

        //[GeneratedRegexAttribute()]
        private static readonly Regex regexExternal = new Regex(@"(?<!(--.*))\bEXTERNAL\b[^\S\r\n]+((?<tag>('|\$)?\w+(\\\w+)?)(([^\S\r\n]*,[^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexLocal = new Regex(@"(?<!(--.*))\bLOCAL\b\s+((?<name>\w+)(\s+AT\s+(?<type>\w+(\(\d+\))?))?((\s*,[^\S\r\n]*(--.*)?((\n*|\r*)+\s*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+(\s*:\s*('|\$)?(?<type>.*))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexTag = new Regex(@"(?<!(--.*))\bPOINT\b\s*(?<tag>\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexLabel = new Regex(@"(?<!(--.*))((\bBLOCK\b|\bPHASE\b|\bSTEP\b|\bSUBROUTINE\b|\bSEQUENCE\b)\s+(?<label>\w+)|^\s*(?<label>\b\w+\b):)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex regexEnum = new Regex(@"(?<!(--.*))(\bENUMERATION\b\s*(?<name>\w+)\s*=\s*(?<value>[\w\d\s/'$]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline); 
        private static readonly Regex regexParamList = new Regex(@"((?<!(--.*))(\bPARAM_LIST\b\s*(?<paramlist>\w+)).*[\s\r\n]+)?(?<!(--.*))\bPARAMETER\b[^\S\r\n]+((?<parameter>\w+)(([^\S\r\n]*,[^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+)?)|([^\S\r\n]*(--.*)?((\n*|\r*)+[^\S\r\n]*&[^\S\r\n]+,[^\S\r\n]*)?))?)+(\s*:\s*('|\$)?(?<type>.*))?([\s\r\n]+\bEND\b\s+(?<paramlistEnd>\w+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexSubroutineParameters = new Regex(@"\bSUBROUTINE\b\s+\b\w+\b\((?<arguments>.*)\)");

        private string previousWord = "";
        private bool? sampleFile = null;

        //todo: label currently overrides cds e.g.
        //Parameter sum:number
        //sum: loop for x in y
        public void Style(Scintilla scintilla, int startPos, int endPos)
        {
            if (sampleFile == null)
                sampleFile = SampleCl.IsSampleFile(scintilla.Text);

            ParseText(scintilla.Text);
            // Back up to the line start
            var line = scintilla.LineFromPosition(startPos);
            startPos = scintilla.Lines[line].Position;

            var previousText = "";
            var previousStyle = Default;
            var length = 0;
            var state = STATE_UNKNOWN;
            var dollarSign = false;

            // Start styling
            scintilla.StartStyling(startPos);
            while (startPos < endPos - 1) //changed this to 'endpos - 1' from 'endpos'... unsure of consequences.. supposed to fix it trying to style past last char
            {
                var c = (char)scintilla.GetCharAt(startPos);
                var lastChar = startPos == endPos - 1;
                char? nextChar = (char)scintilla.GetCharAt(startPos+1);
                char? previousChar = (char)scintilla.GetCharAt(startPos - 1);

                var comment = false;
                if (nextChar == '-')
                {
                    if ((char)scintilla.GetCharAt(startPos + 2) == '-')
                        comment = true;
                }
            REPROCESS:
                switch (state)
                {
                    case STATE_UNKNOWN:
                        //if (c == '"')
                        //    break;
                        if (c == '"')
                        {
                            // Start of "string"
                            if (length != 0)
                                previousText = scintilla.GetTextRange(startPos - length, length);
                            else
                                previousText = scintilla.GetTextRange(startPos - 1, 1);

                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(1, String);
                            state = STATE_STRING;
                        }
                        else if (c == '%')
                        {
                            state = STATE_DIRECTIVE;
                            goto REPROCESS;
                        }
                        else if (char.IsDigit(c))
                        {
                            state = STATE_NUMBER;
                            goto REPROCESS;
                        }
                        else if (char.IsLetter(c))
                        {
                            state = STATE_PROPERTY;
                            if (length != 0)
                                previousText = scintilla.GetTextRange(startPos - length, length);
                            else
                                previousText = scintilla.GetTextRange(startPos - 1, 1);
                            goto REPROCESS;
                        }
                        else if (c == '-' && !lastChar && nextChar == '-')
                        {
                            state = STATE_COMMENT;
                            goto REPROCESS;
                        }
                        else if (c == '(' || c == ')' || c == '=' || c == '+' || c == '-' || c == '*' || c == '/' || c == '<' || c == '>' || c == '.' || c == ':' || c == ';' || c == ',' || c == '$' || c == '&' || c == '\'' || c == '\\')
                        {
                            var previousNonSpaceChar = GetPreviousNoneSpaceChar(scintilla, startPos);
                            if (c == '$' && previousChar != null && nextChar != null && !char.IsWhiteSpace((char)previousChar) && !char.IsWhiteSpace((char)nextChar)) //built in function //if (!string.IsNullOrWhiteSpace(previousText) && c == '$' && previousNonSpaceChar != ':') //built in function
                            {
                                state = STATE_PROPERTY;
                                goto REPROCESS;
                            }
                            else if (c == '$')
                                dollarSign = true;

                            previousText = scintilla.GetTextRange(startPos - 1, 1);
                            previousStyle = Symbol;

                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(1, Symbol);
                            state = STATE_UNKNOWN;
                        }
                        else
                        {
                            // Everything else
                            if (!char.IsWhiteSpace(c))
                            {
                                if (length != 0)
                                    previousText = scintilla.GetTextRange(startPos - length, length);
                                else
                                    previousText = scintilla.GetTextRange(startPos - 1, 1);
                                previousStyle = Default;
                            }

                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(1, Default);
                        }
                        break;

                    case STATE_STRING:
                        if (c == '"')
                        {
                            length++;
                            previousText = scintilla.GetTextRange(startPos - length, length);
                            previousStyle = String;

                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(length, String);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        else
                        {
                            length++;
                        }
                        break;

                    case STATE_NUMBER:
                        if (char.IsDigit(c) || ((c == 'e' || c == 'E') && (nextChar == '+' || nextChar == '-' || char.IsDigit(nextChar ?? 'g')))) // || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == 'x'
                        {
                            length++;
                        }
                        else if (c == '_' || char.IsLetter(c))
                        {
                            state = STATE_PROPERTY;
                            goto REPROCESS;
                        }
                        else
                        {
                            previousText = scintilla.GetTextRange(startPos - length, length);
                            previousStyle = Number;

                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(length, Number);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;

                    case STATE_PROPERTY:
                        if (!lastChar && char.IsLetterOrDigit(c) || c == '_' || c == '$') //$ is for built in functions e.g. file$createvolume
                        {
                            length++;
                        }
                        else
                        {
                            if (lastChar)
                                length++;

                            var debugLine = scintilla.LineFromPosition(startPos);
                            var debug = scintilla.Lines[debugLine].Text;

                            var style = Property;
                            var text = scintilla.GetTextRange(startPos - length, length);
                            var previousNonWhiteSpaceText = GetPreviousNoneSpaceText(scintilla, startPos - length - 1); //todo: should this -1 be there?
                            var previousNonWhiteSpaceChar = GetPreviousNoneSpaceChar(scintilla, startPos - length);

                            if (dollarSign)
                                text = "$" + text;

                            if (keywordList.Contains(text, StringComparer.OrdinalIgnoreCase) 
                                && previousText != "'" 
                                && previousText != "$" 
                                && !(previousNonWhiteSpaceChar ==  '=' && keyWordExceptions.Contains(text, StringComparer.OrdinalIgnoreCase))
                                && (!typeExceptions.Contains(text, StringComparer.OrdinalIgnoreCase) || previousNonWhiteSpaceChar == ':')
                                )
                                style = Keyword;
                            //used to come after locallist check
                            else if (labelList.Contains(text, StringComparer.OrdinalIgnoreCase) && (labelKeywords.Contains(previousNonWhiteSpaceText, StringComparer.OrdinalIgnoreCase) || c == ':') && !labelExceptions.Contains(previousNonWhiteSpaceText, StringComparer.OrdinalIgnoreCase))
                                style = Label;
                            else if (externalList.Contains(text, StringComparer.OrdinalIgnoreCase) && !labelKeywords.Contains(previousNonWhiteSpaceText, StringComparer.OrdinalIgnoreCase))
                            {
                                style = External;
                                if ((bool)sampleFile && SampleTdcTags.Any())
                                {
                                    if (!SampleTdcTags.Contains(text, StringComparer.OrdinalIgnoreCase))
                                        style = ExternalMissing;
                                }
                                else if (TdcTags.Any())
                                {
                                    if (!TdcTags.Contains(text, StringComparer.OrdinalIgnoreCase))
                                        style = ExternalMissing;
                                }
                                
                            }
                            else if (LocalList.Select(x => x.Name).Contains(text, StringComparer.OrdinalIgnoreCase) && (scintilla.GetTextRange(startPos - text.Length - 2, 2) == ".." || scintilla.GetCharAt(startPos - text.Length - 1) != '.'))
                                style = Local;
                            else if (ClParameterList.Select(x => x.ParamList).Contains(text, StringComparer.OrdinalIgnoreCase)) // && (c == ':')
                                style = ParameterList;
                            else if (ClParameterList.Select(x => x.Name).Contains(text, StringComparer.OrdinalIgnoreCase)) // && (c == ':')
                                style = CustomDataSegment; //todo: need to verify previous tag is not external, e.g. if cds = PV then that could style wrong PV AND see PARAMETER NUL_D:$REG_CTL
                            else if (EnumerationList.Select(x => x.Name).Contains(text, StringComparer.OrdinalIgnoreCase)) // && (c == ':')
                                style = Enumeration;

                            if (style == Property || style == Keyword) //number is keyword and function
                            {
                                var function = CLFunctions.Functions.FirstOrDefault(x => x.Name.ToUpper() == text.ToUpper());
                                if (function != null)
                                {
                                    if (!function.Arguments || (function.Arguments && GetNextNoneSpaceChar(scintilla, startPos) == '('))
                                        style = Function;
                                }
                            }

                            previousText = scintilla.GetTextRange(startPos - length, length);
                            previousStyle = style;
                            if (startPos != scintilla.TextLength - 1)
                            {
                                scintilla.SetStyling(length, style);

                                scintilla.IndicatorCurrent = Function;
                                if (style == Function)
                                    scintilla.IndicatorFillRange(startPos - length, length);
                                else
                                    scintilla.IndicatorClearRange(startPos - length, length);
                            }
                            length = 0;
                            state = STATE_UNKNOWN;
                            dollarSign = false;
                            previousWord = text;
                            goto REPROCESS;
                        }
                        break;

                    case STATE_COMMENT:
                        if (lastChar || (c == '\n' || c == '\r')) //last character or end of line
                        {
                            length++;
                            if (nextChar == '\n' || nextChar == '\r')
                            {
                                break;
                            }

                            previousText = scintilla.GetTextRange(startPos - length, length);
                            previousStyle = Comment;
                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(length, Comment);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        else
                        {
                            length++;
                        }
                        break;
                    case STATE_DIRECTIVE:
                        if (lastChar || (c == '\n' || c == '\r') || comment) //last character or end of line
                        {
                            length++;

                            previousText = scintilla.GetTextRange(startPos - length, length);
                            previousStyle = Directive;

                            if (startPos != scintilla.TextLength - 1)
                                scintilla.SetStyling(length, previousStyle);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        else
                            length++;
                        break;
                }

                startPos++;
            }
        }

        private char? GetNextNoneSpaceChar(Scintilla scintilla, int startPos)
        {
            char? nextChar = (char)scintilla.GetCharAt(startPos++);
            while (nextChar == ' ')
            {
                nextChar = (char)scintilla.GetCharAt(startPos++);
            }
            return nextChar;
        }

        private char GetPreviousNoneSpaceChar(Scintilla scintilla, int startPos)
        {
            var text = GetPreviousNoneSpaceText(scintilla, startPos);
            return text == null || text.Length == 0  ? '\0' : text[text.Length - 1];
        }

        private string GetPreviousNoneSpaceText(Scintilla scintilla, int startPos)
        {
            int newPos = startPos - 1;
            if (newPos < 0)
                return string.Empty;

            var currentChar = (char?)scintilla.GetCharAt(startPos);
            var nextChar = GetNextNoneSpaceChar(scintilla, startPos+1);
            char? prevChar = (char)scintilla.GetCharAt(newPos);
            while (prevChar == ' ')
            {
                prevChar = (char)scintilla.GetCharAt(newPos--);

                if (newPos == -1)
                    break;
            }

            
            while (newPos != -1 && prevChar != ' ' && prevChar != '\n' && prevChar != '\r' && prevChar != ';')
            {
                prevChar = (char?)scintilla.GetCharAt(newPos--);

                if (newPos == -1)
                    break;

            }

            newPos++;

            var prevText = scintilla.GetTextRange(newPos, startPos - newPos).Trim().Trim(';');
            return prevText;
        }

        private void ParseText(string text)
        {
            //var regexSetpoint = new Regex(@"(?<!(--.*))\bSETPOINT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //var regexCds = new Regex(@"(?<!(--.*))\bCUSTOM\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //var regexParamList = new Regex(@"(?<!(--.*))\bPARAM_LIST\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //var regexPackage = new Regex(@"(?<!(--.*))\bPACKAGE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var regexExternalMatches = regexExternal.Matches(text);
            foreach (Match match in regexExternalMatches)
            {
                var tagGroup = match.Groups["tag"];
                foreach (Capture tag in tagGroup.Captures)
                {
                    if (!externalList.Contains(tag.Value))
                        externalList.Add(tag.Value);
                }
            }

            var regexTagMatch = regexTag.Match(text).Groups["tag"].Value;
            if (!externalList.Contains(regexTagMatch))
                externalList.Add(regexTagMatch);

            AttachedPoint = regexTagMatch;

            var regexLocalMatches = regexLocal.Matches(text);
            foreach (Match match in regexLocalMatches)
            {
                string type = null;

                if (match.Groups["type"].Success)
                    type = match.Groups["type"].Captures[0].Value.Trim();

                if (type != null && type.Contains("--"))
                    type = type.Substring(0, type.IndexOf("--")).Trim(); //todo: tack on AM/PM tagname as prefex when defined as "AT" e.g. "LOCAL Average AT NN(69)" would have type of "tag.NN(69)"

                var parameters = match.Groups["name"];
                foreach (Capture capture in parameters.Captures)
                {
                    var clParameter = new CLParameter() { Name = capture.Value, Type = type };
                    if (!LocalList.Any(x => x.Name == capture.Value && x.Type == type))
                        LocalList.Add(clParameter);
                }
            }

            //regexSubroutineParameters
            var regexSubParametersMatch = regexSubroutineParameters.Match(text);
            if (regexSubParametersMatch.Success)
            {
                string paramName;
                string type = null;
                var parameters = regexSubParametersMatch.Groups["arguments"].Value.Split(';');
                foreach (var param in parameters)
                {
                    var spaceIndex = param.IndexOf(" ", 1);
                    if (spaceIndex != -1)
                    {
                        paramName = param[..(spaceIndex - 1)];

                        var inIndex = param.IndexOf("IN ", StringComparison.OrdinalIgnoreCase); //todo: not sure there is always an "IN" keyword
                        var outIndex = param.IndexOf("OUT ", StringComparison.OrdinalIgnoreCase);

                        int startIndex;
                        if (inIndex > outIndex)
                            startIndex = inIndex + 3;
                        else if (inIndex < outIndex)
                            startIndex = outIndex + 4;
                        else
                            startIndex = spaceIndex;

                        type = param[startIndex..];
                    }
                    else
                        paramName = param;
                    var clParameter = new CLParameter() { Name = paramName, Type = type };
                    LocalList.Add(clParameter);
                }
            }

            var regexLabelMatches = regexLabel.Matches(text);
            foreach (Match match in regexLabelMatches)
            {
                var labelGroup = match.Groups["label"];
                foreach (Capture label in labelGroup.Captures)
                {
                    if (!labelList.Contains(label.Value))
                        labelList.Add(label.Value);
                }
            }

            var regexParameterListMatches = regexParamList.Matches(text);
            foreach (Match match in regexParameterListMatches)
            {
                string paramList = null;
                string type = null;
                if (match.Groups["paramlist"].Success)
                    paramList = match.Groups["paramlist"].Captures[0].Value;

                if (match.Groups["type"].Success)
                    type = match.Groups["type"].Captures[0].Value.Trim();

                if (type != null && type.Contains("--"))
                    type = type.Substring(0, type.IndexOf("--")).Trim();

                var parameters = match.Groups["parameter"];
                foreach (Capture capture in parameters.Captures)
                {
                    var clParameter = new CLParameter() { Name = capture.Value, ParamList = paramList, Type = type};
                    if (!ClParameterList.Any(x => x.Name == capture.Value && x.ParamList == paramList && x.Type == type))
                        ClParameterList.Add(clParameter);
                }
            }

            var regexEnumMatches = regexEnum.Matches(text);
            foreach (Match match in regexEnumMatches)
            {
                var enumName = match.Groups["name"].Captures[0].Value;
                var enumValueArray = match.Groups["value"].Captures[0].Value.Split('/');
                string enumValue = null;

                for (int i = 0; i < enumValueArray.Length; i++)
                {
                    if (enumValue != null)
                        enumValue += "\n";

                    enumValue += $"enum{i}={enumValueArray[i].Trim()}";
                }

                var clParameter = new CLEnumeration() { Name = enumName, Value = enumValue };
                EnumerationList.Add(clParameter);
            }
        }

        public string PrettyPrint(string text)
        {
            //\b\w+\b:(\s*\bLOOP\b)?
            var prettyRegex = new Regex(@"^((?<IncreaseIndent>(\bPACKAGE\b|\bSUBROUTINE\b|\bPARAM_LIST\b|\bCUSTOM\b|\bSEQUENCE\b|\bBLOCK\b|\b\w+\b:\s*\bLOOP\b|\bLOOP\b))(\s+(?<IncreaseKey>\b\w+\b))?|(?<DecreaseIndent>(\bREPEAT\b|\bEND\b))(\s+(?<DecreaseKey>\b\w+\b))?|(?<TempIndent>(&)))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
            var prettyText = new StringBuilder();
            var indentNumber = 0;
            var labelIndent = false;
            text = Regex.Replace(text, @"[^\S\r\n]+", " "); //removes long spaces
            
            var indentDict = new Dictionary<string, int>();

            using (StringReader reader = new StringReader(text))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        var groupName = "";

                        var trimmedLine = line.Trim();
                        var prettyRegexMatch = prettyRegex.Match(trimmedLine);

                        if (prettyRegexMatch.Success)
                        {
                            if (prettyRegexMatch.Groups["IncreaseIndent"].Success)
                            {
                                groupName = "IncreaseIndent";
                                if (prettyRegexMatch.Value.Last() == ':')
                                {
                                    if (labelIndent)
                                        indentNumber--;
                                    labelIndent = true;
                                    trimmedLine = Regex.Replace(trimmedLine, @":\s*", ": ");
                                }
                            }
                            else if (prettyRegexMatch.Groups["DecreaseIndent"].Success)
                            {
                                groupName = "DecreaseIndent";

                                if (labelIndent)
                                    indentNumber--;
                                labelIndent = false;
                            }
                            else if (prettyRegexMatch.Groups["TempIndent"].Success)
                                groupName = "TempIndent";
                        }

                        if (groupName == "DecreaseIndent")
                        {
                            if (prettyRegexMatch.Groups["DecreaseKey"].Success)
                            {
                                if (indentDict.TryGetValue(prettyRegexMatch.Groups["DecreaseKey"].Value, out int indentNumber2))
                                    indentNumber = indentNumber2;
                                else
                                    indentNumber--;
                            }
                            else
                                indentNumber--;
                        }

                        if (groupName == "TempIndent")
                            indentNumber++;

                        //if (string.Equals(prettyRegexMatch.Value, "END", StringComparison.OrdinalIgnoreCase))
                        //    indentNumber = 0; //todo: add package, end isn't necassirly 0 indent
                        if (indentNumber > 0)
                            trimmedLine = new string('\t', indentNumber) + trimmedLine;

                        if (groupName == "TempIndent")
                            indentNumber--;

                        if (groupName == "IncreaseIndent")
                        {
                            if (prettyRegexMatch.Groups["IncreaseKey"].Success)
                            {
                                var dictKey = prettyRegexMatch.Groups["IncreaseKey"].Value;
                                indentDict[dictKey] = indentNumber;
                            }
                            else
                            {
                                var dictKey = prettyRegexMatch.Groups["IncreaseIndent"].Value;
                                indentDict[dictKey] = indentNumber;
                            }

                            indentNumber++;
                        }

                        if (trimmedLine.Length > 0)
                            prettyText.AppendLine(trimmedLine);
                    }

                } while (line != null);
            }

            var newText = prettyText.ToString()
                .RegexReplace("[^\\S\\r\\n]*(?<!\\d(e|E))\\+[^\\S\\r\\n]*", " + ") //ignore "1.9e+10"
                .RegexReplace("(?<!=[^\\S\\r\\n]*)[^\\S\\r\\n]*(?<!(-|\\de|\\dE))-(?!-)[^\\S\\r\\n]*", " - ") //ignore "1.9e-10" and "--comment"
                .RegexReplace("[^\\S\\r\\n]*/[^\\S\\r\\n]*", " / ")
                .RegexReplace("[^\\S\\r\\n]*(?<!\\*)\\*(?!\\*)[^\\S\\r\\n]*", " * ")
                .RegexReplace("[^\\S\\r\\n]*\\*\\*[^\\S\\r\\n]*", "**")
                .RegexReplace("[^\\S\\r\\n]*<(?!(=|>))[^\\S\\r\\n]*", " < ")
                .RegexReplace("[^\\S\\r\\n]*<=[^\\S\\r\\n]*", " <= ")
                .RegexReplace("[^\\S\\r\\n]*(?<!<)>(?!=)[^\\S\\r\\n]*", " > ")
                .RegexReplace("[^\\S\\r\\n]*>=[^\\S\\r\\n]*", " >= ")
                .RegexReplace("[^\\S\\r\\n]*<>[^\\S\\r\\n]*", " <> ")
                .RegexReplace("[^\\S\\r\\n]*(?<!(<|>))=[^\\S\\r\\n]*", " = ")
                .RegexReplace("[^\\S\\r\\n]*\\.\\.[^\\S\\r\\n]*", " .. ")
                .RegexReplace("\\([^\\S\\r\\n]*", "(") //(  1+1) => (1+1)
                .RegexReplace("[^\\S\\r\\n]*\\)", ")"); //(1+1  ) => (1+1)

            return newText;
        }
    }
}
