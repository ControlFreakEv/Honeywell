using Honeywell.Database;
using ScintillaNET;
using ScintillaNET_FindReplaceDialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Honeywell.GUI.Mapper.ScintillaExt;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using Honeywell.TDC;
using System.Text.RegularExpressions;
using System.Net.Mail;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Mapper.Samples;

namespace Honeywell.GUI.Mapper
{
    //todo: depending on which package is selected, find/replace CDS parameters with tag names
    //todo: chaning indicators, add "*" to save
    public partial class CLViewerForm : DockContent
    {
        //public ScintillaExt.ScintillaTextEditor ScintillaTextEditorCL { get; set; }
        #region Fields
        public static FindReplace MyFindReplace { get; set; } = new();
        private static Dictionary<string, int> IndicatorDict = new();
        private const string NEW_DOCUMENT_TEXT = "Untitled";
        private const int LINE_NUMBERS_MARGIN_WIDTH = 30;
        private string selectedTdcTag = string.Empty;
        /// <summary>
        /// the background color of the text area
        /// </summary>
        private const int BACK_COLOR = 0x2A211C;

        /// <summary>
        /// default text color of the text area
        /// </summary>
        private const int FORE_COLOR = 0xB7B7B7;

        /// <summary>
        /// change this to whatever margin you want the line numbers to show in
        /// </summary>
        private const int NUMBER_MARGIN = 1;

        /// <summary>
        /// change this to whatever margin you want the bookmarks/breakpoints to show in
        /// </summary>
        private const int BOOKMARK_MARGIN = 2;

        private const int BOOKMARK_MARKER = 2;

        /// <summary>
        /// change this to whatever margin you want the code folding tree (+/-) to show in
        /// </summary>
        private const int FOLDING_MARGIN = 3;

        /// <summary>
        /// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
        /// </summary>
        private const bool CODEFOLDING_CIRCULAR = false;
        public CLLexer Lexer { get; set; } = new();
        public string? PackageAttachment { get; set; }
        DbTdcCL dbTdcCL;
        #endregion

        public CLViewerForm(DbTdcCL cl, string? packageAttachment = null)
        {
            InitializeComponent();
            splitContainer1.Panel2.Controls.Add(ScintillaTextEditorCL);

            using var db = new TdcContext();
            dbTdcCL = db.TdcCLs.FirstOrDefault(x => x.Id == cl.Id);

            if (dbTdcCL == null && cl.FileName == SampleCl.SampleClFileName)
            {
                dbTdcCL = cl;
                packageAttachment = cl.CLTagReferences.FirstOrDefault(x => x.CLAttachedToThisPoint)?.Tag?.Name;
            }

            if (Lexer.PrettyPrint(dbTdcCL.Content) == dbTdcCL.OriginalContent && dbTdcCL.Indicators == null)
                OriginalCLToolStripButton.Enabled = false;

            OpenFile(dbTdcCL.Content);

            //MyFindReplace = new FindReplace();
            MyFindReplace.Scintilla = ScintillaTextEditorCL;
            MyFindReplace.KeyPressed += MyFindReplace_KeyPressed;

            ScintillaTextEditorCL.TextChanged += new System.EventHandler(this.ScintillaTextEditorCL_TextChanged);

            PackageToolStripComboBox.Items.Clear();
           
            var attachements = dbTdcCL.CLTagReferences.Where(x => x.CLAttachedToThisPoint);
            if (attachements.Count() > 1)
            {
                foreach (var attachement in attachements)
                    PackageToolStripComboBox.Items.Add(attachement.Tag.Name);
            }
            else
                PackageToolStripComboBox.Enabled = false;

            PackageToolStripComboBox.SelectedIndex = PackageToolStripComboBox.FindString(packageAttachment);

            if (packageAttachment == null && dbTdcCL.CLTagReferences.Count(x => x.CLAttachedToThisPoint) == 1)
            {
                var attachedPoint = dbTdcCL.CLTagReferences.First(x => x.CLAttachedToThisPoint).Tag.Name;
                ReplaceCdsWithTag(attachedPoint);
            }
        }

