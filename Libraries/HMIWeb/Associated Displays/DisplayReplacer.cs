using System.Xml.Linq;
using HtmlAgilityPack;

namespace Honeywell.HMIWeb
{
    /// <summary>A problem the user can fix, with a message written for them.</summary>
    public class DisplayEditException : Exception
    {
        public DisplayEditException(string message) : base(message) { }
    }

    /// <summary>Writes the edits made in the "New ..." columns back into the display files.</summary>
    public static class DisplayReplacer
    {
        /// <summary>Typed as a new value to blank out a reference.</summary>
        private const string DeleteKeyword = "$null";
        /// <summary>Typed as a new value to set the literal text "$null".</summary>
        private const string EscapedDeleteKeyword = @"\$null";

        /// <param name="deleteGhostReferences">Also remove data objects no shape on the display is bound to.</param>
        public static void ReplaceReferences(string folder, IEnumerable<Shape> shapes, bool deleteGhostReferences)
        {
            var displays = shapes
                .Where(x => x.NewPoint != null || x.NewParameter != null || x.NewMiscPropValue != null || x.NewTooltip != null || x.NewCss != null
                    || (deleteGhostReferences && x.ShapeName == DisplayScanner.GhostShapeName))
                .GroupBy(x => x.Graphic)
                .ToList();

            if (displays.Count == 0)
                throw new DisplayEditException("Nothing to replace.");

            foreach (var display in displays)
                ReplaceInDisplay(folder, display.Key, display.ToList());
        }

