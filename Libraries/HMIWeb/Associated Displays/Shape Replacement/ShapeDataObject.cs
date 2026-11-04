using System.Xml.Linq;

namespace Honeywell.HMIWeb
{
    public class ShapeDataObject
    {
        public XElement DataObject { get; set; }
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
                DataObject.SetAttributeValue("id", id.ToString());
            }
        }

        public ShapeDataObject(XElement dataObject)
        {
            DataObject = dataObject;
            id = int.Parse(dataObject.Attribute("id")?.Value ?? throw new InvalidOperationException("Data object has no 'id' attribute: " + dataObject));
        }

        public ShapeDataObject CopyDataObject()
        {
            return new ShapeDataObject(new XElement(DataObject));
        }
    }
}
