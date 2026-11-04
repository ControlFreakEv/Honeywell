using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace Honeywell.HMIWeb
{
    /// <summary>What a scan found. Problems are displays that had to be skipped because a file is missing.</summary>
    public sealed record DisplayScan(List<Shape> Shapes, List<string> Problems);

    /// <summary>
    /// Reads a folder of Experion displays (each X.htm with its X_files folder) and lists every point, parameter,
    /// tooltip, stylesheet and custom property reference in them.
    /// </summary>
    public static class DisplayScanner
    {
        /// <summary>Shape name given to a data object no shape on the display is bound to (a "ghost reference").</summary>
        public const string GhostShapeName = "??";
        public const string PathSeparator = "/";

        private const string PointNameProperty = "PointRefPointName";
        private const string ParameterNameProperty = "PointRefParamName";
        private const string ScriptPointsProperty = "CommaDelimitedPointNames";
        private const string ScriptParametersProperty = "CommaDelimitedParameters";

        public static DisplayScan Scan(string folder)
        {
            var displays = Directory.GetFiles(folder, "*.htm")
                .AsParallel()
                .AsOrdered()
                .Select(ScanDisplay)
                .ToList();

            var shapes = displays.SelectMany(display => display.Shapes).ToList();
            AssignUniqueShapeIds(shapes);
            return new DisplayScan(shapes, displays.SelectMany(display => display.Problems).ToList());
        }

        public static string FilesFolder(string htmPath) =>
            Path.Join(Path.GetDirectoryName(htmPath), $"{Path.GetFileNameWithoutExtension(htmPath)}_files");

        public static string DataSourcePath(string htmPath) => Path.Join(FilesFolder(htmPath), "DS_datasource1.dsd");

        private static DisplayScan ScanDisplay(string htmPath)
        {
            var dsdPath = DataSourcePath(htmPath);
            var bindingsPath = Path.Join(FilesFolder(htmPath), "Bindings.xml");
            foreach (var requiredFile in new[] { dsdPath, bindingsPath })
                if (!File.Exists(requiredFile))
                    return new DisplayScan(new List<Shape>(), new List<string> { $"{requiredFile} does not exist" });

            var graphic = Path.GetFileNameWithoutExtension(htmPath);
            var customShapes = Directory.GetFiles(FilesFolder(htmPath), "*.sha").SelectMany(ReadCustomShape).ToList();
            var dataShapes = ReadDataObjects(dsdPath, graphic);
            ApplyBindings(bindingsPath, dataShapes);

            var html = new HtmlDocument();
            html.Load(htmPath);

            var shapes = new List<Shape>();
            AddStylesheet(html, graphic, shapes);
            AddTooltips(html, graphic, shapes);
            foreach (var shape in dataShapes)
            {
                LocateOnDisplay(shape, html, customShapes);
                shapes.Add(shape);
            }
            AddEmbeddedCustomParameters(html, graphic, customShapes, shapes);
            AddCustomProperties(html, graphic, shapes);

            return new DisplayScan(shapes, new List<string>());
        }

        private static XElement? Property(IEnumerable<XAttribute> attributes, string name) =>
            attributes.FirstOrDefault(attribute => attribute.Value == name)?.Parent;

        /// <summary>The references inside a custom shape (.sha) file, named by the custom properties they use.</summary>
        private static IEnumerable<CustomShape> ReadCustomShape(string shapePath)
        {
            var shapeName = Path.GetFileName(shapePath);
            var document = XDocument.Load(shapePath);

            foreach (var content in document.Descendants("content"))
            {
                var embedded = CustomShape.GetCustomProperties(content.Value);
                if (embedded == null)
                    continue;

                var shapeId = CustomShape.GetShapeId(content.Value);
                foreach (Match match in embedded)
                {
                    var type = match.Groups["Type"].Value.ToUpper();
                    var value = match.Groups["CustomProperty"].Value;
                    yield return new CustomShape
                    {
                        ShapeName = shapeName,
                        SubShapeName = shapeId,
                        PointName = type == "POINT" ? value : null,
                        ParameterName = type == "PARAMETER" ? value : null,
                        MiscProp = type != "POINT" && type != "PARAMETER" ? value : null,
                        DataType = Shape.ObjectType.CustomParameterEmbedded,
                    };
                }
            }

            static string? ValueAttribute(XElement? property) => property?.Attribute("value")?.Value.ToStringOrNull();

            foreach (var dataObject in document.Descendants("dataobject"))
            {
                var element = dataObject.Ancestors("element").FirstOrDefault();
                if (element == null)
                    continue;

                var shapeId = CustomShape.GetShapeId(element.Descendants("content").FirstOrDefault()?.Value);
                var attributes = dataObject.Descendants("property").Attributes().ToList();

                if (Property(attributes, PointNameProperty) is XElement pointProperty) // point on the "data" tab
                {
                    var point = ValueAttribute(pointProperty);
                    var parameter = ValueAttribute(Property(attributes, ParameterNameProperty));
                    if (point != null || parameter != null)
                    {
                        yield return new CustomShape
                        {
                            ShapeName = shapeName,
                            SubShapeName = shapeId,
                            PointName = CustomShape.GetCustomProperty(point),
                            ParameterName = CustomShape.GetCustomProperty(parameter),
                            DataType = Shape.ObjectType.DataTab,
                        };
                    }
                }
                else if (Property(attributes, ScriptPointsProperty) is XElement scriptPointsProperty) // points on the "script data" tab
                {
                    var points = ValueAttribute(scriptPointsProperty)?.Split(',');
                    var parameters = ValueAttribute(Property(attributes, ScriptParametersProperty))?.Split(',');
                    if (points == null || parameters == null)
                        continue;

                    for (int i = 0; i < points.Length; i++)
                    {
                        var parameter = i < parameters.Length ? parameters[i] : null;
                        if (points[i].ToStringOrNull() == null && parameter?.ToStringOrNull() == null)
                            continue;

                        yield return new CustomShape
                        {
                            ShapeName = shapeName,
                            SubShapeName = shapeId,
                            PointName = CustomShape.GetCustomProperty(points[i]),
                            ParameterName = CustomShape.GetCustomProperty(parameter),
                            DataType = Shape.ObjectType.ScriptTab,
                            CsvIndex = i,
                        };
                    }
                }
            }
        }

        /// <summary>One shape per reference in the display's data source (.dsd) file.</summary>
        private static List<Shape> ReadDataObjects(string dsdPath, string graphic)
        {
            var shapes = new List<Shape>();
            foreach (var dataObject in XDocument.Load(dsdPath).Descendants("dataobject"))
            {
                var dataObjectId = dataObject.Attribute("id")?.Value;
                if (dataObjectId == null)
                    continue;

                var attributes = dataObject.Descendants("property").Attributes().ToList();
                if (Property(attributes, PointNameProperty) is XElement pointProperty) // point on the "data" tab
                {
                    var parameterProperty = Property(attributes, ParameterNameProperty);
                    if (parameterProperty != null)
                        shapes.Add(new Shape { Graphic = graphic, DataObjectId = dataObjectId, Point = pointProperty.Value, Parameter = parameterProperty.Value, DataType = Shape.ObjectType.DataTab });
                }
                else if (Property(attributes, ScriptPointsProperty) is XElement scriptPointsProperty) // points on the "script data" tab
                {
                    var points = scriptPointsProperty.Value.Split(',');
                    var parameters = Property(attributes, ScriptParametersProperty)?.Value.Split(',');
                    if (parameters == null)
                        continue;

                    for (int i = 0; i < points.Length; i++)
                        if (!string.IsNullOrWhiteSpace(points[i]))
                            shapes.Add(new Shape { Graphic = graphic, DataObjectId = dataObjectId, Point = points[i], Parameter = i < parameters.Length ? parameters[i] : null, DataType = Shape.ObjectType.ScriptTab, CsvIndex = i });
                }
            }
            return shapes;
        }

        private static void ApplyBindings(string bindingsPath, List<Shape> shapes)
        {
            foreach (var dataObject in XDocument.Load(bindingsPath).Descendants("dataobject"))
            {
                var bindingId = dataObject.Parent?.Attribute("ID")?.Value;
                if (bindingId == null)
                    continue;

                var dataObjectId = dataObject.Attribute("objectid")?.Value;
                foreach (var shape in shapes.Where(x => x.DataObjectId == dataObjectId))
                    shape.BindingId = bindingId;
            }
        }

        private static void AddStylesheet(HtmlDocument html, string graphic, List<Shape> shapes)
        {
            var node = html.DocumentNode.SelectSingleNode(@"//*[@id=""HDXStylesheet""]");
            if (node != null)
                shapes.Add(new Shape { Graphic = graphic, ShapeName = node.Id, Css = node.Attributes["href"]?.Value });
        }

        private static void AddTooltips(HtmlDocument html, string graphic, List<Shape> shapes)
        {
            foreach (var node in html.DocumentNode.SelectNodes("//*[@title]") ?? Enumerable.Empty<HtmlNode>())
                shapes.Add(new Shape { Graphic = graphic, ShapeName = node.Id, Tooltip = node.Attributes["title"]?.Value });
        }

        /// <summary>Finds the node a data object is bound to and fills in the shape's name, path and source.</summary>
        private static void LocateOnDisplay(Shape shape, HtmlDocument html, List<CustomShape> customShapes)
        {
            if (shape.BindingId == null)
            {
                shape.ShapePath = GhostShapeName;
                shape.ShapeName = GhostShapeName;
                return;
            }

            var node = html.DocumentNode.SelectNodes($"//*[contains(@hdxproperties,'HDXBINDINGID:{shape.BindingId};')]")?.FirstOrDefault();
            if (node == null)
                return;

            shape.ShapeName = node.Id;
            var src = node.Attributes["src"];

            // Walk up to the page. The outermost node with a src is the shape on the display.
            HtmlNode? current = node;
            while (current != null && current.Id != "Page")
            {
                shape.ShapePath = shape.ShapePath == null ? current.Id : $"{current.Id} {PathSeparator} {shape.ShapePath}";
                current = current.ParentNode;

                if (current?.Attributes["src"] is HtmlAttribute outerSrc)
                {
                    src = outerSrc;
                    shape.ShapeName = current.Id;
                }
            }

            if (src == null)
                return;

            shape.ShapeSource = Path.GetFileName(src.Value);
            if (shape.ShapePath?.Contains(PathSeparator) == true)
                ApplyCustomParameter(shape, src.Value ?? string.Empty, customShapes);
        }

        /// <summary>The reference is inside a custom shape, so look up which custom properties it uses.</summary>
        private static void ApplyCustomParameter(Shape shape, string src, List<CustomShape> customShapes)
        {
            shape.CustomParamType = shape.DataType;
            shape.DataType = Shape.ObjectType.CustomParameter;

            var shapeId = shape.ShapePath!.Split(PathSeparator)[^1].Trim();
            var matches = customShapes
                .Where(x => x.ShapeName.Equals(shape.ShapeSource, StringComparison.OrdinalIgnoreCase))
                .Where(x => $"{shape.ShapeName}_{x.SubShapeName}" == shapeId)
                .Where(x => x.CsvIndex == shape.CsvIndex)
                .Where(x => x.DataType == shape.CustomParamType)
                .ToList();

            if (matches.Count > 1)
                throw new DisplayEditException($"Can't determine shape for {shape.ShapePath}.");
            if (matches.Count == 0)
                throw new DisplayEditException($"Missing shape '{src}'");

            shape.CustomParamPointName = matches[0].PointName;
            shape.CustomParamParameterName = matches[0].ParameterName;
        }

        /// <summary>Custom properties a shape substitutes directly into its markup rather than through a data object.</summary>
        private static void AddEmbeddedCustomParameters(HtmlDocument html, string graphic, List<CustomShape> customShapes, List<Shape> shapes)
        {
            var embedded = customShapes.Where(x => x.DataType == Shape.ObjectType.CustomParameterEmbedded && (x.PointName != null || x.ParameterName != null));
            foreach (var customShape in embedded)
            {
                var instances = html.DocumentNode.SelectNodes($@"//*[@src="".\{graphic}_files\{customShape.ShapeName}""]");
                foreach (var instance in instances ?? Enumerable.Empty<HtmlNode>())
                {
                    var subNode = html.DocumentNode.SelectNodes($@"//*[@id=""{instance.Id}_{customShape.SubShapeName}""]")?.FirstOrDefault();
                    if (subNode == null)
                        continue;

                    var shape = new Shape
                    {
                        Graphic = graphic,
                        CustomParamPointName = customShape.PointName,
                        CustomParamParameterName = customShape.ParameterName,
                        DataType = Shape.ObjectType.CustomParameter,
                        CustomParamType = Shape.ObjectType.CustomParameterEmbedded,
                        ShapeName = instance.Id,
                        ShapeSource = customShape.ShapeName,
                    };

                    if (customShape.PointName != null)
                        shape.Point = subNode.InnerText.Trim();
                    else
                        shape.Parameter = subNode.InnerText.Trim();

                    for (HtmlNode? current = subNode; current != null && current.Id != "Page"; current = current.ParentNode)
                        shape.ShapePath = shape.ShapePath == null ? current.Id : $"{current.Id} {PathSeparator} {shape.ShapePath}";

                    shapes.Add(shape);
                }
            }
        }

        /// <summary>
        /// One row per value in each shape's "parameters" attribute, so any of them can be edited. This includes
        /// the ones already listed above as points or parameters.
        /// </summary>
        private static void AddCustomProperties(HtmlDocument html, string graphic, List<Shape> shapes)
        {
            foreach (var node in html.DocumentNode.SelectNodes("//*[@src]") ?? Enumerable.Empty<HtmlNode>())
            {
                var parameters = node.Attributes["parameters"]?.Value;
                if (string.IsNullOrWhiteSpace(parameters))
                    continue;

                foreach (var property in CustomParameter.GetListOfCustomParameters(parameters))
                {
                    shapes.Add(new Shape
                    {
                        Graphic = graphic,
                        ShapeName = node.Id,
                        DataType = Shape.ObjectType.CustomParameter,
                        ShapeSource = Path.GetFileName(node.Attributes["src"]?.Value),
                        CustomParamType = Shape.ObjectType.CustomParameterMisc,
                        MiscPropValue = property.Value,
                        CustomPropMiscName = property.Name,
                    });
                }
            }
        }

        private static void AssignUniqueShapeIds(List<Shape> shapes)
        {
            var ids = new Dictionary<string, int>();
            var sorted = shapes.Select(x => new { x.Graphic, x.ShapeName }).Distinct().OrderBy(x => x.ShapeName).ThenBy(x => x.Graphic);
            foreach (var shape in sorted)
                ids[$"{shape.Graphic}.{shape.ShapeName}"] = ids.Count;

            foreach (var shape in shapes)
            {
                shape.GfxAndShape = $"{shape.Graphic}.{shape.ShapeName}";
                shape.UniqueShapeId = ids[shape.GfxAndShape];
            }
        }
    }
}