        /// <param name="replacementDisplay">File name of the display holding the shapes to swap in.</param>
        /// <param name="resize">Resize each new shape to the shape it replaces.</param>
        public static void SwapShapes(string folder, string replacementDisplay, IEnumerable<Shape> shapes, bool resize)
        {
            var replacements = AssociatedDisplays.GetShapeReplacements(folder, replacementDisplay).ToList();
            if (replacements.Count == 0)
                throw new DisplayEditException("No shape replacement gfx specified.");

            var requested = shapes
                .Where(x => x.ReplaceShapeName != null && !x.ReplaceShapeName.Equals(x.ShapeSource, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (requested.Count == 0)
                throw new DisplayEditException("Nothing to replace.");

            var missing = requested
                .Select(x => x.ReplaceShapeName!.ToUpper())
                .Distinct()
                .Where(name => !replacements.Any(x => x.NewShapeName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (missing.Count > 0)
                throw new DisplayEditException($"Add the following missing shapes to '{replacementDisplay}'\n{string.Join("\n", missing.Select(name => $"'{name}'"))}");

            var replacementHtm = Directory.GetFiles(folder, replacementDisplay).First();
            var replacementFiles = DisplayScanner.FilesFolder(replacementHtm);

            Parallel.ForEach(requested.GroupBy(x => x.Graphic), display =>
            {
                var htmPath = HtmPath(folder, display.Key);
                var filesFolder = DisplayScanner.FilesFolder(htmPath);
                var html = new HtmlDocument();
                html.Load(htmPath);

                foreach (var shape in display.Select(x => new { x.ShapeName, x.ReplaceShapeName }).Distinct())
                {
                    File.Copy(Path.Join(replacementFiles, shape.ReplaceShapeName), Path.Join(filesFolder, shape.ReplaceShapeName), true);

                    var replacement = replacements.First(x => x.NewShapeName.Equals(shape.ReplaceShapeName, StringComparison.OrdinalIgnoreCase));
                    var node = FindById(html, shape.ShapeName)
                        ?? throw new DisplayEditException($"Can't find shape '{shape.ShapeName}' in graphic '{display.Key}' to replace it with '{shape.ReplaceShapeName}'");
                    var parent = node.ParentNode
                        ?? throw new DisplayEditException($"Shape '{shape.ShapeName}' in graphic '{display.Key}' has no parent node");

                    var newNode = replacement.ReplaceShape(node, filesFolder, resize);
                    node.Remove();
                    parent.AppendChild(newNode);
                }

                html.Save(htmPath);
            });
        }

        private static string HtmPath(string folder, string? graphic)
        {
            var htmPath = Path.Join(folder, $"{graphic}.htm");
            if (!File.Exists(htmPath))
                throw new DisplayEditException($"{htmPath} does not exist");
            return htmPath;
        }

        private static HtmlNode? FindById(HtmlDocument html, string? id) =>
            html.DocumentNode.SelectNodes($@"//*[@id=""{id}""]")?.FirstOrDefault();

        private static void ReplaceInDisplay(string folder, string? graphic, List<Shape> shapes)
        {
            var htmPath = HtmPath(folder, graphic);
            var dsdPath = DisplayScanner.DataSourcePath(htmPath);
            if (!File.Exists(dsdPath))
                throw new DisplayEditException($"{dsdPath} does not exist");

            var html = new HtmlDocument();
            html.Load(htmPath);
            var dataSource = XDocument.Load(dsdPath);
            var scripts = new Dictionary<string, Script>(); // by the script node's xpath

            foreach (var shape in shapes)
            {
                shape.NewPoint = ResolveKeyword(shape.NewPoint);
                shape.NewParameter = ResolveKeyword(shape.NewParameter);
                shape.NewMiscPropValue = ResolveKeyword(shape.NewMiscPropValue);
                shape.NewCss = ResolveKeyword(shape.NewCss);
                shape.NewTooltip = ResolveKeyword(shape.NewTooltip);

                ReplaceTooltipAndStylesheet(shape, html, graphic);

                var dataObject = dataSource.Root?.Descendants("dataobject")
                    .FirstOrDefault(x => shape.DataObjectId != null && shape.DataObjectId.Equals(x.Attribute("id")?.Value, StringComparison.OrdinalIgnoreCase));

                if (shape.ShapeName == DisplayScanner.GhostShapeName)
                {
                    dataObject?.Remove();
                    continue;
                }

                var attributes = dataObject?.Descendants("property").Attributes().ToList();
                if (shape.DataType == Shape.ObjectType.DataTab || shape.CustomParamType == Shape.ObjectType.DataTab)
                {
                    if (shape.NewPoint != null)
                        Property(attributes, "PointRefPointName")?.SetValue(shape.NewPoint);
                    if (shape.NewParameter != null)
                        Property(attributes, "PointRefParamName")?.SetValue(shape.NewParameter);
                }
                else if (shape.DataType == Shape.ObjectType.ScriptTab || shape.CustomParamType == Shape.ObjectType.ScriptTab)
                {
                    ReplaceScriptDataNames(shape, attributes);
                    QueueScriptReplacements(shape, html, scripts);
                }

                if (shape.DataType == Shape.ObjectType.CustomParameter)
                {
                    if (shape.CustomParamType == Shape.ObjectType.CustomParameterEmbedded)
                        ReplaceEmbeddedParameter(shape, html, graphic);
                    else
                        ReplaceCustomParameters(shape, html, graphic);
                }
            }

            foreach (var (xPath, script) in scripts)
            {
                var node = html.DocumentNode.Descendants().FirstOrDefault(x => x.XPath == xPath);
                if (node != null)
                    node.InnerHtml = script.UpdateScript();
            }

            dataSource.Save(dsdPath);
            html.Save(htmPath);
        }

        private static string? ResolveKeyword(string? value)
        {
            if (DeleteKeyword.Equals(value, StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            if (EscapedDeleteKeyword.Equals(value, StringComparison.OrdinalIgnoreCase))
                return DeleteKeyword;
            return value;
        }

        private static XElement? Property(List<XAttribute>? attributes, string name) =>
            attributes?.FirstOrDefault(attribute => attribute.Value == name)?.Parent;

        private static void ReplaceTooltipAndStylesheet(Shape shape, HtmlDocument html, string? graphic)
        {
            if (shape.NewTooltip != null)
            {
                var node = html.DocumentNode.SelectSingleNode($@"//*[@id=""{shape.ShapeName}""]")
                    ?? throw new DisplayEditException($"Can't find shape '{shape.ShapeName}' in graphic '{graphic}' to set its tooltip");
                node.SetAttributeValue("title", shape.NewTooltip);
            }

            if (shape.NewCss != null)
            {
                var node = html.DocumentNode.SelectSingleNode(@"//*[@id=""HDXStylesheet""]")
                    ?? throw new DisplayEditException($"Can't find the HDXStylesheet node in graphic '{graphic}' to set its CSS");
                node.SetAttributeValue("href", shape.NewCss);
            }
        }

        private static void ReplaceScriptDataNames(Shape shape, List<XAttribute>? attributes)
        {
            var pointsProperty = Property(attributes, "CommaDelimitedPointNames");
            var parametersProperty = Property(attributes, "CommaDelimitedParameters");
            if (pointsProperty == null || parametersProperty == null)
                return;

            if (shape.NewPoint != null)
                pointsProperty.SetValue(WithItem(pointsProperty.Value, shape.CsvIndex, shape.NewPoint));
            if (shape.NewParameter != null)
                parametersProperty.SetValue(WithItem(parametersProperty.Value, shape.CsvIndex, shape.NewParameter));
        }

        private static string WithItem(string commaDelimited, int index, string value)
        {
            var items = commaDelimited.Split(',');
            items[index] = value;
            return string.Join(",", items);
        }

        /// <summary>Records the rename in every script that quotes this point.parameter. Applied once per script at the end.</summary>
        private static void QueueScriptReplacements(Shape shape, HtmlDocument html, Dictionary<string, Script> scripts)
        {
            var oldTag = $@"""{shape.Point}.{shape.Parameter}""".ToUpper();
            var newTag = $@"""{shape.NewPoint ?? shape.Point}.{shape.NewParameter ?? shape.Parameter}""".ToUpper();

            var nodes = html.DocumentNode.Descendants()
                .Where(x => x.Name.Equals("SCRIPT", StringComparison.OrdinalIgnoreCase) && x.InnerText.ToUpper().Contains(oldTag));
            foreach (var node in nodes)
            {
                if (!scripts.TryGetValue(node.XPath, out var script))
                    scripts[node.XPath] = script = new Script(node.InnerHtml);
                script.UpdateReplacements(oldTag, newTag);
            }
        }

        private static void ReplaceEmbeddedParameter(Shape shape, HtmlDocument html, string? graphic)
        {
            var shapePath = shape.ShapePath
                ?? throw new DisplayEditException($"Shape '{shape.ShapeName}' in graphic '{graphic}' has no shape path for its embedded custom parameter");
            var shapeId = shapePath.Split(DisplayScanner.PathSeparator)[^1].Trim();
            var node = FindById(html, shapeId)
                ?? throw new DisplayEditException($"Can't find node '{shapeId}' in graphic '{graphic}' for an embedded custom parameter of shape '{shape.ShapeName}'");

            if (shape.Point != null && shape.NewPoint != null)
                node.InnerHtml = node.InnerHtml.ReplaceWholeName(shape.Point, shape.NewPoint);
            if (shape.Parameter != null && shape.NewParameter != null)
                node.InnerHtml = node.InnerHtml.ReplaceWholeName(shape.Parameter, shape.NewParameter);
        }

        private static void ReplaceCustomParameters(Shape shape, HtmlDocument html, string? graphic)
        {
            var node = FindById(html, shape.ShapeName);
            var parameters = node?.Attributes["parameters"]?.Value;
            if (node == null || parameters == null)
                throw new DisplayEditException($"Can't find Custom Parameters in Graphic {graphic} for shape {shape.ShapeName}");

            static string Swap(string parameters, string? name, string? oldValue, string? newValue) => newValue == null
                ? parameters
                : parameters.Replace($"?{name}:{oldValue};", $"?{name}:{newValue};", StringComparison.OrdinalIgnoreCase);

            parameters = Swap(parameters, shape.CustomParamPointName, shape.Point, shape.NewPoint);
            parameters = Swap(parameters, shape.CustomParamParameterName, shape.Parameter, shape.NewParameter);
            parameters = Swap(parameters, shape.CustomPropMiscName, shape.MiscPropValue, shape.NewMiscPropValue);
            node.SetAttributeValue("parameters", parameters);
        }
    }
}
