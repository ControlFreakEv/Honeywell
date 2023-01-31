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

            var style = node.Attributes["style"].Value;
            _left = regexLeft.Match(style).Value;
            _top = regexTop.Match(style).Value;
            _width = regexWidth.Match(style).Value;
            _height = regexHeight.Match(style).Value;

            var src = node.Attributes["src"];
            var srcShape = Path.GetFileName(src.Value);
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
                var t = htmBindingNode.Attributes.FirstOrDefault(x => x.Value.Contains("HDXBINDINGID:"));
                if (t == null)
                    continue;
                var shapeSubObject = htmBindingNode.Id[(node.Id.Length + 1)..];
                var shapeElement = xdocShape.Descendants("content").Where(x => x.Value.Contains($"id={shapeSubObject} ")).FirstOrDefault();
                while (shapeElement.Name != "element")
                {
                    shapeElement = shapeElement.Parent;
                }
                var shapeDataObjects = shapeElement.Descendants("dataobject");

                var bindingId = ShapeBinding.GetIdFromHtml(htmBindingNode);
                var binding = xdocBinding.Descendants("binding").FirstOrDefault(x => x.Attribute("ID").Value == bindingId);

                var bindingDataObjects = binding.Descendants("dataobject");
                var dataObjectList = new List<ShapeDataObject>();
                foreach (var bindingDataObject in bindingDataObjects)
                {
                    var dataobjectId = bindingDataObject.Attribute("objectid").Value;
                    var dsdDataObject = xdocDsd.Descendants("dataobject").Where(x => x.Attribute("id").Value == dataobjectId).FirstOrDefault();

                    var propertyAttributes = dsdDataObject.Descendants("property").Attributes();
                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                    {
                        string pointSearch = "PointRefPointName";
                        string paramSearch = "PointRefParamName";

                        var pointElement = propertyAttributes.FirstOrDefault(x => x.Value == pointSearch)?.Parent;
                        var parameterElement = propertyAttributes.FirstOrDefault(x => x.Value == paramSearch)?.Parent;

                        pointElement.Value = shapeDataObjects.Descendants("property").FirstOrDefault(x => x.Attribute("name").Value == pointSearch).Attribute("value").Value;
                        parameterElement.Value = shapeDataObjects.Descendants("property").FirstOrDefault(x => x.Attribute("name").Value == paramSearch).Attribute("value").Value;
                    }
                    else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                    {
                        string pointSearch = "CommaDelimitedPointNames";
                        string paramSearch = "CommaDelimitedParameters";

                        var pointElement = propertyAttributes.FirstOrDefault(x => x.Value == pointSearch)?.Parent;
                        var parameterElement = propertyAttributes.FirstOrDefault(x => x.Value == paramSearch)?.Parent;

                        pointElement.Value = shapeDataObjects.Descendants("property").FirstOrDefault(x => x.Attribute("name").Value == pointSearch).Attribute("value").Value;
                        parameterElement.Value = shapeDataObjects.Descendants("property").FirstOrDefault(x => x.Attribute("name").Value == paramSearch).Attribute("value").Value;
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

            var oldStyle = oldNode.Attributes["style"];
            var oldLeft = regexLeft.Match(oldStyle.Value).Value;
            var oldTop = regexTop.Match(oldStyle.Value).Value;
            var oldWidth = regexWidth.Match(oldStyle.Value).Value;
            var oldHeight = regexHeight.Match(oldStyle.Value).Value;

            var clonedStyle = clonedNode.Attributes["style"];
            if (resizeShape)
                clonedStyle.Value = clonedStyle.Value.Replace(_left, oldLeft).Replace(_top, oldTop).Replace(_width, oldWidth).Replace(_height, oldHeight);
            else
                clonedStyle.Value = clonedStyle.Value.Replace(_left, oldLeft).Replace(_top, oldTop);

            var oldSrc = oldNode.Attributes["src"].Value;
            var oldSrcShape = Path.GetFileName(oldSrc);
            clonedNode.Attributes.FirstOrDefault(x => x.Name == "src").Value = oldSrc.Replace(oldSrcShape, NewShapeName, StringComparison.OrdinalIgnoreCase);

            var htmParameters = oldNode.Attributes.Where(x => x.Name == "parameters").FirstOrDefault()?.Value;
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
            clonedNode.Attributes.FirstOrDefault(x => x.Name == "parameters").Value = parameters; //parameters

            string newPrefix = clonedNode.Id;
            var oldPrefix = NewNode.Id;
            var childNodes = clonedNode.Descendants().Where(x => !string.IsNullOrWhiteSpace(x.Id));
            foreach (var child in childNodes)
            {
                var newId = newPrefix + child.Id[oldPrefix.Length..];
                child.Id = newId;
            }
                

            #endregion

            #region Update Bindings
            customParameters = CustomParameter.GetListOfCustomParameters(parameters);
            var newBindingList = new List<ShapeBinding>();


            var bindingsPath = $@"{newGfxFilesPath}\Bindings.xml";
            var xdocBinding = XDocument.Load(bindingsPath);
            var newBindingId = xdocBinding.Descendants("binding").Select(x => x.Attribute("ID").Value).Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastBinding = xdocBinding.Descendants("binding").LastOrDefault();

            var dsdPath = $@"{newGfxFilesPath}\DS_datasource1.dsd";
            var xdocDsd = XDocument.Load(dsdPath);
            var newDataObjectId = xdocDsd.Descendants("dataobject").Select(x => x.Attribute("id").Value).Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastDataObject = xdocDsd.Descendants("dataobject").LastOrDefault();

            //add new bindings and data objects to Bindings.xml and DS_datasource1.dsd
            foreach (var binding in NewBindings)
            {
                var newbinding = binding.CloneBinding();
                newbinding.Id = newBindingId++;

                foreach (var newDataObject in newbinding.DataObjects)
                {
                    var bindingDataobject = newbinding.BindingElement.Descendants("dataobject").FirstOrDefault(x => x.Attribute("objectid").Value == newDataObject.Id.ToString());
                    newDataObject.Id = newDataObjectId++;
                    bindingDataobject.Attribute("objectid").Value = newDataObject.Id.ToString();

                    var propertyAttributes = newDataObject.DataObject.Descendants("property").Attributes();

                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                    {
                        string pointSearch = "PointRefPointName";
                        string paramSearch = "PointRefParamName";

                        var pointElement = propertyAttributes.FirstOrDefault(x => x.Value == pointSearch)?.Parent;
                        var parameterElement = propertyAttributes.FirstOrDefault(x => x.Value == paramSearch)?.Parent;
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

                        var pointElement = propertyAttributes.FirstOrDefault(x => x.Value == pointSearch)?.Parent;
                        var parameterElement = propertyAttributes.FirstOrDefault(x => x.Value == paramSearch)?.Parent;

                        var points = pointElement.Value.Split(',');
                        var parameters2 = parameterElement.Value.Split(',');
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(points[i]) && !string.IsNullOrWhiteSpace(parameters2[i]))
                            {
                                var pointValue = customParameters.FirstOrDefault(x => points[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}").Value;
                                var paramValue = customParameters.FirstOrDefault(x => parameters2[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}").Value;

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
            var newNodeBindings = clonedNode.Descendants().Where(x => x.Attributes.Where(y => y.Name == "hdxproperties" && y.Value.Contains("HDXBINDINGID:")).Select(y => y.OwnerNode.Id).Contains(x.Id));
            foreach (var newNodeWithBinging in newNodeBindings)
            {
                var hdxprop = newNodeWithBinging.Attributes.FirstOrDefault(x => x.Name == "hdxproperties");
                if (hdxprop != null && int.TryParse(ShapeBinding.GetIdFromHtml(newNodeWithBinging), out int oldClonedBindingId))
                {
                    var bindingIndex = NewBindings.IndexOf(NewBindings.FirstOrDefault(x => x.Id == oldClonedBindingId));
                    newBindingId = newBindingList[bindingIndex].Id;

                    hdxprop.Value = hdxprop.Value.Replace($"HDXBINDINGID:{oldClonedBindingId};", $"HDXBINDINGID:{newBindingId};", StringComparison.OrdinalIgnoreCase);
                }
            }

            //remove old bindings and data objects to Bindings.xml and DS_datasource1.dsd
            var oldNodeBindings = oldNode.Descendants().Where(x => x.Attributes.Where(y => y.Name == "hdxproperties" && y.Value.Contains("HDXBINDINGID:")).Select(y => y.OwnerNode.Id).Contains(x.Id));
            foreach (var oldNodeWithBinging in oldNodeBindings)
            {
                var hdxprop = oldNodeWithBinging.Attributes.FirstOrDefault(x => x.Name == "hdxproperties");
                if (hdxprop != null && int.TryParse(ShapeBinding.GetIdFromHtml(oldNodeWithBinging), out int oldNodeBindingId))
                {
                    var oldBinding = xdocBinding.Descendants("binding").FirstOrDefault(x => x.Attribute("ID").Value == oldNodeBindingId.ToString());
                    var oldBindingDataObjects = oldBinding.Descendants("dataobject");
                    foreach (var oldBindingDataObject in oldBindingDataObjects)
                    {
                        var oldDataObjectId = oldBindingDataObject.Attribute("objectid").Value;
                        var oldDataObject = xdocDsd.Descendants("dataobject").FirstOrDefault(x => x.Attribute("id").Value == oldDataObjectId);
                        oldDataObject.Remove();
                    }
                    oldBinding.Remove();
                }
            }


            xdocBinding.Save(bindingsPath);
            xdocDsd.Save(dsdPath);
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
                            var embeddedNode = clonedNode.Descendants().Where(x => x.Id == htmId).FirstOrDefault();
                            var type = embeddedParam.Groups["Type"].Value.ToUpper();
                            var value = embeddedParam.Groups["CustomProperty"].Value.ToUpper();
                            var newInnerText = customParameters.FirstOrDefault(x => x.Name.ToUpper() == value && x.Type.ToUpper() == type).Value;
                            //var oldInnerText = NewCustomParameters.FirstOrDefault(x => x.Name.ToUpper() == value && x.Type.ToUpper() == type).Value;
                            embeddedNode.InnerHtml = newInnerText;
                        }
                    }
                }
            }
            #endregion
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
            var newBindingId = xdocBinding.Descendants("binding").Select(x => x.Attribute("ID").Value).Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastBinding = xdocBinding.Descendants("binding").LastOrDefault();

            var dsdPath = $@"{newGfxFilesPath}\DS_datasource1.dsd";
            var xdocDsd = XDocument.Load(dsdPath);
            var newDataObjectId = xdocDsd.Descendants("dataobject").Select(x => x.Attribute("id").Value).Where(x => x.All(char.IsNumber)).Select(x => int.Parse(x)).Max(x => x) + 1;
            var lastDataObject = xdocDsd.Descendants("dataobject").LastOrDefault();

            foreach (var binding in NewBindings)
            {
                
                var newbinding = binding.CloneBinding();
                newbinding.Id = newBindingId++;

                foreach (var newDataObject in newbinding.DataObjects)
                {
                    var bindingDataobject = newbinding.BindingElement.Descendants("dataobject").FirstOrDefault(x => x.Attribute("objectid").Value == newDataObject.Id.ToString());
                    newDataObject.Id = newDataObjectId++;
                    bindingDataobject.Attribute("objectid").Value = newDataObject.Id.ToString();

                    var propertyAttributes = newDataObject.DataObject.Descendants("property").Attributes();

                    if (propertyAttributes.Where(x => x.Value == "PointRefPointName").Any()) //point on "data" tab
                    {
                        string pointSearch = "PointRefPointName";
                        string paramSearch = "PointRefParamName";

                        var pointElement = propertyAttributes.FirstOrDefault(x => x.Value == pointSearch)?.Parent;
                        var parameterElement = propertyAttributes.FirstOrDefault(x => x.Value == paramSearch)?.Parent;

                        pointElement.Value = customParameters.FirstOrDefault(x => pointElement.Value.ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}").Value;
                        parameterElement.Value = customParameters.FirstOrDefault(x => parameterElement.Value.ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}").Value;
                    }
                    else if (propertyAttributes.Where(x => x.Value == "CommaDelimitedPointNames").Any()) //point(s) on "script data" tab
                    {
                        string pointSearch = "CommaDelimitedPointNames";
                        string paramSearch = "CommaDelimitedParameters";

                        var pointElement = propertyAttributes.FirstOrDefault(x => x.Value == pointSearch)?.Parent;
                        var parameterElement = propertyAttributes.FirstOrDefault(x => x.Value == paramSearch)?.Parent;

                        var points = pointElement.Value.Split(',');
                        var parameters = parameterElement.Value.Split(',');
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(points[i]) && !string.IsNullOrWhiteSpace(parameters[i]))
                            {
                                points[i] = customParameters.FirstOrDefault(x => points[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}").Value;
                                parameters[i] = customParameters.FirstOrDefault(x => parameters[i].ToUpper() == "{%" + x.Type.ToUpper() + "::" + x.Name.ToUpper() + "%}").Value;
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
    }
}
