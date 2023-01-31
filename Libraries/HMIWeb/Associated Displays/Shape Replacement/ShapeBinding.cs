using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Honeywell.HMIWeb
{
    public class ShapeBinding
    {
        public XElement BindingElement { get; set; }
        private int id;
        public int Id 
        { 
            get
            {
                return id;
            }
            set
            {
                id = value;
                BindingElement.Attribute("ID").Value = id.ToString();
            }
        }
        public List<ShapeDataObject> DataObjects { get; set; } = new List<ShapeDataObject>();
        private static Regex regexbinding = new(@"HDXBINDINGID:(?<Id>\d+);", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ShapeBinding(XElement binding, List<ShapeDataObject> dataObjects)
        {
            BindingElement = binding;
            id = int.Parse(binding.Attribute("ID").Value);
            DataObjects = dataObjects;
        }

        public static string? GetIdFromHtml(HtmlNode node)
        {
            var hdxProp = node.Attributes.Where(x => x.Name == "hdxproperties").FirstOrDefault();
            if (hdxProp != null)
            {
                var match = regexbinding.Match(hdxProp.Value).Groups["Id"];
                return match.Value;
            }
            return null;
        }

        public ShapeBinding CloneBinding()
        {
            var newElement = new XElement(BindingElement);
            var newdataObjects = new List<ShapeDataObject>();
            foreach (var item in DataObjects)
            {
                newdataObjects.Add(item.CopyDataObject());
            }
            var shapeBinding = new ShapeBinding(newElement, newdataObjects);
            return shapeBinding;
        }
    }
}
