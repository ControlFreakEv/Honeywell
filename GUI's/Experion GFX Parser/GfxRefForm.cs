using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Honeywell.HMIWeb;
using System.Linq;
using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.Windows.Forms;
//using ExperionGfx;
using System.Diagnostics;

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
            ExportToXlsx();
        }

        private string GetXlsxPath() => $@"{GfxPath.Text}\Associated Displays.xlsx";

        private void ExportToXlsx()
        {
            if (AssociatedDisplayDataGrid.Rows.Count == 0 || AssociatedDisplayDataGrid.DataSource is not DataTable table)
            {
                MessageBox.Show("Nothing to Export. Click 'Go' first.");
                return;
            }

            string filePath = GetXlsxPath();
            try
            {
                ShapeWorkbook.Export(table, GetDataTableMapping(), filePath);
                MessageBox.Show($"File saved at \n{filePath}");
            }
            catch (IOException)
            {
                MessageBox.Show($"Close {filePath} and try again.");
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            ImportXlxs();
        }

        private void ImportXlxs()
        {
            string filePath = GetXlsxPath();
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"{filePath} does not exist");
                return;
            }

            try
            {
                var shapes = ShapeWorkbook.Import(filePath, GetDataTableMapping(), header =>
                    MessageBox.Show($"Could not find header {header}", "Warning: Missing Column", MessageBoxButtons.OKCancel) != DialogResult.Cancel);
                if (shapes != null)
                    PopulateDataGrid(shapes);
            }
            catch (IOException)
            {
                MessageBox.Show($"Close {filePath} and try again.");
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(ex.Message);
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

            var dt = ((DataTable?)AssociatedDisplayDataGrid.DataSource);
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

                                        var elementNode = dataObject.Ancestors("element").FirstOrDefault();
                                        if (elementNode != null && (dataPoint != null || dataParameter != null))
                                        {
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

                                        var elementNode = dataObject.Ancestors("element").FirstOrDefault();
                                        if (elementNode != null && scriptPoints != null && scriptParameters != null)
                                        {
                                            for (int i = 0; i < scriptPoints.Length; i++)
                                            {
                                                var scriptParameter = i < scriptParameters.Length ? scriptParameters[i] : null;
                                                if (scriptPoints[i].ToStringOrNull() != null || scriptParameter?.ToStringOrNull() != null)
                                                {
                                                    var contentNode = elementNode.Descendants("content").FirstOrDefault();
                                                    var shapeId = CustomShape.GetShapeId(contentNode?.Value);

                                                    var customPropPoint = CustomShape.GetCustomProperty(scriptPoints[i]);
                                                    var customPropParam = CustomShape.GetCustomProperty(scriptParameter);

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
                        foreach (var tooltipNode in tooltipNodes ?? Enumerable.Empty<HtmlAgilityPack.HtmlNode>())
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
                                var node = htmlDoc.DocumentNode.SelectNodes(xPath)?.FirstOrDefault();

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
                                        if (node == null)
                                            break;

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

                                        if (shape.ShapePath?.Contains(pathSeparator) == true)
                                        {
                                            shape.CustomParamType = shape.DataType;
                                            shape.DataType = Shape.ObjectType.CustomParameter;

                                            var shapeId = shape.ShapePath.Split(pathSeparator)[^1].Trim();
                                            var customShapes = customShapeList
                                                .Where(x => x.ShapeName.ToUpper() == shape.ShapeSource?.ToUpper())
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

                                    var subNode = htmlDoc.DocumentNode.SelectNodes(xPath)?.FirstOrDefault();
                                    if (subNode == null)
                                        continue;
                                    if (customShape.PointName != null)
                                        shape.Point = subNode.InnerText.Trim();
                                    else if (customShape.ParameterName != null)
                                        shape.Parameter = subNode.InnerText.Trim();


                                    while (subNode.Id != "Page")
                                    {
                                        if (shape.ShapePath == null)
                                            shape.ShapePath = subNode.Id;
                                        else
                                            shape.ShapePath = $"{subNode.Id} {pathSeparator} {shape.ShapePath}";

                                        subNode = subNode.ParentNode;
                                        if (subNode == null)
                                            break;
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
                                var shapeSrc = Path.GetFileName(src?.Value);
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
                        var tooltipNode = htmlDoc.DocumentNode.SelectSingleNode(xPath)
                            ?? throw new InvalidOperationException($"Can't find shape '{shape.ShapeName}' in graphic '{gfxName}' to set its tooltip");
                        tooltipNode.SetAttributeValue("title", shape.NewTooltip);
                    }

                    if (shape.NewCss != null)
                    {
                        var cssNode = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""HDXStylesheet""]")
                            ?? throw new InvalidOperationException($"Can't find the HDXStylesheet node in graphic '{gfxName}' to set its CSS");
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
                                var shapePath = shape.ShapePath
                                    ?? throw new InvalidOperationException($"Shape '{shape.ShapeName}' in graphic '{gfxName}' has no shape path for its embedded custom parameter");
                                var shapeId = shapePath.Split(pathSeparator)[^1].Trim();
                                var xPath = $@"//*[@id=""{shapeId}""]";
                                var node = htmlDoc.DocumentNode.SelectNodes(xPath)?.FirstOrDefault()
                                    ?? throw new InvalidOperationException($"Can't find node '{shapeId}' in graphic '{gfxName}' for an embedded custom parameter of shape '{shape.ShapeName}'");
                                if (node != null)
                                {
                                    if (shape.Point != null && shape.NewPoint != null)
                                    {
                                        node.InnerHtml = node.InnerHtml.ReplaceWholeName(shape.Point, shape.NewPoint);
                                    }
                                        

                                    if (shape.Parameter != null && shape.NewParameter != null)
                                        node.InnerHtml = node.InnerHtml.ReplaceWholeName(shape.Parameter, shape.NewParameter);
                                }

                            }
                            else
                            {
                                string xPath = $@"//*[@id=""{shape.ShapeName}""]";
                                var node = htmlDoc.DocumentNode.SelectNodes(xPath)?.FirstOrDefault();
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
            var shapeReplacementsFromDataGrid = shapesDataGrid.Where(x => x.ReplaceShapeName != null).Where(x => x.ReplaceShapeName!.ToUpper() != x.ShapeSource?.ToUpper());
            if (!shapeReplacementsFromDataGrid.Any())
            {
                MessageBox.Show("Nothing to replace.");
                return;
            }

            var distinctShapeReplacements = shapeReplacementsFromDataGrid.Select(x => x.ReplaceShapeName!.ToUpper()).Distinct();
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
                            var nodeToReplace = htmlDoc.DocumentNode.SelectNodes(xPath)?.FirstOrDefault()
                                ?? throw new InvalidOperationException($"Can't find shape '{shape.ShapeName}' in graphic '{gfxName}' to replace it with '{shape.ReplaceShapeName}'");
                            var insertionPoint = nodeToReplace.ParentNode
                                ?? throw new InvalidOperationException($"Shape '{shape.ShapeName}' in graphic '{gfxName}' has no parent node");
                            var newNode = replacement.ReplaceShape(nodeToReplace, gfxFilesPath, ResizeCheckBox.Checked);
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
                PropertyDescriptor? prop = properties[item.Key];
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