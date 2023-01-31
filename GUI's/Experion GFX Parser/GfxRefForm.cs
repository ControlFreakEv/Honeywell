using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
using Honeywell.HMIWeb;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.Windows.Forms;
using Org.BouncyCastle.Crypto.Parameters;
using NPOI.POIFS.Crypt;
//using ExperionGfx;
using NPOI.HPSF;
using System.Diagnostics;
//using NPOI.SS.Formula.Functions;

namespace Honeywell.GUI.ExperionGfx
{
    public partial class GfxRefForm : Form
    {
        string pathSeparator = "/";
        public GfxRefForm()
        {
            InitializeComponent();
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            GetParallelShapeRefs();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            //ExportToCsv();
            ExportToXlsx();
        }

        private string GetXlsxPath() => $@"{GfxPath.Text}\Associated Displays.xlsx";
        private string XlsxSheet = "Gfx Refs";

        private void ExportToXlsx()
        {
            string filePath = GetXlsxPath();
            try
            {
                if (AssociatedDisplayDataGrid.Rows.Count == 0)
                {
                    MessageBox.Show("Nothing to Export. Click 'Go' first.");
                    return;
                }

                //create workbook
                IWorkbook workbook = new XSSFWorkbook();
                int colIndex = 0;
                int rowIndex = 0;
                ISheet ws = workbook.CreateSheet(XlsxSheet);
                IRow row = ws.CreateRow(rowIndex++);

                //create headers
                var dict = GetDataTableMapping();
                var numericColumnIndex = new List<int>();
                foreach (var kv in dict)
                {
                    if (kv.Key == nameof(Shape.CsvIndex) || kv.Key == nameof(Shape.UniqueShapeId))
                        numericColumnIndex.Add(colIndex);

                    row.CreateCell(colIndex++).SetCellValue(kv.Value);
                }

                //populate data
                var dt = (DataTable)AssociatedDisplayDataGrid.DataSource;
                foreach (DataRow dr in dt.Rows)
                {
                    row = ws.CreateRow(rowIndex++);
                    colIndex = 0;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        var cell = row.CreateCell(colIndex);

                        var cellValue = dr[dc].ToString();
                        //set value
                        if (numericColumnIndex.Contains(colIndex))
                        {
                            if (int.TryParse(cellValue, out int numericValue))
                                cell.SetCellValue(numericValue);
                            else
                                cell.SetCellValue(cellValue);
                        }
                        else
                            cell.SetCellValue(cellValue);

                        colIndex++;
                    }
                }

                //format sheets
                ws.CreateFreezePane(0, 1, 0, 1);
                ws.SetAutoFilter(new CellRangeAddress(0, ws.LastRowNum, 0, --colIndex));
                //for (int i = 0; i <= colIndex; i++)
                //    ws.AutoSizeColumn(i);


                //save file
                using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write);
                workbook.Write(stream, false);
                