        private void OpenFile(string text, bool useIndicators = true, bool enablePrettyPrint = true)
        {
            SetScintillaToCurrentOptions();
            if (enablePrettyPrint)
            {
                var prettyPrint = Lexer.PrettyPrint(text);
                ScintillaTextEditorCL.Text = prettyPrint;
            }
            else
                ScintillaTextEditorCL.Text = text;


            this.Text = dbTdcCL.FileName;

            #region Indicators
            if (useIndicators && dbTdcCL.Indicators != null)
            {
                var indicatorArray = dbTdcCL.Indicators.Split(';');
                foreach (var indicator in indicatorArray)
                {
                    var splitIndicator = indicator.Split(',');
                    var index = int.Parse(splitIndicator[0]);
                    var start = int.Parse(splitIndicator[1]);
                    var end = int.Parse(splitIndicator[2]);

                    ScintillaTextEditorCL.IndicatorCurrent = index;
                    ScintillaTextEditorCL.IndicatorFillRange(start, end - start);
                }
            }
            #endregion
        }

        private void CLViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //todo: save back to DB
        }

        private void MyFindReplace_KeyPressed(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            ScintillaNet_KeyDown(sender, e);
        }

        private void ScintillaTextEditorCL_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            var scintilla = sender as ScintillaTextEditor;
            var startPos = scintilla.GetEndStyled();
            var endPos = e.Position;
            //var debug = scintilla.GetTextRange(startPos, endPos);
            Lexer.Style(scintilla, startPos, endPos);
        }

        private void InitBookmarkMargin()
        {
            //TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

            var margin = ScintillaTextEditorCL.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);
            //margin.Cursor = MarginCursor.Arrow;

            var marker = ScintillaTextEditorCL.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);
        }

        private void InitCodeFolding()
        {
            ScintillaTextEditorCL.SetFoldMarginColor(true, IntToMediaColor(BACK_COLOR));
            ScintillaTextEditorCL.SetFoldMarginHighlightColor(true, IntToMediaColor(BACK_COLOR));

            // Enable code folding
            ScintillaTextEditorCL.SetProperty("fold", "1");
            ScintillaTextEditorCL.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            ScintillaTextEditorCL.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            ScintillaTextEditorCL.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            ScintillaTextEditorCL.Margins[FOLDING_MARGIN].Sensitive = true;
            ScintillaTextEditorCL.Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                ScintillaTextEditorCL.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                ScintillaTextEditorCL.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            ScintillaTextEditorCL.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            ScintillaTextEditorCL.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            ScintillaTextEditorCL.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            ScintillaTextEditorCL.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            ScintillaTextEditorCL.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            ScintillaTextEditorCL.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            ScintillaTextEditorCL.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            ScintillaTextEditorCL.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
        }

        private void InitColors()
        {
            ScintillaTextEditorCL.CaretForeColor = Color.White;
            ScintillaTextEditorCL.SetSelectionBackColor(true, IntToMediaColor(0x114D9C));

            //FindReplace.Indicator.ForeColor = System.Drawing.Color.DarkOrange;
        }

        private void InitNumberMargin()
        {
            ScintillaTextEditorCL.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            ScintillaTextEditorCL.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            ScintillaTextEditorCL.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            ScintillaTextEditorCL.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = ScintillaTextEditorCL.Margins[NUMBER_MARGIN];
            nums.Width = LINE_NUMBERS_MARGIN_WIDTH;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;
        }

        private void InitSyntaxColoring()
        {
            // Configure the default style
            ScintillaTextEditorCL.StyleResetDefault();
            ScintillaTextEditorCL.Styles[Style.Default].Font = "Consolas";
            ScintillaTextEditorCL.Styles[Style.Default].Size = 10;
            ScintillaTextEditorCL.Styles[Style.Default].BackColor = IntToColor(0x212121);
            ScintillaTextEditorCL.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            ScintillaTextEditorCL.StyleClearAll();

            ScintillaTextEditorCL.Styles[CLLexer.Default].ForeColor = Color.White; //IntToColor(0x48A8EE);
            ScintillaTextEditorCL.Styles[CLLexer.Keyword].ForeColor = Color.HotPink;
            ScintillaTextEditorCL.Styles[CLLexer.Property].ForeColor = Color.White;
            ScintillaTextEditorCL.Styles[CLLexer.Number].ForeColor = Color.LightGreen;
            ScintillaTextEditorCL.Styles[CLLexer.String].ForeColor = Color.Yellow;
            ScintillaTextEditorCL.Styles[CLLexer.Comment].ForeColor = Color.Green;
            ScintillaTextEditorCL.Styles[CLLexer.Symbol].ForeColor = Color.Gold;
            ScintillaTextEditorCL.Styles[CLLexer.Local].ForeColor = Color.Orange;
            ScintillaTextEditorCL.Styles[CLLexer.External].ForeColor = Color.Cyan;
            ScintillaTextEditorCL.Styles[CLLexer.External].Case = StyleCase.Upper;
            ScintillaTextEditorCL.Styles[CLLexer.Label].ForeColor = Color.Teal;
            ScintillaTextEditorCL.Styles[CLLexer.Function].ForeColor = Color.Khaki;
            ScintillaTextEditorCL.Styles[CLLexer.ExternalMissing].ForeColor = Color.Red;
            ScintillaTextEditorCL.Styles[CLLexer.Directive].ForeColor = Color.Gray;
            ScintillaTextEditorCL.Styles[CLLexer.Enumeration].ForeColor = Color.YellowGreen;
            ScintillaTextEditorCL.Styles[CLLexer.ParameterList].ForeColor = Color.YellowGreen;
            ScintillaTextEditorCL.Styles[CLLexer.CustomDataSegment].ForeColor = Color.SkyBlue;

            // Define an indicator
            ScintillaTextEditorCL.Indicators[CLLexer.Function].Style = IndicatorStyle.Dots;
            ScintillaTextEditorCL.Indicators[CLLexer.Function].ForeColor = Color.White;

            ScintillaTextEditorCL.Lexer = ScintillaNET.Lexer.Container;
        }

        /// <summary>
        /// Converts a Win32 colour to a Drawing.Color
        /// </summary>
        /// <param name="rgb">A Win32 color.</param>
        /// <returns>A System.Drawing color.</returns>
        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        /// <summary>
        /// Converts a Win32 colour to a Media Color
        /// </summary>
        /// <param name="rgb">A Win32 color.</param>
        /// <returns>A System.Media color.</returns>
        public static Color IntToMediaColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void ScintillaNet_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateEbParameters(sender as Scintilla);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (MyFindReplace == null) break;

                    //Clear current highlights
                    MyFindReplace.ClearAllHighlights();

                    //Highlight identical text
                    string sWord = ScintillaTextEditorCL.GetWordFromPosition(ScintillaTextEditorCL.CurrentPosition);
                    if (!string.IsNullOrEmpty(sWord))
                        MyFindReplace.FindAll(sWord, false, true);

                    break;

                default:
                    break;
            }
        }

        private void SetScintillaToCurrentOptions()
        {
            ScintillaTextEditorCL.KeyDown += ScintillaNet_KeyDown;
            ScintillaTextEditorCL.MouseDown += ScintillaNet_MouseDown;
            ScintillaTextEditorCL.DwellStart += ScintillaNet_DwellStart;
            ScintillaTextEditorCL.DwellEnd += ScintillaNet_DwellEnd;
            ScintillaTextEditorCL.MouseDwellTime = 1000;

            // INITIAL VIEW CONFIG
            ScintillaTextEditorCL.WrapMode = WrapMode.None;
            ScintillaTextEditorCL.IndentationGuides = IndentView.LookBoth;

            // STYLING
            InitColors();
            InitSyntaxColoring();

            // NUMBER MARGIN
            InitNumberMargin();

            // BOOKMARK MARGIN
            InitBookmarkMargin();

            // CODE FOLDING MARGIN
            InitCodeFolding();

            // Turn on line numbers?
            //if (lineNumbersMenuItem.IsChecked)
            //    ScintillaTextEditorCL.Margins[NUMBER_MARGIN].Width = LINE_NUMBERS_MARGIN_WIDTH;
            //else
                ScintillaTextEditorCL.Margins[NUMBER_MARGIN].Width = 0;

            // Turn on white space?
            //if (whitespaceMenuItem.IsChecked)
            //    ScintillaTextEditorCL.ViewWhitespace = WhitespaceMode.VisibleAlways;
            //else
                ScintillaTextEditorCL.ViewWhitespace = WhitespaceMode.Invisible;

            // Turn on word wrap?
            //if (wordWrapMenuItem.IsChecked)
            //    ScintillaTextEditorCL.WrapMode = WrapMode.Word;
            //else
                ScintillaTextEditorCL.WrapMode = WrapMode.None;

            // Show EOL?
            //ScintillaTextEditorCL.ViewEol = endOfLineMenuItem.IsChecked;

            // Set the zoom
            //ScintillaTextEditorCL.Zoom = _zoomLevel;

            ScintillaTextEditorCL.Indicators[0].ForeColor = OrangeToolStripTextBox.BackColor;
            ScintillaTextEditorCL.Indicators[0].Style = IndicatorStyle.RoundBox;
            IndicatorDict[OrangeToolStripTextBox.Text] = 0;

            ScintillaTextEditorCL.Indicators[1].ForeColor = YellowToolStripTextBox.BackColor;
            ScintillaTextEditorCL.Indicators[1].Style = IndicatorStyle.RoundBox;
            IndicatorDict[YellowToolStripTextBox.Text] = 1;

            ScintillaTextEditorCL.Indicators[2].ForeColor = BlueToolStripTextBox.BackColor;
            ScintillaTextEditorCL.Indicators[2].Style = IndicatorStyle.RoundBox;
            IndicatorDict[BlueToolStripTextBox.Text] = 2;

            ScintillaTextEditorCL.Indicators[3].ForeColor = GreenToolStripTextBox.BackColor;
            ScintillaTextEditorCL.Indicators[3].Style = IndicatorStyle.RoundBox;
            IndicatorDict[GreenToolStripTextBox.Text] = 3;

            ScintillaTextEditorCL.Indicators[4].ForeColor = RedToolStripTextBox.BackColor;
            ScintillaTextEditorCL.Indicators[4].Style = IndicatorStyle.RoundBox;
            IndicatorDict[RedToolStripTextBox.Text] = 4;
        }

        private void ScintillaNet_DwellEnd(object sender, DwellEventArgs e)
        {
            var scintilla = sender as Scintilla;
            if (scintilla != null)
                scintilla.CallTipCancel();
        }

        private void ScintillaNet_DwellStart(object sender, DwellEventArgs e)
        {
            UpdateTooltip(sender as Scintilla);
        }

        private void ScintillaNet_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateEbParameters(sender as Scintilla);

            if (e.Control && e.KeyCode == Keys.F)
            {
                MyFindReplace.ShowFind();
                e.SuppressKeyPress = true;
            }
            else if (e.Shift && e.KeyCode == Keys.F3)
            {
                MyFindReplace.Window.FindPrevious();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                MyFindReplace.Window.FindNext();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.H)
            {
                MyFindReplace.ShowReplace();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                MyFindReplace.ShowIncrementalSearch();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveFile();
                e.SuppressKeyPress = true;
            }
            //else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.G)
            //{
            //    GoTo MyGoTo = new GoTo((Scintilla)sender);
            //    MyGoTo.ShowGoToDialog();
            //    e.SuppressKeyPress = true;
            //}
        }

        private void UpdateEbParameters(Scintilla scintilla)
        {
            if (scintilla == null)
                return;

            var pos = scintilla.CurrentPosition;
            var wordStart = scintilla.WordStartPosition(pos, true);
            var style = scintilla.GetStyleAt(wordStart);
            if (style == CLLexer.External)
            {
                var word = GetTagName(scintilla, pos);
                UpdateEbParameters(word);
            }
            else if (style == CLLexer.Local || style == CLLexer.CustomDataSegment)
            {
                UpdateEbParameters(Lexer.AttachedPoint);
            }
        }

        private string GetTagName(Scintilla scintilla, int pos)
        {
            //var pos = scintilla.CurrentPosition;
            var wordStart = scintilla.WordStartPosition(pos, true);
            var style = scintilla.GetStyleAt(wordStart);

            var wordEnd = scintilla.WordEndPosition(pos, true); //todo: update this for HG tags
            var boxChar = (char)scintilla.GetCharAt(wordStart - 1);
            if (boxChar == '$') // does not account for $
            {
                wordStart--; // e.g. $HY01B19

                var hgWordEnd = wordEnd;
                var orginalLine = scintilla.LineFromPosition(wordStart);
                var dotCount = 0;
                while (hgWordEnd <= scintilla.TextLength)
                {
                    var c = (char)scintilla.GetCharAt(hgWordEnd);
                    if (c == '.')
                        dotCount++;

                    var hgStyle = scintilla.GetStyleAt(hgWordEnd);
                    var line = scintilla.LineFromPosition(wordStart);
                    if (hgStyle == CLLexer.Comment || c == ',' || c == ' ' || line > orginalLine || dotCount > 1)
                        break;
                    hgWordEnd++;
                }

                var word2 = scintilla.GetTextRange(wordStart, wordEnd - wordStart).ToUpper().Trim();
                using var db = new TdcContext();
                if (db.TdcTags.Any(x => x.Name == word2))
                    wordEnd = hgWordEnd;
            }

            var word = scintilla.GetTextRange(wordStart, wordEnd - wordStart).ToUpper().Trim();

            return word;
        }

        private void UpdateEbParameters(string word)
        {
            if (word != null && selectedTdcTag != word)
            {
                selectedTdcTag = word;
                using var db = new TdcContext();
                var tag = db.TdcTags.FirstOrDefault(x => x.Name == selectedTdcTag);

                if (dbTdcCL.FileName == SampleCl.SampleClFileName)
                    tag = SampleCl.SampleTags.FirstOrDefault(x => x.Name == selectedTdcTag);

                if (tag == null)
                    return;
                ParameterDgvForm.UpdateDataSource(Program.MainForm.MainParameterForm, tag.Params.OrderBy(x => x.SortName).ToList(), true, true);
                CLOnTagDgvForm.UpdateDataSource(Program.MainForm.MainClOnTagForm, tag.Name, tag.CLTagReferences.Select(x => x.CL).OrderBy(x => x.FileName).ToList(), true);
            }
        }

        private void UpdateTooltip(Scintilla scintilla)
        {
            if (scintilla == null)
                return;

            Point point = MousePosition;
            var cor = scintilla.PointToClient(point);
            var pos = scintilla.CharPositionFromPoint(cor.X, cor.Y);
            var style = scintilla.GetStyleAt(pos);

            string tooltip = null;
            if (style == CLLexer.Function)
            {
                var word = scintilla.GetWordFromPosition(pos);
                var function = CLFunctions.Functions.FirstOrDefault(x => x.Name.Equals(word, StringComparison.OrdinalIgnoreCase));
                if (function == null)
                {
                    var start = scintilla.WordStartPosition(pos, true);
                    var start2 = scintilla.WordStartPosition(start - 1, true);
                    var end = scintilla.WordEndPosition(pos, true);
                    var word2 = scintilla.GetTextRange(start2, end - start2);
                    function = CLFunctions.Functions.FirstOrDefault(x => x.Name.Equals(word2, StringComparison.OrdinalIgnoreCase));
                    if (function == null)
                        return;
                }

                tooltip = function.Description;
            }
            else if (style == CLLexer.Enumeration)
            {
                var word = scintilla.GetWordFromPosition(pos);
                tooltip = Lexer.EnumerationList.Where(x => x.Name.Equals(word, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Value;
            }
            else if (style == CLLexer.ParameterList)
            {
                var word = scintilla.GetWordFromPosition(pos);
                tooltip = GetParamListToolip(word);
            }
            else if (style == CLLexer.CustomDataSegment)
            {
                //todo: what to do when same cds parameter e.g. multiple lists have "PV"
                var word = scintilla.GetWordFromPosition(pos);
                tooltip = GetParamToolip(word);
            }
            else if (style == CLLexer.Local)
            {
                var word = scintilla.GetWordFromPosition(pos);
                tooltip = Lexer.LocalList.Where(x => x.Name.Equals(word, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Type ?? "Number";
            }
            else if (style == CLLexer.External)
            {
                var word = GetTagName(scintilla, pos);
                using var db = new TdcContext();
                var tag = db.TdcTags.FirstOrDefault(x => x.Name == word);
                if (tag != null)
                    tooltip = $"{tag.PointType}\n{tag.LcnAddress}";
            }

            if (tooltip != null)
                scintilla.CallTipShow(pos, tooltip);
        }

        private string GetParamToolip(string word, int recursion = 0)
        {
            string tooltip = null;
            var parameter = Lexer.ClParameterList.FirstOrDefault(x => x != null && x.Name.Equals(word, StringComparison.OrdinalIgnoreCase));

            if (parameter != null)
            {
                //if (parameter.ParamList != null)
                //    tooltip += $"Defined in PARAM_LIST {parameter.ParamList}";

                if (parameter.Type != null)
                {
                    tooltip += $"Type - {parameter.Type}";

                    var enumeration = Lexer.EnumerationList.Where(x => x.Name.Equals(parameter.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Value;
                    if (enumeration != null)
                    {
                        foreach (var enumValue in enumeration.Split('\n'))
                        {
                            tooltip += $"\n\t{enumValue}";
                        }
                    }
                    else
                    {
                        var tooltip2 = GetParamListToolip(parameter.Type, recursion + 1);
                        if (tooltip2 != null)
                            tooltip += $"\n{tooltip2}";
                    }
                }
                else
                    tooltip += $"Type - Number";
            }

            return AddIndentToTooltip(tooltip, recursion);
        }

        private string GetParamListToolip(string paramList, int recursion = 0)
        {
            string tooltip = null;
            var clParameters = Lexer.ClParameterList.Where(x => x.ParamList != null && x.ParamList.Equals(paramList, StringComparison.OrdinalIgnoreCase));
            if (clParameters.Any())
            {
                tooltip += $"PARAM_LIST {paramList}";
                foreach (var clParam in clParameters)
                {
                    tooltip += $"\n\t{clParam.Name}";
                    tooltip += "\n" + GetParamToolip(clParam.Name, recursion + 1);
                }
            }
            return AddIndentToTooltip(tooltip, recursion);
        }

        private string AddIndentToTooltip(string tooltip, int recursion)
        {
            if (recursion > 0 && tooltip != null)
            {
                var stringbuilder = new StringBuilder();
                var indent = new string('\t', recursion);
                using (StringReader reader = new StringReader(tooltip))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        stringbuilder.AppendLine(indent + line);
                    }
                }
                tooltip = stringbuilder.ToString();
            }
            return tooltip;
        }

        private void OriginalCLToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFile(dbTdcCL.OriginalContent, false, false); //todo: make sure indicators go away
        }

        private void ModifiedCLToolStripButton_Click(object sender, EventArgs e)
        {
            if (PackageAttachment == null)
                OpenFile(dbTdcCL.Content);
            else
                ReplaceCdsWithTag(PackageAttachment);
        }

        private void SaveCLToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            using var db = new TdcContext();
            var cl = db.TdcCLs.FirstOrDefault(x => x.Id == dbTdcCL.Id);
            if (cl != null)
            {
                #region Indicators
                string dbIndicator = null;
                var textLength = ScintillaTextEditorCL.TextLength;
                for (int i = 0; i < IndicatorDict.Count; i++)
                {
                    var indicator = ScintillaTextEditorCL.Indicators[i];
                    var bitmapFlag = (1 << indicator.Index);

                    var startPos = 0;
                    var endPos = 0;

                    do
                    {
                        startPos = indicator.Start(endPos);
                        endPos = indicator.End(startPos);

                        var bitmap = ScintillaTextEditorCL.IndicatorAllOnFor(startPos);
                        var filled = ((bitmapFlag & bitmap) == bitmapFlag);
                        if (filled)
                        {
                            // Do stuff with indicator range
                            Debug.WriteLine(ScintillaTextEditorCL.GetTextRange(startPos, (endPos - startPos)));
                            dbIndicator += $"{i},{startPos},{endPos};";
                        }

                    } while (endPos != 0 && endPos < textLength);
                }
                cl.Indicators = dbIndicator?.TrimEnd(';');
                #endregion

                cl.Content = ScintillaTextEditorCL.Text;
                db.Update(cl);
                db.SaveChanges();
                SaveCLToolStripButton.Text = "Save";


            }
        }

        private void PrettyPrintToolStripButton_Click(object sender, EventArgs e)
        {
            var prettyPrint = Lexer.PrettyPrint(ScintillaTextEditorCL.Text);
            ScintillaTextEditorCL.Text = prettyPrint;
        }

        private void ScintillaTextEditorCL_TextChanged(object sender, EventArgs e)
        {
            CLDifferentFromOriginal();
        }

        private void CLDifferentFromOriginal()
        {
            OriginalCLToolStripButton.Enabled = true;
            SaveCLToolStripButton.Text = "*Save";
        }

        private void CommentToolStripButton_Click(object sender, EventArgs e)
        {
            var newLine = "\r\n";

            var startIndex = ScintillaTextEditorCL.SelectionStart;
            int endIndex = ScintillaTextEditorCL.SelectionEnd;

            var newLineIndex = ScintillaTextEditorCL.Text.LastIndexOf(newLine, startIndex);
            if (newLineIndex != -1)
            {
                ScintillaTextEditorCL.SetSelection(newLineIndex, endIndex);
                startIndex = newLineIndex + newLine.Length;
            }
            else
            {
                ScintillaTextEditorCL.SetSelection(0, endIndex);
                startIndex = 0;
            }

            var text = ScintillaTextEditorCL.SelectedText.Replace("\r\n", "\r\n--");
            endIndex -= ScintillaTextEditorCL.SelectedText.Length - text.Length;

            if (startIndex == 0 && ScintillaTextEditorCL.SelectedText.Length > 1 && ScintillaTextEditorCL.SelectedText[..2] != "--")
            {
                text = "--" + text;
                endIndex += 2;
            }

            ScintillaTextEditorCL.ReplaceSelection(text);
            ScintillaTextEditorCL.SetSelection(startIndex, endIndex);
        }

        private void UncommentTolStripButton_Click(object sender, EventArgs e)
        {
            var newLine = "\r\n";

            var startIndex = ScintillaTextEditorCL.SelectionStart;
            int endIndex = ScintillaTextEditorCL.SelectionEnd;

            var newLineIndex = ScintillaTextEditorCL.Text.LastIndexOf(newLine, startIndex);
            if (newLineIndex != -1)
            {
                ScintillaTextEditorCL.SetSelection(newLineIndex, endIndex);
                startIndex = newLineIndex + newLine.Length;
            }
            else
            {
                ScintillaTextEditorCL.SetSelection(0, endIndex);
                startIndex = 0;
            }
               
            var text = ScintillaTextEditorCL.SelectedText.Replace(newLine + "--", newLine);
            endIndex -= ScintillaTextEditorCL.SelectedText.Length - text.Length;

            if (startIndex == 0 && ScintillaTextEditorCL.SelectedText.Length > 1 && ScintillaTextEditorCL.SelectedText[..2] == "--")
            {
                text = text[2..];
                endIndex -= 2;
            }

            ScintillaTextEditorCL.ReplaceSelection(text);
            ScintillaTextEditorCL.SetSelection(startIndex, endIndex);
        }

        private void HighlightoolStripButton_Click(object sender, EventArgs e)
        {
            for (int i = ScintillaTextEditorCL.SelectionStart; i <= ScintillaTextEditorCL.SelectionEnd; i++)
            {
                for (int k = 0; k < IndicatorDict.Count; k++) //4 is the # of indicators defined
                {
                    ScintillaTextEditorCL.IndicatorCurrent = k;
                    ScintillaTextEditorCL.IndicatorClearRange(i, 1);
                }
            }

            var key = HighlightToolStripDropDownButton.Text;
            if (key != "None")
            {
                ScintillaTextEditorCL.IndicatorCurrent = IndicatorDict[key];
                ScintillaTextEditorCL.IndicatorFillRange(ScintillaTextEditorCL.SelectionStart, ScintillaTextEditorCL.SelectedText.Length);
            }

            CLDifferentFromOriginal();
        }

        private void NoneToolStripTextBox_Click(object sender, EventArgs e)
        {
            ToolStripTextBoxClick(sender);
        }

        private void OrangeToolStripTextBox_Click(object sender, EventArgs e)
        {
            ToolStripTextBoxClick(sender);
        }

        private void YellowToolStripTextBox_Click(object sender, EventArgs e)
        {
            ToolStripTextBoxClick(sender);
        }

        private void BlueToolStripTextBox_Click(object sender, EventArgs e)
        {
            ToolStripTextBoxClick(sender);
        }

        private void GreenToolStripTextBox_Click(object sender, EventArgs e)
        {
            ToolStripTextBoxClick(sender);
        }

        private void RedToolStripTextBox_Click(object sender, EventArgs e)
        {
            ToolStripTextBoxClick(sender);
        }

        private void ToolStripTextBoxClick(object sender)
        {
            var key = ((ToolStripMenuItem)sender).Text;
            HighlightToolStripDropDownButton.Text = key;
            HighlightToolStripDropDownButton.BackColor = ((ToolStripMenuItem)sender).BackColor;
            HighlightToolStripDropDownButton.HideDropDown();
        }

        private void FindToolStripButton_Click(object sender, EventArgs e)
        {
            MyFindReplace.ShowFind();
        }

        private void ReplaceCdsWithTag(string attachedPoint)
        {
            //todo: adds extra tags for EDC_DENS, also CDS in 1 CL could appear in another CL
            using var db = new TdcContext();
            var cl = db.TdcCLs.FirstOrDefault(x => x.Id == dbTdcCL.Id);

            if (cl == null && dbTdcCL.FileName == SampleCl.SampleClFileName)
                cl = dbTdcCL;

            var attachedTag = cl.CLTagReferences.FirstOrDefault(x => x.Tag.Name == attachedPoint)?.Tag;
            var content = dbTdcCL.OriginalContent;

            var newExternalTags = new List<string>();
            foreach (var cds in attachedTag.Params.Where(x => x.CDS))
            {
                Regex regexCds = new(@"(?<!(--.*))(\b" + cds.Name + @"\b)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                content = regexCds.Replace(content, cds.Value);

                Regex regexparam = new(@"(?<!(--.*))(\bPARAMETER\b\s+\b" + cds.Value + @"\b)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                content = regexparam.Replace(content, "PARAMETER " + cds.Name);

                if (!newExternalTags.Contains(cds.Value))
                    newExternalTags.Add(cds.Value);
            }

            var regexBlock = new Regex(@"(?<!(--.*))\bBlock\b\s*(?<tag>\w+)", RegexOptions.IgnoreCase);
            var pointMatch = regexBlock.Match(content);

            #region update external statement
            if (newExternalTags.Any())
            {
                var externalStatement = Environment.NewLine + "\tEXTERNAL " + newExternalTags.Aggregate((x, y) => x + ", " + y) + " --added by Parser";
                var externalIndex = content.IndexOf(Environment.NewLine, pointMatch.Index + pointMatch.Length);

                while (content[(externalIndex + 1)..].Trim()[0] == '&')
                    externalIndex = content.IndexOf(Environment.NewLine, ++externalIndex);

                content = content.Insert(externalIndex, externalStatement);
            }
            #endregion

            #region update block statement
            var genericIndex = content.IndexOf("GENERIC", pointMatch.Index + pointMatch.Length, StringComparison.OrdinalIgnoreCase);
            if (genericIndex != -1)
            {
                var semiColonIndex = content.IndexOf(";", genericIndex + 7);
                var parenthesisIndex = content.IndexOf(")", genericIndex + 7);
                var index = semiColonIndex != -1 ? semiColonIndex : parenthesisIndex;
                index = parenthesisIndex < index ? parenthesisIndex : index;
                content = content.Remove(genericIndex, index - genericIndex); //remove generic...
                content = content.Insert(genericIndex, $"POINT {attachedTag.Name}");
            }
            #endregion

            OpenFile(content);
        }

        private void PackageToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PackageAttachment = PackageToolStripComboBox.Text;
            if (PackageAttachment == null)
                return;

            ReplaceCdsWithTag(PackageAttachment);
            var tags = TagsInCL.GetCLTags(dbTdcCL, PackageAttachment);
            Program.MainForm.MainTagsOnClForm.UpdateDataSource(tags);
        }
    }
}
