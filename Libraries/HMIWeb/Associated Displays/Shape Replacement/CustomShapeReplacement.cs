using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Honeywell.HMIWeb
{
    public class CustomShapeReplacement
    {
        public string NewShapeName { get; set; }
        public List<CustomParameter> NewCustomParameters { get; set; }
        public HtmlNode NewNode { get; set; }
        private readonly string _left;
        private readonly string _top;
        private readonly string _width;
        private readonly string _height;
        public List<ShapeBinding> NewBindings { get; set; }
        private static readonly string xPathBinding = $@"//*[contains(@hdxproperties,'HDXBINDINGID:')]";
        private static readonly Regex regexLeft = new(@"LEFT:\s\d+px;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexTop = new(@"TOP:\s\d+px;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexWidth = new(@"WIDTH:\s\d+px;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexHeight = new(@"HEIGHT:\s\d+px;", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public CustomShapeReplacement(HtmlNode node, string gfxFilesPath)
        {
            var dsdPath = $@"{gfxFilesPath}\DS_datasource1.dsd";
            var bindingsPath = $@"{gfxFilesPath}\Bindings.xml";

            var xdocBinding = XDocument.Load(bindingsPath);
            var xdocDsd = XDocument.Load(dsdPath);

            var style = node.Attributes["style"]?.Value
                ?? throw new InvalidOperationException($"Shape '{node.Id}' in '{gfxFilesPath}' has no 'style' attribute");
            _left = regexLeft.Match(style).Value;
            _top = regexTop.Match(style).Value;
            _width = regexWidth.Match(style).Value;
            _height = regexHeight.Match(style).Value;

            var src = node.Attributes["src"]?.Value
                ?? throw new InvalidOperationException($"Shape '{node.Id}' in '{gfxFilesPath}' has no 'src' attribute");
            var srcShape = Path.GetFileName(src);
            var shapePath = $@"{gfxFilesPath}\{srcShape}";
            var customShapeName = Path.GetFileName(shapePath);
            var xdocShape = XDocument.Load(shapePath);

            var shapeParameters = xdocShape.Descendants("parameter");
            var customParameters = CustomParameter.GetListOfCustomParameters(shapeParameters);

            var bindingList = new List<ShapeBinding>();
            //var htmBindingNodes = node.SelectNodes(xPathBinding);
            var htmBindingNodes = node.DescendantsAndSelf();
            foreach (var htmBindingNode in htmBindingNodes)
            {
                var t = htmBindingNode.Attributes.FirstOrDefault(x => x.Value?.Contains("HDXBINDINGID:") == true);
                if (t == null)
                    continue;
                var nodeId = node.Id ?? throw new InvalidOperationException($"Shape '{customShapeName}' in '{gfxFilesPath}' has a node with no id");
                var bindingNodeId = htmBindingNode.Id ?? throw new InvalidOperationException($"Shape '{nodeId}' in '{gfxFilesPath}' has a bound child node with no id");
                var shapeSubObject = bindingNodeId[(nodeId.Length + 1)..];
                var shapeElement = xdocShape.Descendants("content").Where(x => x.Value.Contains($"id={shapeSubObject} ")).FirstOrDefault()
                    ?.AncestorsAndSelf("element").FirstOrDefault()
                    ?? throw new InvalidOperationException($"Can't find the shape element for '{shapeSubObject}' in the shape file");
                var shapeDataObjects = shapeElement.Descendants("dataobject");

                var bindingId = ShapeBinding.GetIdFromHtml(htmBindingNode)
                    ?? throw new InvalidOperationException($"Node '{bindingNodeId}' in '{gfxFilesPath}' has no hdxproperties binding id");
                var binding = xdocBinding.Descendants("binding").FirstOrDefault(x => x.Attribute("ID")?.Value == bindingId)
                    ?? throw new InvalidOperationException($"Can't find binding '{bindingId}' for node '{bindingNodeId}' in '{bindingsPath}'");

                var bindingDataObjects = binding.Descendants("dataobject");
                var dataObjectList = new List<ShapeDataObject>();
                foreach (var bindingDataObject in bindingDataObjects)
                {
                    var dataobjectId = bindingDataObject.Attribute("objectid")?.Value
                        ?? throw new InvalidOperationException($"Binding '{bindingId}' in '{bindingsPath}' has a dataobject with no 'objectid' attribute");
                    var dsdDataObject = xdocDsd.Descendants("dataobject").Where(x => x.Attribute("id")?.Value == dataobjectId).FirstOrDefault()
                        ?? throw new InvalidOperationException($"Can't find dataobject '{dataobjectId}' for binding '{bindingId}' in '{dsdPath}'");

                    var propertyAttributes = dsdDataObject.Descendants("property").Attributes();
                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                    {
                        string pointSearch = "PointRefPointName";
                        string paramSearch = "PointRefParamName";

                        var pointElement = GetPropertyElement(propertyAttributes, pointSearch, dataobjectId, customShapeName);
                        var parameterElement = GetPropertyElement(propertyAttributes, paramSearch, dataobjectId, customShapeName);

                        pointElement.Value = GetShapePropertyValue(shapeDataObjects, pointSearch, shapeSubObject, customShapeName);
                        parameterElement.Value = GetShapePropertyValue(shapeDataObjects, paramSearch, shapeSubObject, customShapeName);
                    }
                    else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                    {
                        string pointSearch = "CommaDelimitedPointNames";
                        string paramSearch = "CommaDelimitedParameters";

                        var pointElement = GetPropertyElement(propertyAttributes, pointSearch, dataobjectId, customShapeName);
                        var parameterElement = GetPropertyElement(propertyAttributes, paramSearch, dataobjectId, customShapeName);

                        pointElement.Value = GetShapePropertyValue(shapeDataObjects, pointSearch, shapeSubObject, customShapeName);
                        parameterElement.Value = GetShapePropertyValue(shapeDataObjects, paramSearch, shapeSubObject, customShapeName);
                    }
                    dataObjectList.Add(new ShapeDataObject(dsdDataObject));
                }
                bindingList.Add(new ShapeBinding(binding, dataObjectList));
            }
            
            NewShapeName = customShapeName;
            NewCustomParameters = customParameters;
            NewNode = node;
            NewBindings = bindingList;
        }

        public HtmlNode ReplaceShape(HtmlNode oldNode, string newGfxFilesPath, bool resizeShape)
        {
            #region Update Htm
            var clonedNode = NewNode.CloneNode(true);
            clonedNode.Id = oldNode.Id;

            var oldStyle = oldNode.Attributes["style"]?.Value
                ?? throw new InvalidOperationException($"Shape '{oldNode.Id}' in '{newGfxFilesPath}' has no 'style' attribute");
            var oldLeft = regexLeft.Match(oldStyle).Value;
            var oldTop = regexTop.Match(oldStyle).Value;
            var oldWidth = regexWidth.Match(oldStyle).Value;
            var oldHeight = regexHeight.Match(oldStyle).Value;

            var clonedStyle = clonedNode.Attributes["style"];
            var clonedStyleValue = clonedStyle?.Value
                ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no 'style' attribute");
            if (resizeShape)
                clonedStyle.Value = clonedStyleValue.Replace(_left, oldLeft).Replace(_top, oldTop).Replace(_width, oldWidth).Replace(_height, oldHeight);
            else
                clonedStyle.Value = clonedStyleValue.Replace(_left, oldLeft).Replace(_top, oldTop);

            var oldSrc = oldNode.Attributes["src"]?.Value
                ?? throw new InvalidOperationException($"Shape '{oldNode.Id}' in '{newGfxFilesPath}' has no 'src' attribute");
            var oldSrcShape = Path.GetFileName(oldSrc);
            var clonedSrc = clonedNode.Attributes.FirstOrDefault(x => x.Name == "src")
                ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no 'src' attribute");
            clonedSrc.Value = oldSrc.Replace(oldSrcShape, NewShapeName, StringComparison.OrdinalIgnoreCase);

            var htmParameters = oldNode.Attributes.Where(x => x.Name == "parameters").FirstOrDefault()?.Value
                ?? throw new InvalidOperationException($"Shape '{oldNode.Id}' in '{newGfxFilesPath}' has no 'parameters' attribute");
            List<CustomParameter> customParameters = CustomParameter.GetListOfCustomParameters(htmParameters);

            string parameters = string.Empty;
            foreach (var newCustomParameter in NewCustomParameters)
            {
                var match = customParameters.FirstOrDefault(x => x.Type == newCustomParameter.Type && x.Name == newCustomParameter.Name);
                if (match != null)
                    parameters += $"{newCustomParameter.Type}?{newCustomParameter.Name}:{match.Value};";
                else
                    parameters += $"{newCustomParameter.Type}?{newCustomParameter.Name}:{newCustomParameter.Value};";
            }
            var clonedParameters = clonedNode.Attributes.FirstOrDefault(x => x.Name == "parameters")
                ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no 'parameters' attribute");
            clonedParameters.Value = parameters; //parameters

            string newPrefix = clonedNode.Id
                ?? throw new InvalidOperationException($"A shape in '{newGfxFilesPath}' being replaced with '{NewShapeName}' has no id");
            var oldPrefix = NewNode.Id
                ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no id");
            var childNodes = clonedNode.Descendants().Where(x => !string.IsNullOrWhiteSpace(x.Id));
            foreach (var child in childNodes)
            {
                var newId = newPrefix + child.Id![oldPrefix.Length..];
                child.Id = newId;
            }
                

            #endregion

            #region Update Bindings
            customParameters = CustomParameter.GetListOfCustomParameters(parameters);
            var newBindingList = new List<ShapeBinding>();


            var bindingsPath = $@"{newGfxFilesPath}\Bindings.xml";
            var xdocBinding = XDocument.Load(bindingsPath);
            var newBindingId = xdocBinding.Descendants("binding").Select(x => x.Attribute("ID")?.Value).OfType<string>().Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastBinding = xdocBinding.Descendants("binding").LastOrDefault()
                ?? throw new InvalidOperationException($"'{bindingsPath}' has no bindings");

            var dsdPath = $@"{newGfxFilesPath}\DS_datasource1.dsd";
            var xdocDsd = XDocument.Load(dsdPath);
            var newDataObjectId = xdocDsd.Descendants("dataobject").Select(x => x.Attribute("id")?.Value).OfType<string>().Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastDataObject = xdocDsd.Descendants("dataobject").LastOrDefault()
                ?? throw new InvalidOperationException($"'{dsdPath}' has no dataobjects");

            //add new bindings and data objects to Bindings.xml and DS_datasource1.dsd
            foreach (var binding in NewBindings)
            {
                var newbinding = binding.CloneBinding();
                newbinding.Id = newBindingId++;

                foreach (var newDataObject in newbinding.DataObjects)
                {
                    var sourceDataObjectId = newDataObject.Id.ToString();
                    var bindingDataobject = newbinding.BindingElement.Descendants("dataobject").FirstOrDefault(x => x.Attribute("objectid")?.Value == sourceDataObjectId)
                        ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no binding dataobject for data object '{sourceDataObjectId}'");
                    newDataObject.Id = newDataObjectId++;
                    bindingDataobject.SetAttributeValue("objectid", newDataObject.Id.ToString());

                    var propertyAttributes = newDataObject.DataObject.Descendants("property").Attributes();

                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                    {
                        string pointSearch = "PointRefPointName";
                        string paramSearch = "PointRefParamName";

                        var pointElement = GetPropertyElement(propertyAttributes, pointSearch, sourceDataObjectId, NewShapeName);
                        var parameterElement = GetPropertyElement(propertyAttributes, paramSearch, sourceDataObjectId, NewShapeName);
                        var pointValue = customParameters.FirstOrDefault(x => pointElement.Value.ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value;
                        var paramValue = customParameters.FirstOrDefault(x => parameterElement.Value.ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value;

                        if (pointValue != null)
                            pointElement.Value = pointValue;
                        if (paramValue != null)
                            parameterElement.Value = paramValue;
                    }
                    else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                    {
                        string pointSearch = "CommaDelimitedPointNames";
                        string paramSearch = "CommaDelimitedParameters";

                        var pointElement = GetPropertyElement(propertyAttributes, pointSearch, sourceDataObjectId, NewShapeName);
                        var parameterElement = GetPropertyElement(propertyAttributes, paramSearch, sourceDataObjectId, NewShapeName);

                        var points = pointElement.Value.Split(',');
                        var parameters2 = parameterElement.Value.Split(',');
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(points[i]) && !string.IsNullOrWhiteSpace(parameters2[i]))
                            {
                                var pointValue = customParameters.FirstOrDefault(x => points[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value;
                                var paramValue = customParameters.FirstOrDefault(x => parameters2[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value;

                                if (pointValue != null)
                                    points[i] = pointValue;
                                if (paramValue != null)
                                    parameters2[i] = paramValue;
                            }
                        }
                        pointElement.Value = points.Aggregate((x, y) => $"{x},{y}"); ;
                        parameterElement.Value = parameters2.Aggregate((x, y) => $"{x},{y}");
                    }
                    lastDataObject.AddAfterSelf(newDataObject.DataObject);
                    lastDataObject = newDataObject.DataObject;
                }

                lastBinding.AddAfterSelf(newbinding.BindingElement);
                lastBinding = newbinding.BindingElement;
                newBindingList.Add(newbinding);
            }

            //update cloned node with new bindings
            var newNodeBindings = clonedNode.Descendants().Where(x => x.Attributes.Where(y => y.Name == "hdxproperties" && y.Value?.Contains("HDXBINDINGID:") == true).Select(y => y.OwnerNode?.Id).Contains(x.Id));
            foreach (var newNodeWithBinging in newNodeBindings)
            {
                var hdxprop = newNodeWithBinging.Attributes.FirstOrDefault(x => x.Name == "hdxproperties");
                if (hdxprop != null && int.TryParse(ShapeBinding.GetIdFromHtml(newNodeWithBinging), out int oldClonedBindingId))
                {
                    var bindingIndex = NewBindings.FindIndex(x => x.Id == oldClonedBindingId);
                    if (bindingIndex < 0)
                        throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no binding '{oldClonedBindingId}' for node '{newNodeWithBinging.Id}'");
                    newBindingId = newBindingList[bindingIndex].Id;

                    hdxprop.Value = hdxprop.Value?.Replace($"HDXBINDINGID:{oldClonedBindingId};", $"HDXBINDINGID:{newBindingId};", StringComparison.OrdinalIgnoreCase);
                }
            }

            //remove old bindings and data objects to Bindings.xml and DS_datasource1.dsd
            var oldNodeBindings = oldNode.Descendants().Where(x => x.Attributes.Where(y => y.Name == "hdxproperties" && y.Value?.Contains("HDXBINDINGID:") == true).Select(y => y.OwnerNode?.Id).Contains(x.Id));
            foreach (var oldNodeWithBinging in oldNodeBindings)
            {
                var hdxprop = oldNodeWithBinging.Attributes.FirstOrDefault(x => x.Name == "hdxproperties");
                if (hdxprop != null && int.TryParse(ShapeBinding.GetIdFromHtml(oldNodeWithBinging), out int oldNodeBindingId))
                {
                    var oldBinding = xdocBinding.Descendants("binding").FirstOrDefault(x => x.Attribute("ID")?.Value == oldNodeBindingId.ToString())
                        ?? throw new InvalidOperationException($"Can't find binding '{oldNodeBindingId}' for node '{oldNodeWithBinging.Id}' in '{bindingsPath}'");
                    var oldBindingDataObjects = oldBinding.Descendants("dataobject");
                    foreach (var oldBindingDataObject in oldBindingDataObjects)
                    {
                        var oldDataObjectId = oldBindingDataObject.Attribute("objectid")?.Value
                            ?? throw new InvalidOperationException($"Binding '{oldNodeBindingId}' in '{bindingsPath}' has a dataobject with no 'objectid' attribute");
                        var oldDataObject = xdocDsd.Descendants("dataobject").FirstOrDefault(x => x.Attribute("id")?.Value == oldDataObjectId)
                            ?? throw new InvalidOperationException($"Can't find dataobject '{oldDataObjectId}' for binding '{oldNodeBindingId}' in '{dsdPath}'");
                        oldDataObject.Remove();
                    }
                    oldBinding.Remove();
                }
            }


            #endregion

            #region Update embedded parameters
            var shapePath = $@"{newGfxFilesPath}\{NewShapeName}";
            var xdocShape = XDocument.Load(shapePath);
            var shapeContent = xdocShape.Descendants("content");
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
                            var htmId = $"{newPrefix}_{shapeId}";
                            var embeddedNode = clonedNode.Descendants().Where(x => x.Id == htmId).FirstOrDefault()
                                ?? throw new InvalidOperationException($"Shape '{newPrefix}' has no node '{htmId}' for an embedded parameter of '{NewShapeName}'");
                            var type = embeddedParam.Groups["Type"].Value.ToUpper();
                            var value = embeddedParam.Groups["CustomProperty"].Value.ToUpper();
                            var newInnerText = customParameters.FirstOrDefault(x => x.Name.ToUpper() == value && x.Type.ToUpper() == type)?.Value
                                ?? throw new InvalidOperationException($"Shape '{newPrefix}' has no custom parameter '{type}::{value}' for embedded node '{htmId}'");
                            //var oldInnerText = NewCustomParameters.FirstOrDefault(x => x.Name.ToUpper() == value && x.Type.ToUpper() == type).Value;
                            embeddedNode.InnerHtml = newInnerText;
                        }
                    }
                }
            }
            #endregion

            //save last, so a missing embedded parameter above can't leave these files half-edited
            xdocBinding.Save(bindingsPath);
            xdocDsd.Save(dsdPath);
            return clonedNode;
        }

        /// <summary>
        /// Copy bindings and dataobjects to file with updated ID's. Returns list of updating ID's
        /// </summary>
        /// <param name="newGfxFilesPath"></param>
        /// <returns></returns>
        private List<ShapeBinding> CopyBindingsToFile(string newGfxFilesPath, List<CustomParameter> customParameters)
        {
            var newBindings = new List<ShapeBinding>();

           
            var bindingsPath = $@"{newGfxFilesPath}\Bindings.xml";
            var xdocBinding = XDocument.Load(bindingsPath);
            var newBindingId = xdocBinding.Descendants("binding").Select(x => x.Attribute("ID")?.Value).OfType<string>().Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastBinding = xdocBinding.Descendants("binding").LastOrDefault()
                ?? throw new InvalidOperationException($"'{bindingsPath}' has no bindings");

            var dsdPath = $@"{newGfxFilesPath}\DS_datasource1.dsd";
            var xdocDsd = XDocument.Load(dsdPath);
            var newDataObjectId = xdocDsd.Descendants("dataobject").Select(x => x.Attribute("id")?.Value).OfType<string>().Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastDataObject = xdocDsd.Descendants("dataobject").LastOrDefault()
                ?? throw new InvalidOperationException($"'{dsdPath}' has no dataobjects");

            foreach (var binding in NewBindings)
            {
                
                var newbinding = binding.CloneBinding();
                newbinding.Id = newBindingId++;

                foreach (var newDataObject in newbinding.DataObjects)
                {
                    var sourceDataObjectId = newDataObject.Id.ToString();
                    var bindingDataobject = newbinding.BindingElement.Descendants("dataobject").FirstOrDefault(x => x.Attribute("objectid")?.Value == sourceDataObjectId)
                        ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no binding dataobject for data object '{sourceDataObjectId}'");
                    newDataObject.Id = newDataObjectId++;
                    bindingDataobject.SetAttributeValue("objectid", newDataObject.Id.ToString());

                    var propertyAttributes = newDataObject.DataObject.Descendants("property").Attributes();

                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                    {
                        string pointSearch = "PointRefPointName";
                        string paramSearch = "PointRefParamName";

                        var pointElement = GetPropertyElement(propertyAttributes, pointSearch, sourceDataObjectId, NewShapeName);
                        var parameterElement = GetPropertyElement(propertyAttributes, paramSearch, sourceDataObjectId, NewShapeName);

                        pointElement.Value = customParameters.FirstOrDefault(x => pointElement.Value.ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value
                            ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no custom parameter for '{pointElement.Value}'");
                        parameterElement.Value = customParameters.FirstOrDefault(x => parameterElement.Value.ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value
                            ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no custom parameter for '{parameterElement.Value}'");
                    }
                    else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                    {
                        string pointSearch = "CommaDelimitedPointNames";
                        string paramSearch = "CommaDelimitedParameters";

                        var pointElement = GetPropertyElement(propertyAttributes, pointSearch, sourceDataObjectId, NewShapeName);
                        var parameterElement = GetPropertyElement(propertyAttributes, paramSearch, sourceDataObjectId, NewShapeName);

                        var points = pointElement.Value.Split(',');
                        var parameters = parameterElement.Value.Split(',');
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(points[i]) && !string.IsNullOrWhiteSpace(parameters[i]))
                            {
                                points[i] = customParameters.FirstOrDefault(x => points[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value
                                    ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no custom parameter for '{points[i]}'");
                                parameters[i] = customParameters.FirstOrDefault(x => parameters[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}")?.Value
                                    ?? throw new InvalidOperationException($"Replacement shape '{NewShapeName}' has no custom parameter for '{parameters[i]}'");
                            }
                        }
                        pointElement.Value = points.Aggregate((x, y) => $"{x},{y}"); ;
                        parameterElement.Value = parameters.Aggregate((x, y) => $"{x},{y}");
                    }
                    lastDataObject.AddAfterSelf(newDataObject.DataObject);
                    lastDataObject = newDataObject.DataObject;
                }

                lastBinding.AddAfterSelf(newbinding.BindingElement);
                lastBinding = newbinding.BindingElement;
                newBindings.Add(newbinding);
            }
            //xdocBinding.Save(bindingsPath);
            //xdocDsd.Save(dsdPath);
            return newBindings;
        }

        private static XElement GetPropertyElement(IEnumerable<XAttribute> propertyAttributes, string propertyName, string dataObjectId, string shapeName)
        {
            return propertyAttributes.FirstOrDefault(x => x.Value == propertyName)?.Parent
                ?? throw new InvalidOperationException($"Data object '{dataObjectId}' of shape '{shapeName}' has no '{propertyName}' property");
        }

        private static string GetShapePropertyValue(IEnumerable<XElement> shapeDataObjects, string propertyName, string shapeSubObject, string shapeName)
        {
            return shapeDataObjects.Descendants("property").FirstOrDefault(x => x.Attribute("name")?.Value == propertyName)?.Attribute("value")?.Value
                ?? throw new InvalidOperationException($"Element '{shapeSubObject}' of shape '{shapeName}' has no value for property '{propertyName}'");
        }
    }
}