                MessageBox.Show($"File saved at \n{filePath}");
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("The process cannot access the file"))
                    MessageBox.Show($"Close {filePath} and try again.");
            }
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void ExportToCsv()
        {
            string filePath = $@"{GfxPath.Text}\Associated Displays.csv";
            try
            {
                if (AssociatedDisplayDataGrid.Rows.Count == 0)
                {
                    MessageBox.Show("Nothing to Export. Click 'Go' first.");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                var dt = (DataTable)AssociatedDisplayDataGrid.DataSource;
                IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in dt.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field?.ToString()?.Replace("\"", "\"\""), "\""));
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(filePath, sb.ToString());
                MessageBox.Show($"File saved at \n{filePath}");
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("The process cannot access the file"))
                    MessageBox.Show($"Close {filePath} and try again.");
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            //ImportCsv();
            ImportXlxs();
        }

        private void ImportXlxs()
        {
            string filePath = GetXlsxPath();
            if (!File.Exists(filePath))
                MessageBox.Show($"{filePath} does not exist");
            else
            {
                var shapeList = new ConcurrentBag<Shape>();
                

                //open workbook
                using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                IWorkbook workbook = new XSSFWorkbook(stream);
                var ws = workbook.GetSheet(XlsxSheet);
                if (ws == null)
                    throw new Exception($"Could not locate worksheet {XlsxSheet}");

                //locate headers
                var headerMap = GetDataTableMapping();
                var headerDict = new Dictionary<string, int>();
                var headerRow = ws.GetRow(0);
                foreach (KeyValuePair<string, string> kv in headerMap)
                {
                    var header = headerRow.Cells.FirstOrDefault(x => x.StringCellValue == kv.Value);
                    if (header?.StringCellValue == null)
                    {
                        var response = MessageBox.Show($"Could not find header {kv.Value}", "Warning: Missing Column", MessageBoxButtons.OKCancel);
                        if (response == DialogResult.Cancel)
                            return;
                    }
                    headerDict[kv.Key] = header?.ColumnIndex ?? -1;
                }

                //for (int k = 1; k <= ws.LastRowNum; k++)
                Parallel.For(1, ws.LastRowNum + 1, k =>
                {
                    var row = ws.GetRow(k);
                    var shapeSource = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.ShapeSource)]);
                    var graphic = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.Graphic)]);
                    var shapeName = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.ShapeName)]);
                    var shapePath = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.ShapePath)]);
                    var point = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.Point)]);
                    var parameter = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.Parameter)]);
                    var dataType = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.DataType)]);
                    var dataObjectId = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.DataObjectId)]);
                    var bindingId = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.BindingId)]);
                    var newPoint = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.NewPoint)]);
                    var newParameter = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.NewParameter)]);
                    var customParamPointName = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.CustomParamPointName)]);
                    var customParamParameterName = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.CustomParamParameterName)]);
                    var replaceShapeName = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.ReplaceShapeName)]);
                    var customParamType = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.CustomParamType)]);
                    var csvIndex = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.CsvIndex)]);
                    var miscPropValue = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.MiscPropValue)]);
                    var customPropMiscName = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.CustomPropMiscName)]);
                    var newMiscPropValue = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.NewMiscPropValue)]);
                    var gfxAndShape = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.GfxAndShape)]);
                    var gfxAndShapeId = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.UniqueShapeId)]);
                    var tooltip = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.Tooltip)]);
                    var newTooltip = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.NewTooltip)]);
                    var css = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.Css)]);
                    var newCss = row.Cells.FirstOrDefault(x => x.ColumnIndex == headerDict[nameof(Shape.NewCss)]);

                    var shape = new Shape()
                    {
                        ShapeSource = shapeSource.ConvertToString(),
                        Graphic = graphic.ConvertToString(),
                        ShapeName = shapeName.ConvertToString(),
                        ShapePath = shapePath.ConvertToString(),
                        Point = point.ConvertToString(),
                        Parameter = parameter.ConvertToString(),
                        DataType = Shape.ParseObjectType(dataType.ConvertToString()),
                        DataObjectId = dataObjectId.ConvertToString(),
                        BindingId = bindingId.ConvertToString(),
                        NewPoint = newPoint.ConvertToString(),
                        NewParameter = newParameter.ConvertToString(),
                        CustomParamPointName = customParamPointName.ConvertToString(),
                        CustomParamParameterName = customParamParameterName.ConvertToString(),
                        CustomParamType = Shape.ParseObjectType(customParamType.ConvertToString()),
                        CsvIndex = csvIndex.ConvertToInt() ?? 0,
                        ReplaceShapeName = replaceShapeName.ConvertToString(),
                        MiscPropValue = miscPropValue.ConvertToString(),
                        CustomPropMiscName = customPropMiscName.ConvertToString(),
                        NewMiscPropValue = newMiscPropValue.ConvertToString(),
                        GfxAndShape = gfxAndShape.ConvertToString(),
                        UniqueShapeId = gfxAndShapeId.ConvertToInt(),
                        Tooltip = tooltip.ConvertToString(),
                        NewTooltip = newTooltip.ConvertToString(),
                        Css = css.ConvertToString(),
                        NewCss = newCss.ConvertToString(),
                    };
                    shapeList.Add(shape);
                });

                PopulateDataGrid(shapeList);
            }
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void ImportCsv()
        {
            string filePath = $@"{GfxPath.Text}\Associated Displays.csv";
            if (!File.Exists(filePath))
                MessageBox.Show($"{filePath} does not exist");
            else
            {
                var shapeList = new ConcurrentBag<Shape>();
                var dict = GetDataTableMapping().Keys.ToList();
                var lines = WriteSafeReadAllLines(filePath).Skip(1);
                Parallel.ForEach(lines, csv =>
                {
                    string[] line;
                    if (csv.Contains('"'))
                    {
                        using (TextFieldParser parser = new TextFieldParser(new StringReader(csv)))
                        {
                            parser.TextFieldType = FieldType.Delimited;
                            parser.HasFieldsEnclosedInQuotes = true;
                            parser.SetDelimiters(",");

                            line = parser.ReadFields();
                        }
                    }
                    else
                        line = csv.Split(',');
                    int csvIndex = 0;
                    int.TryParse(line[dict.IndexOf(nameof(Shape.CsvIndex))], out csvIndex);

                    int? gfxAndShapeId = null;
                    if (int.TryParse(line[dict.IndexOf(nameof(Shape.UniqueShapeId))], out int temp))
                        gfxAndShapeId = temp;

                    var shape = new Shape()
                    {
                        ShapeSource = line[dict.IndexOf(nameof(Shape.ShapeSource))],
                        Graphic = line[dict.IndexOf(nameof(Shape.Graphic))],
                        ShapeName = line[dict.IndexOf(nameof(Shape.ShapeName))],
                        ShapePath = line[dict.IndexOf(nameof(Shape.ShapePath))],
                        Point = line[dict.IndexOf(nameof(Shape.Point))],
                        Parameter = line[dict.IndexOf(nameof(Shape.Parameter))],
                        DataType = Shape.ParseObjectType(line[dict.IndexOf(nameof(Shape.DataType))]),
                        DataObjectId = line[dict.IndexOf(nameof(Shape.DataObjectId))],
                        BindingId = line[dict.IndexOf(nameof(Shape.BindingId))],
                        NewPoint = line[dict.IndexOf(nameof(Shape.NewPoint))],
                        NewParameter = line[dict.IndexOf(nameof(Shape.NewParameter))],
                        CustomParamPointName = line[dict.IndexOf(nameof(Shape.CustomParamPointName))],
                        CustomParamParameterName = line[dict.IndexOf(nameof(Shape.CustomParamParameterName))],
                        CustomParamType = Shape.ParseObjectType(line[dict.IndexOf(nameof(Shape.CustomParamType))]),
                        CsvIndex = csvIndex,
                        ReplaceShapeName = line[dict.IndexOf(nameof(Shape.ReplaceShapeName))],
                        MiscPropValue = line[dict.IndexOf(nameof(Shape.MiscPropValue))],
                        CustomPropMiscName = line[dict.IndexOf(nameof(Shape.CustomPropMiscName))],
                        NewMiscPropValue = line[dict.IndexOf(nameof(Shape.NewMiscPropValue))],
                        GfxAndShape = line[dict.IndexOf(nameof(Shape.GfxAndShape))],
                        UniqueShapeId = gfxAndShapeId,
                    };
                    shapeList.Add(shape);
                });

                PopulateDataGrid(shapeList);
            }
        }

        public string[] WriteSafeReadAllLines(string path)
        {
            using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(csv))
            {
                List<string> file = new List<string>();
                while (!sr.EndOfStream)
                {
                    file.Add(sr.ReadLine());
                }

                return file.ToArray();
            }
        }

        private void PopulateDataGrid(IEnumerable<Shape> shapeList)
        {
            AssociatedDisplayDataGrid.Columns.Clear();
            AssociatedDisplayDataGrid.DataSource = GetDataTable(shapeList);
        }

        private ConcurrentBag<Shape> GetShapesFromDataGrid()
        {
            var shapeList = new ConcurrentBag<Shape>();

            var dt = ((DataTable)AssociatedDisplayDataGrid.DataSource);
            if (dt != null)
            {
                var dtEnumerable = dt.AsEnumerable();//.SkipWhile(x => x[ShapeSourceColumn.Index] == ShapeSourceColumn.HeaderText);

                Parallel.ForEach(dtEnumerable, row =>
                {
                    int.TryParse(row[CsvIndexColumn.Index]?.ToStringOrNull(), out int csvIndex);
                    int.TryParse(row[UniqueShapeIdColumn.Index]?.ToStringOrNull(), out int uniqueShapeIndex);

                    var shape = new Shape()
                    {
                        UniqueShapeId = uniqueShapeIndex,
                        Graphic = row[GraphicColumn.Index]?.ToStringOrNull(),
                        ShapeSource = row[ShapeSourceColumn.Index]?.ToStringOrNull(),
                        ShapeName = row[ShapeNameColumn.Index]?.ToStringOrNull(),
                        ShapePath = row[ShapePathColumn.Index]?.ToStringOrNull(),
                        CustomParamPointName = row[CustomParameterPointNameColumn.Index]?.ToStringOrNull(),
                        CustomParamParameterName = row[CustomParameterParamNameColumn.Index]?.ToStringOrNull(),
                        CustomPropMiscName = row[CustomPropMiscNameColumn.Index]?.ToStringOrNull(),
                        Point = row[PointColumn.Index]?.ToStringOrNull(),
                        Parameter = row[ParameterColumn.Index]?.ToStringOrNull(),
                        MiscPropValue = row[CustomPropMiscValueColumn.Index]?.ToStringOrNull(),
                        Tooltip = row[ToolTipColumn.Index]?.ToStringOrNull(),
                        Css = row[CssColumn.Index]?.ToStringOrNull(),
                        DataType = Shape.ParseObjectType(row[TypeColumn.Index]?.ToStringOrNull()),
                        NewPoint = row[NewPointColumn.Index]?.ToStringOrNull(),
                        NewParameter = row[NewParameterColumn.Index]?.ToStringOrNull(),
                        NewMiscPropValue = row[ReplaceMiscPropColumn.Index]?.ToStringOrNull(),
                        NewTooltip = row[NewToolTipColumn.Index]?.ToStringOrNull(),
                        NewCss = row[NewCssColumn.Index]?.ToStringOrNull(),
                        ReplaceShapeName = row[ReplaceShapeColumn.Index]?.ToStringOrNull(),
                        CustomParamType = Shape.ParseObjectType(row[CustomParameterTypeColumn.Index]?.ToStringOrNull()),
                        GfxAndShape = row[GfxShapeColumn.Index]?.ToStringOrNull(),
                        DataObjectId = row[DataObjectIdColumn.Index]?.ToStringOrNull(),
                        BindingId = row[BindingIdColumn.Index]?.ToStringOrNull(),
                        CsvIndex = csvIndex,
                    };
                    if (shape.ReplaceShapeName != ReplaceShapeColumn.HeaderText)
                        shapeList.Add(shape);
                });
            }
            //else
            //    MessageBox.Show("Nothing in data grid");
           
            return shapeList;
        }

        private void GetParallelShapeRefs()
        {
            if (Directory.Exists(GfxPath.Text))
            {
                var concurrentShapeList = new ConcurrentBag<Shape>();

                var files = Directory.GetFiles(GfxPath.Text, "*.htm");
                Parallel.ForEach(files, filePath =>
                {
                    var gfxName = Path.GetFileNameWithoutExtension(filePath);
                    var gfxFilesPath = $@"{GfxPath.Text}\{gfxName}_files";

                    var dsdPath = $@"{gfxFilesPath}\DS_datasource1.dsd";
                    var bindingsPath = $@"{gfxFilesPath}\Bindings.xml";

                    if (!File.Exists(dsdPath))
                        MessageBox.Show($"{dsdPath} does not exist");
                    else if (!File.Exists(bindingsPath))
                        MessageBox.Show($"{bindingsPath} does not exist");
                    else
                    {
                        var shapeList = new List<Shape>();
                        var addToConcurrentList = new List<Shape>();
                        XDocument xdoc;
                        var customShapeList = new List<CustomShape>();

                        #region Custom Parameter Names
                        var customShapesFiles = Directory.GetFiles(gfxFilesPath, "*.sha"); ;
                        foreach (var shapePath in customShapesFiles)
                        {
                            if (!File.Exists(shapePath))
                                MessageBox.Show($"{shapePath} does not exist");
                            else
                            {
                                var customShapeName = Path.GetFileName(shapePath);
                                xdoc = XDocument.Load(shapePath);

                                var shapeContent = xdoc.Descendants("content");
                                foreach (var contentNode in shapeContent)
                                {
                                    var embeddedCustomParams = CustomShape.GetCustomProperties(contentNode.Value);
                                    if (embeddedCustomParams != null)
                                    {
                                        var shapeId = CustomShape.GetShapeId(contentNode?.Value);
                                        foreach (Match embeddedParam in embeddedCustomParams)
                                        {
                                            if (embeddedParam != null)
                                            {
                                                string? customPropPoint = null;
                                                string? customPropParam = null;
                                                string? customPropMisc = null;

                                                var type = embeddedParam.Groups["Type"].Value.ToUpper();
                                                var value = embeddedParam.Groups["CustomProperty"].Value;
                                                if (type == "POINT")
                                                    customPropPoint = value;
                                                else if (type == "PARAMETER")
                                                    customPropParam = value;
                                                else
                                                    customPropMisc = value;

                                                var customShape = new CustomShape() { ShapeName = customShapeName, SubShapeName = shapeId, PointName = customPropPoint, ParameterName = customPropParam, MiscProp = customPropMisc, DataType = Shape.ObjectType.CustomParameterEmbedded, CsvIndex = 0 };
                                                customShapeList.Add(customShape);
                                            }
                                        }
                                    }
                                }


                                var shapeDataObjects = xdoc.Descendants("dataobject");
                                foreach (var dataObject in shapeDataObjects)
                                {
                                    //var dataObjectType = dataObject.Attribute("type")?.Value;
                                    var propertyAttributes = dataObject.Descendants("property").Attributes();
                                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                                    {
                                        var dataPoint = propertyAttributes.Where(x => x.Value == "PointRefPointName").FirstOrDefault()?.Parent?.Attributes("value")?.FirstOrDefault()?.Value.ToStringOrNull();
                                        var dataParameter = propertyAttributes.Where(x => x.Value == "PointRefParamName").FirstOrDefault()?.Parent?.Attributes("value")?.FirstOrDefault()?.Value.ToStringOrNull();

                                        if (dataPoint != null || dataParameter != null)
                                        {
                                            var elementNode = dataObject.Parent;
                                            while (elementNode?.Name != "element")
                                            {
                                                elementNode = elementNode?.Parent;
                                            }

                                            var contentNode = elementNode.Descendants("content").FirstOrDefault();
                                            var shapeId = CustomShape.GetShapeId(contentNode?.Value);

                                            var customPropPoint = CustomShape.GetCustomProperty(dataPoint);
                                            var customPropParam = CustomShape.GetCustomProperty(dataParameter);

                                            var customShape = new CustomShape { ShapeName = customShapeName, SubShapeName = shapeId, PointName = customPropPoint, ParameterName = customPropParam, DataType = Shape.ObjectType.DataTab, CsvIndex = 0 };
                                            customShapeList.Add(customShape);
                                        }
                                    }
                                    else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                                    {
                                        var scriptPoints = propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").FirstOrDefault()?.Parent?.Attributes("value")?.FirstOrDefault()?.Value.ToStringOrNull()?.Split(',');
                                        var scriptParameters = propertyAttributes.Where(x => x.Value == "CommaDelimitedParameters").FirstOrDefault()?.Parent?.Attributes("value")?.FirstOrDefault()?.Value.ToStringOrNull()?.Split(',');

                                        if (scriptPoints != null && scriptParameters != null)
                                        {
                                            for (int i = 0; i < scriptPoints.Length; i++)
                                            {
                                                if (scriptPoints[i].ToStringOrNull() != null || scriptParameters[i].ToStringOrNull() != null)
                                                {
                                                    var elementNode = dataObject.Parent;
                                                    while (elementNode?.Name != "element")
                                                    {
                                                        elementNode = elementNode?.Parent;
                                                    }

                                                    var contentNode = elementNode.Descendants("content").FirstOrDefault();
                                                    var shapeId = CustomShape.GetShapeId(contentNode?.Value);

                                                    var customPropPoint = CustomShape.GetCustomProperty(scriptPoints[i]);
                                                    var customPropParam = CustomShape.GetCustomProperty(scriptParameters[i]);

                                                    var customShape = new CustomShape { ShapeName = customShapeName, SubShapeName = shapeId, PointName = customPropPoint, ParameterName = customPropParam, DataType = Shape.ObjectType.ScriptTab, CsvIndex = i };
                                                    customShapeList.Add(customShape);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region DSD Parsing
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        xdoc = XDocument.Load(dsdPath);
                        var dataObjects = xdoc.Descendants("dataobject");
                        foreach (var dataObject in dataObjects)
                        {
                            //var dataObjectType = dataObject.Attribute("type")?.Value;
                            var dataObjectId = dataObject.Attribute("id")?.Value;

                            if (dataObjectId != null)
                            {
                                var propertyAttributes = dataObject.Descendants("property").Attributes();
                                if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                                {
                                    var dataPoint = propertyAttributes.Where(x => x.Value == "PointRefPointName").FirstOrDefault()?.Parent?.Value;
                                    var dataParameter = propertyAttributes.Where(x => x.Value == "PointRefParamName").FirstOrDefault()?.Parent?.Value;

                                    if (dataPoint != null && dataParameter != null)
                                    {
                                        var shape = new Shape { Graphic = gfxName, DataObjectId = dataObjectId, Point = dataPoint, Parameter = dataParameter, DataType = Shape.ObjectType.DataTab };
                                        shapeList.Add(shape);
                                    }
                                }
                                else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                                {
                                    var scriptPoints = propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").FirstOrDefault()?.Parent?.Value.Split(',');
                                    var scriptParameters = propertyAttributes.Where(x => x.Value == "CommaDelimitedParameters").FirstOrDefault()?.Parent?.Value.Split(',');

                                    if (scriptPoints != null && scriptParameters != null)
                                    {
                                        for (int i = 0; i < scriptPoints.Length; i++)
                                        {
                                            if (!string.IsNullOrWhiteSpace(scriptPoints[i]))
                                            {
                                                var shape = new Shape { Graphic = gfxName, DataObjectId = dataObjectId, Point = scriptPoints[i], Parameter = scriptParameters[i], DataType = Shape.ObjectType.ScriptTab, CsvIndex = i };
                                                shapeList.Add(shape);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Binding Parsing
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        xdoc = XDocument.Load(bindingsPath);
                        dataObjects = xdoc.Descendants("dataobject");
                        foreach (var dataObject in dataObjects)
                        {
                            var dataObjectId = dataObject.Attribute("objectid")?.Value;
                            var binding = dataObject.Parent;
                            var bindingId = binding?.Attribute("ID")?.Value;

                            var shapes = shapeList.Where(x => x.DataObjectId == dataObjectId);
                            foreach (var shape in shapes)
                            {
                                if (shape != null && bindingId != null)
                                    shape.BindingId = bindingId;
                            }
                        }
                        #endregion

                        #region HTM Parsing
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.Load(filePath);

                        #region CSS
                        var cssNode = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""HDXStylesheet""]");
                        if (cssNode != null)
                        {
                            var cssShape = new Shape
                            {
                                Graphic = gfxName,
                                ShapeName = cssNode.Id,
                                Css = cssNode.Attributes.FirstOrDefault(x => x.Name == "href")?.Value
                            };
                            addToConcurrentList.Add(cssShape);
                        }
                        #endregion

                        #region Toolip
                        var tooltipNodes = htmlDoc.DocumentNode.SelectNodes(@"//*[@title]");
                        foreach (var tooltipNode in tooltipNodes)
                        {
                            var tooltipShape = new Shape
                            {
                                Graphic = gfxName,
                                ShapeName = tooltipNode.Id,
                                Tooltip = tooltipNode.Attributes.FirstOrDefault(x => x.Name == "title")?.Value
                            };
                            addToConcurrentList.Add(tooltipShape);
                        }
                        #endregion

                        for (int i = 0; i < shapeList.Count; i++)
                        {
                            var shape = shapeList[i];
                            if (shape.BindingId == null)
                            {
                                shape.ShapePath = "??";
                                shape.ShapeName = "??";
                            }
                            else
                            {
                                string xPath = $@"//*[contains(@hdxproperties,'HDXBINDINGID:{shape.BindingId};')]";
                                var node = htmlDoc.DocumentNode.SelectNodes(xPath).FirstOrDefault();

                                if (node != null)
                                {
                                    shape.ShapeName = node.Id;
                                    var src = node.Attributes["src"];

                                    while (node.Id != "Page")
                                    {
                                        if (shape.ShapePath == null)
                                            shape.ShapePath = node.Id;
                                        else
                                            shape.ShapePath = $"{node.Id} {pathSeparator} {shape.ShapePath}";

                                        node = node.ParentNode;

                                        var newerSrc = node.Attributes["src"];
                                        if (newerSrc != null)
                                        {
                                            src = newerSrc;
                                            shape.ShapeName = node.Id;
                                        }

                                    }


                                    if (src != null)
                                    {
                                        var srcShape = Path.GetFileName(src.Value);
                                        shape.ShapeSource = srcShape;

                                        if (shape.ShapePath.Contains(pathSeparator))
                                        {
                                            shape.CustomParamType = shape.DataType;
                                            shape.DataType = Shape.ObjectType.CustomParameter;

                                            var shapeId = shape.ShapePath.Split(pathSeparator)[^1].Trim();
                                            var customShapes = customShapeList
                                                .Where(x => x.ShapeName.ToUpper() == shape.ShapeSource.ToUpper())
                                                .Where(x => $"{shape.ShapeName}_{x.SubShapeName}" == shapeId)
                                                .Where(x => x.CsvIndex == shape.CsvIndex)
                                                .Where(x => x.DataType == shape.CustomParamType);

                                            if (customShapes == null || customShapes.Count() > 1)
                                                throw new Exception($"Can't determine shape for {shape.ShapePath}. Contact customer support.");

                                            var customShape = customShapes.FirstOrDefault();
                                            if (customShape == null)
                                                throw new Exception($"Missing shape '{src.Value}'");

                                            shape.CustomParamPointName = customShapes.FirstOrDefault()?.PointName;
                                            shape.CustomParamParameterName = customShapes.FirstOrDefault()?.ParameterName;
                                        }
                                    }
                                }
                            }

                            addToConcurrentList.Add(shape);
                        }

                        //embedded custom params
                        var customParams = customShapeList.Where(x => x.DataType == Shape.ObjectType.CustomParameterEmbedded && (x.PointName != null || x.ParameterName != null));

                        foreach (var customShape in customParams)
                        {
                            var xPath = $@"//*[@src="".\{gfxName}_files\{customShape.ShapeName}""]";
                            var nodes = htmlDoc.DocumentNode.SelectNodes(xPath);
                            if (nodes != null && nodes.Any())
                            {
                                foreach (var node in nodes)
                                {
                                    var shape = new Shape
                                    {
                                        Graphic = gfxName,
                                        CustomParamPointName = customShape.PointName,
                                        CustomParamParameterName = customShape.ParameterName,
                                        DataType = Shape.ObjectType.CustomParameter,
                                        CustomParamType = Shape.ObjectType.CustomParameterEmbedded,
                                        ShapeName = node.Id,
                                        ShapeSource = customShape.ShapeName
                                    };

                                    //shape.Point = shapeList.Where(x => x.ShapeName == shape.ShapeName && x.CustomParamPointName?.ToUpper() == shape.CustomParamPointName?.ToUpper() && x.Point != null)?.FirstOrDefault()?.Point;
                                    //shape.Parameter = shapeList.Where(x => x.ShapeName == shape.ShapeName && x.CustomParamParameterName?.ToUpper() == shape.CustomParamParameterName?.ToUpper() && x.Parameter != null)?.FirstOrDefault()?.Parameter;

                                    xPath = $@"//*[@id=""{node.Id}_{customShape.SubShapeName}""]";

                                    var subNode = htmlDoc.DocumentNode.SelectNodes(xPath).FirstOrDefault();
                                    if (customShape.PointName != null)
                                        shape.Point = subNode?.InnerText.Trim();
                                    else if (customShape.ParameterName != null)
                                        shape.Parameter = subNode?.InnerText.Trim();


                                    while (subNode.Id != "Page")
                                    {
                                        if (shape.ShapePath == null)
                                            shape.ShapePath = subNode.Id;
                                        else
                                            shape.ShapePath = $"{subNode.Id} {pathSeparator} {shape.ShapePath}";

                                        subNode = subNode.ParentNode;
                                    }

                                    addToConcurrentList.Add(shape);
                                }
                            }
                        }
                        #endregion

                        #region Add extra custom params
                        string xPathSrc = $@"//*[@src]";
                        var customParamNodes = htmlDoc.DocumentNode.SelectNodes(xPathSrc);
                        if (customParamNodes != null)
                        {
                            foreach (var node in customParamNodes)
                            {
                                var src = node.Attributes["src"];
                                var shapeSrc = Path.GetFileName(src.Value);
                                var parameters = node.Attributes["parameters"]?.Value;
                                if (!string.IsNullOrWhiteSpace(parameters))
                                {
                                    var shapeTemplate = new Shape() { Graphic = gfxName, ShapeName = node.Id, DataType = Shape.ObjectType.CustomParameter, ShapeSource = shapeSrc, CustomParamType = Shape.ObjectType.CustomParameterMisc, };
                                    var allProps = CustomParameter.GetListOfCustomParameters(parameters);
                                    var extraCustomProps = allProps.Where(prop => !addToConcurrentList.Any(shape => shape.CustomParamPointName == prop.Name || shape.CustomParamParameterName == prop.Name || shape.CustomPropMiscName == prop.Name)); //.Where(x => x.Type.ToUpper() != "POINT" && x.Type.ToUpper() != "PARAMETER");

                                    var existingCustomProps = new List<string>();
                                    existingCustomProps.AddRange(addToConcurrentList.Where(x => x.CustomParamPointName != null).Select(x => $"{x.ShapeSource}.{x.CustomParamPointName}"));
                                    existingCustomProps.AddRange(addToConcurrentList.Where(x => x.CustomParamParameterName != null).Select(x => $"{x.ShapeSource}.{x.CustomParamParameterName}"));
                                    existingCustomProps.AddRange(addToConcurrentList.Where(x => x.CustomPropMiscName != null).Select(x => $"{x.ShapeSource}.{x.CustomPropMiscName}"));
                                    existingCustomProps = existingCustomProps.Distinct().ToList();

                                    foreach (var miscProp in allProps)
                                    {
                                        if (existingCustomProps.ContainsNoCase(miscProp.Name))
                                            continue;

                                        var shapeMisc = shapeTemplate.Clone();
                                        shapeMisc.MiscPropValue = miscProp.Value;
                                        shapeMisc.CustomPropMiscName = miscProp.Name;
                                        addToConcurrentList.Add(shapeMisc);
                                    }
                                }
                            }
                        }

                        #endregion

                        foreach (var item in addToConcurrentList)
                            concurrentShapeList.Add(item);
                    }//end of file
                });

                var shapeIdDict = new Dictionary<string, int>();
                var sortOrder = concurrentShapeList.Select(x => new { x.Graphic, x.ShapeName }).Distinct().OrderBy(x => x.ShapeName).ThenBy(x => x.Graphic);

                int i = 0;
                foreach (var item in sortOrder)
                    shapeIdDict[$"{item.Graphic}.{item.ShapeName}"] = i++;

                foreach (var shape in concurrentShapeList)
                {
                    shape.GfxAndShape = $"{shape.Graphic}.{shape.ShapeName}";
                    shape.UniqueShapeId = shapeIdDict[shape.GfxAndShape];
                }

                PopulateDataGrid(concurrentShapeList.AsEnumerable());
            }
            else
                MessageBox.Show("Invalid Path");
        }


        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            IEnumerable<IGrouping<string?, Shape>> groupedShapeList;
            var shapesDataGrid = GetShapesFromDataGrid();
            if (GhostRefCheckBox.Checked) //delete ghost refs
                groupedShapeList = shapesDataGrid.Where(x => x.NewPoint != null || x.NewParameter != null || x.NewMiscPropValue != null || x.NewTooltip != null || x.NewCss != null || x.ShapeName == "??").GroupBy(x => x.Graphic);
            else
                groupedShapeList = shapesDataGrid.Where(x => x.NewPoint != null || x.NewParameter != null || x.NewMiscPropValue != null || x.NewTooltip != null || x.NewCss != null).GroupBy(x => x.Graphic);

            if (!groupedShapeList.Any())
            {
                MessageBox.Show("Nothing to replace.");
                return;
            }

            foreach (var group in groupedShapeList)
            {
                var shapeList = group.ToList();

                #region update parameters to delete
                string nullRefKeyword = "$null";
                foreach (var nullRef in shapeList.Where(x => x.NewPoint != null && x.NewPoint.Equals(nullRefKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewPoint = string.Empty;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewParameter != null && x.NewParameter.Equals(nullRefKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewParameter = string.Empty;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewMiscPropValue != null && x.NewMiscPropValue.Equals(nullRefKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewMiscPropValue = string.Empty;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewCss != null && x.NewCss.Equals(nullRefKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewCss = string.Empty;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewTooltip != null && x.NewTooltip.Equals(nullRefKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewTooltip = string.Empty;
                }

                string nullRefEscapeKeyword = @"\$null";
                foreach (var nullRef in shapeList.Where(x => x.NewPoint != null && x.NewPoint.Equals(nullRefEscapeKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewPoint = nullRefKeyword;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewParameter != null && x.NewParameter.Equals(nullRefEscapeKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewParameter = nullRefKeyword;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewMiscPropValue != null && x.NewMiscPropValue.Equals(nullRefEscapeKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewMiscPropValue = nullRefKeyword;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewCss != null && x.NewCss.Equals(nullRefEscapeKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewCss = nullRefKeyword;
                }

                foreach (var nullRef in shapeList.Where(x => x.NewTooltip != null && x.NewTooltip.Equals(nullRefEscapeKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    nullRef.NewTooltip = nullRefKeyword;
                }
                #endregion

                var gfxName = shapeList[0].Graphic;
                var htmPath = $@"{GfxPath.Text}\{gfxName}.htm";
                if (!File.Exists(htmPath))
                    MessageBox.Show($"{htmPath} does not exist");
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.Load(htmPath);

                var gfxFilesPath = $@"{GfxPath.Text}\{gfxName}_files";
                var dsdPath = $@"{gfxFilesPath}\DS_datasource1.dsd";
                if (!File.Exists(dsdPath))
                    MessageBox.Show($"{dsdPath} does not exist");
                XDocument xdoc = XDocument.Load(dsdPath);

                var dictScripts = new Dictionary<string, Script>(); //key = xpath

                foreach (Shape shape in shapeList)
                {
                    if (shape.NewTooltip != null)
                    {
                        var xPath = $@"//*[@id=""{shape.ShapeName}""]";
                        var tooltipNode = htmlDoc.DocumentNode.SelectSingleNode(xPath);
                        tooltipNode.SetAttributeValue("title", shape.NewTooltip);
                    }

                    if (shape.NewCss != null)
                    {
                        var cssNode = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""HDXStylesheet""]");
                        cssNode.SetAttributeValue("href", shape.NewCss);
                    }

                    //remove ghost reg
                    if (shape.ShapeName == "??")
                    {
                        #region Ghost Ref
                        var dataObject = xdoc.Root?.Descendants("dataobject").Attributes().Where(x => x.Name == "id" && x.Value.ToUpper() == shape.DataObjectId?.ToUpper()).FirstOrDefault()?.Parent;
                        dataObject?.Remove();
                        #endregion
                    }
                    else
                    {
                        var dataObject = xdoc.Root?.Descendants("dataobject").Attributes().Where(x => x.Name == "id" && x.Value.ToUpper() == shape.DataObjectId?.ToUpper()).FirstOrDefault()?.Parent;
                        var propertyAttributes = dataObject?.Descendants("property").Attributes();
                        if (shape.DataType == Shape.ObjectType.DataTab || shape.CustomParamType == Shape.ObjectType.DataTab) //point on "data" tab
                        {
                            #region Data Tab
                            if (shape.NewPoint != null)
                                propertyAttributes?.Where(x => x.Value == "PointRefPointName").FirstOrDefault()?.Parent?.SetValue(shape.NewPoint);

                            if (shape.NewParameter != null)
                                propertyAttributes?.Where(x => x.Value == "PointRefParamName").FirstOrDefault()?.Parent?.SetValue(shape.NewParameter);
                            #endregion
                        }
                        else if (shape.DataType == Shape.ObjectType.ScriptTab || shape.CustomParamType == Shape.ObjectType.ScriptTab) //point(s) on "script data" tab
                        {
                            #region Script Tab
                            var scriptPoints = propertyAttributes?.Where(x => x.Value == "CommaDelimitedPointNames").FirstOrDefault()?.Parent;
                            var scriptParameters = propertyAttributes?.Where(x => x.Value == "CommaDelimitedParameters").FirstOrDefault()?.Parent;

                            var points = scriptPoints?.Value.Split(',');
                            var parameters = scriptParameters?.Value.Split(',');

                            if (points != null && parameters != null)
                            {
                                if (shape.NewPoint != null)
                                {
                                    points[shape.CsvIndex] = shape.NewPoint;
                                    var newCsvPoints = points.Aggregate((x, y) => $"{x},{y}");
                                    scriptPoints?.SetValue(newCsvPoints);
                                }

                                if (shape.NewParameter != null)
                                {
                                    parameters[shape.CsvIndex] = shape.NewParameter;
                                    var newCsvParameters = parameters.Aggregate((x, y) => $"{x},{y}");
                                    scriptParameters?.SetValue(newCsvParameters);
                                }
                            }
                            #endregion

                            #region Script
                            var oldScriptTag = $@"""{shape.Point}.{shape.Parameter}""".ToUpper();
                            var newScriptTag = $@"""{shape.NewPoint ?? shape.Point}.{shape.NewParameter ?? shape.Parameter}""".ToUpper();
                            var htmlScripts = htmlDoc.DocumentNode.Descendants().Where(x => x.Name.ToUpper() == "SCRIPT").Where(x => x.InnerText.ToUpper().Contains(oldScriptTag));
                            if (htmlScripts != null && htmlScripts.Any())
                            {
                                foreach (var node in htmlScripts)
                                {
                                    if (!dictScripts.ContainsKey(node.XPath))
                                        dictScripts.Add(node.XPath, new Script(node.InnerHtml));

                                    Script script = dictScripts[node.XPath];
                                    script.UpdateReplacements(oldScriptTag, newScriptTag);
                                }
                            }

                            #endregion
                        }

                        if (shape.DataType == Shape.ObjectType.CustomParameter) //custom parameter
                        {
                            if (shape.CustomParamType == Shape.ObjectType.CustomParameterEmbedded)
                            {
                                var shapeId = shape.ShapePath.Split(pathSeparator)[^1].Trim();
                                var xPath = $@"//*[@id=""{shapeId}""]";
                                var node = htmlDoc.DocumentNode.SelectNodes(xPath).FirstOrDefault();
                                if (node != null)
                                {
                                    if (shape.Point != null && shape.NewPoint != null)
                                    {
                                        node.InnerHtml = node.InnerHtml.Replace(shape.Point, shape.NewPoint, StringComparison.OrdinalIgnoreCase);
                                    }
                                        

                                    if (shape.Parameter != null && shape.NewParameter != null)
                                        node.InnerHtml = node.InnerHtml.Replace(shape.Parameter, shape.NewParameter, StringComparison.OrdinalIgnoreCase);
                                }

                            }
                            else
                            {
                                string xPath = $@"//*[@id=""{shape.ShapeName}""]";
                                var node = htmlDoc.DocumentNode.SelectNodes(xPath).FirstOrDefault();
                                if (node == null)
                                    throw new Exception($"Can't find Custom Parameters in Graphic {gfxName} for shape {shape.ShapeName}");

                                var parameters = node.Attributes.Where(x => x.Name == "parameters").FirstOrDefault()?.Value;
                                if (parameters == null)
                                    throw new Exception($"Can't find Custom Parameters in Graphic {gfxName} for shape {shape.ShapeName}");

                                if (shape.NewPoint != null)
                                {
                                    var oldvalue = $"?{shape.CustomParamPointName}:{shape.Point};";
                                    var newValue = $"?{shape.CustomParamPointName}:{shape.NewPoint};";
                                    parameters = parameters.Replace(oldvalue, newValue, StringComparison.OrdinalIgnoreCase);
                                }

                                if (shape.NewParameter != null)
                                {
                                    var oldvalue = $"?{shape.CustomParamParameterName}:{shape.Parameter};";
                                    var newValue = $"?{shape.CustomParamParameterName}:{shape.NewParameter};";
                                    parameters = parameters.Replace(oldvalue, newValue, StringComparison.OrdinalIgnoreCase);
                                }

                                if (shape.NewMiscPropValue != null)
                                {
                                    var oldvalue = $"?{shape.CustomPropMiscName}:{shape.MiscPropValue};";
                                    var newValue = $"?{shape.CustomPropMiscName}:{shape.NewMiscPropValue};";
                                    parameters = parameters.Replace(oldvalue, newValue, StringComparison.OrdinalIgnoreCase);
                                }
                                node.SetAttributeValue("parameters", parameters);
                            }
                        }
                    }
                } //next shape

                foreach (var kvPair in dictScripts)
                {
                    var xPath = kvPair.Key;
                    var script = kvPair.Value;
                    var oldNode = htmlDoc.DocumentNode.Descendants().Where(x => x.XPath == xPath).FirstOrDefault();
                    if (oldNode != null)
                        oldNode.InnerHtml = script.UpdateScript();
                }

                xdoc.Save(dsdPath);
                htmlDoc.Save(htmPath);
            } //next graphic
            MessageBox.Show($"Done Replacing. Files saved at \n {GfxPath.Text}");
      
        }

        private void ReplaceShapeButton_Click(object sender, EventArgs e)
        {
            var shapeReplacements = AssociatedDisplays.GetShapeReplacements(GfxPath.Text, ShapeReplacementTextBox.Text);
            if (!shapeReplacements.Any())
            {
                MessageBox.Show("No shape replacement gfx specified.");
                return;
            }

            var shapesDataGrid = GetShapesFromDataGrid();
            var shapeReplacementsFromDataGrid = shapesDataGrid.Where(x => x.ReplaceShapeName != null).Where(x => x.ReplaceShapeName.ToUpper() != x.ShapeSource?.ToUpper());
            if (!shapeReplacementsFromDataGrid.Any())
            {
                MessageBox.Show("Nothing to replace.");
                return;
            }

            var distinctShapeReplacements = shapeReplacementsFromDataGrid.Select(x => x.ReplaceShapeName.ToUpper()).Distinct();
            var shapeReplacementsDefined = shapeReplacements.Select(y => y.NewShapeName.ToUpper());
            var missingShapes = distinctShapeReplacements.Where(x => !shapeReplacementsDefined.Contains(x)); //x is replacement shape name (all caps). Search shape replacement gfx to verify shape exists
            if (missingShapes.Any())
            {
                MessageBox.Show($"Add the following missing shapes to '{ShapeReplacementTextBox.Text}'\n{missingShapes.Aggregate((x, y) => $"'{x}'\n'{y}'")}");
                return;
            }

            var replaceShapeHtm = Directory.GetFiles(GfxPath.Text, ShapeReplacementTextBox.Text).FirstOrDefault();
            var replaceShapeGfxName = Path.GetFileNameWithoutExtension(replaceShapeHtm);
            var replaceShapeGfxFilesPath = $@"{GfxPath.Text}\{replaceShapeGfxName}_files";

            //todo: test case sensitivity on shape replacement
            Parallel.ForEach(shapeReplacementsFromDataGrid.GroupBy(x => x.Graphic), group =>
            {
                var shapeList = group.Select(x => new { x.Graphic, x.ShapeName, x.ReplaceShapeName }).Distinct().ToList();
                var gfxName = shapeList[0].Graphic;
                var gfxFilesPath = $@"{GfxPath.Text}\{gfxName}_files";
                var htmPath = $@"{GfxPath.Text}\{gfxName}.htm";

                if (!File.Exists(htmPath))
                    MessageBox.Show($"{htmPath} does not exist");

                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.Load(htmPath);

                foreach (var shape in shapeList)
                {
                    var sourceShape = @$"{replaceShapeGfxFilesPath}\{shape.ReplaceShapeName}";
                    var destShape = @$"{gfxFilesPath}\{shape.ReplaceShapeName}";
                    File.Copy(sourceShape, destShape, true);
                    if (!string.IsNullOrEmpty(shape.ReplaceShapeName))
                    {
                        var replacement = shapeReplacements.Where(x => x.NewShapeName.ToUpper() == shape.ReplaceShapeName.ToUpper()).FirstOrDefault();
                        if (replacement != null)
                        {
                            string xPath = $@"//*[@id=""{shape.ShapeName}""]";
                            var nodeToReplace = htmlDoc.DocumentNode.SelectNodes(xPath).FirstOrDefault();
                            var newNode = replacement.ReplaceShape(nodeToReplace, gfxFilesPath, ResizeCheckBox.Checked);
                            var insertionPoint = nodeToReplace.ParentNode;
                            nodeToReplace.Remove();

                            insertionPoint.AppendChild(newNode);

                        }
                    }
                }
                htmlDoc.Save(htmPath);
            });

            MessageBox.Show("Shapes have been replaced");
        }

        public DataTable GetDataTable(IEnumerable<Shape> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Shape));
            DataTable table = new DataTable();

            var dict = GetDataTableMapping();
            foreach (KeyValuePair<string, string> item in dict)
            {
                PropertyDescriptor prop = properties[item.Key];
                //var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var value = item.Value.ToString();
                table.Columns.Add(value);
            }

            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[dict[prop.Name]] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Shape property is key, column header is value
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetDataTableMapping()
        {
            var dict = new Dictionary<string, string>
            {
                { nameof(Shape.UniqueShapeId), UniqueShapeIdColumn.HeaderText },
                { nameof(Shape.Graphic), GraphicColumn.HeaderText },
                { nameof(Shape.ShapeSource), ShapeSourceColumn.HeaderText },
                { nameof(Shape.ShapeName), ShapeNameColumn.HeaderText },
                { nameof(Shape.ShapePath), ShapePathColumn.HeaderText },
                { nameof(Shape.CustomParamPointName), CustomParameterPointNameColumn.HeaderText },
                { nameof(Shape.CustomParamParameterName), CustomParameterParamNameColumn.HeaderText },
                { nameof(Shape.CustomPropMiscName), CustomPropMiscNameColumn.HeaderText },
                { nameof(Shape.Point), PointColumn.HeaderText },
                { nameof(Shape.Parameter), ParameterColumn.HeaderText },
                { nameof(Shape.MiscPropValue), CustomPropMiscValueColumn.HeaderText },
                { nameof(Shape.Tooltip), ToolTipColumn.HeaderText },
                { nameof(Shape.Css), CssColumn.HeaderText },
                { nameof(Shape.DataType), TypeColumn.HeaderText },
                { nameof(Shape.NewPoint), NewPointColumn.HeaderText },
                { nameof(Shape.NewParameter), NewParameterColumn.HeaderText },
                { nameof(Shape.NewMiscPropValue), ReplaceMiscPropColumn.HeaderText },
                { nameof(Shape.NewTooltip), NewToolTipColumn.HeaderText },
                { nameof(Shape.NewCss), NewCssColumn.HeaderText },
                { nameof(Shape.ReplaceShapeName), ReplaceShapeColumn.HeaderText },
                { nameof(Shape.CustomParamType), CustomParameterTypeColumn.HeaderText },
                { nameof(Shape.GfxAndShape), GfxShapeColumn.HeaderText },
                { nameof(Shape.DataObjectId), DataObjectIdColumn.HeaderText },
                { nameof(Shape.BindingId), BindingIdColumn.HeaderText },
                { nameof(Shape.CsvIndex), CsvIndexColumn.HeaderText },
            };

            return dict;
        }

        private void HelpToolStripButton_Click(object sender, EventArgs e)
        {
            var readMe = AppDomain.CurrentDomain.BaseDirectory + "ReadMe.pdf";

            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C start \"\" /max \"{readMe}\""
            };
            Process process = new() { StartInfo = startInfo};
            process.Start();
        }
    }
}