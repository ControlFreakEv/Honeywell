using System.Reflection;

namespace Honeywell.HMIWeb
{
    public class Shape
    {
        public string? Graphic { get; set; }
        public string? ShapeName { get; set; }
        public string? ShapeSource { get; set; }
        public string? ShapePath { get; set; }
        public string? Point { get; set; }
        public string? Parameter { get; set; }
        public string? MiscPropValue { get; set; }
        public string? DataObjectId { get; set; }
        public string? BindingId { get; set; }
        public ObjectType? DataType { get; set; }
        public string? NewPoint { get; set; }
        public string? NewParameter { get; set; }
        public string? NewMiscPropValue { get; set; }
        public string? CustomParamPointName { get; set; }
        public string? CustomParamParameterName { get; set; }
        public string? CustomPropMiscName { get; set; }
        public ObjectType? CustomParamType { get; set; }
        public int CsvIndex { get; set; }
        public string? ReplaceShapeName { get; set; }
        public string? GfxAndShape { get; set; }
        public int? UniqueShapeId { get; set; }
        public string? Tooltip { get; set; }
        public string? NewTooltip { get; set; }
        public string? Css { get; set; }
        public string? NewCss { get; set; }

        public enum ObjectType { DataTab, ScriptTab, CustomParameter, CustomParameterScript, CustomParameterData, CustomParameterEmbedded, CustomParameterMisc }

        public static ObjectType? ParseObjectType(string? type)
        {
            if (type == null)
                return null;

            if (type == ObjectType.DataTab.ToString())
                return ObjectType.DataTab;
            else if(type == ObjectType.ScriptTab.ToString())
                return ObjectType.ScriptTab;
            else if (type == ObjectType.CustomParameter.ToString())
                return ObjectType.CustomParameter;
            else if (type == ObjectType.CustomParameterScript.ToString())
                return ObjectType.CustomParameterScript;
            else if (type == ObjectType.CustomParameterData.ToString())
                return ObjectType.CustomParameterData;
            else if (type == ObjectType.CustomParameterEmbedded.ToString())
                return ObjectType.CustomParameterEmbedded;
            else if (type == ObjectType.CustomParameterMisc.ToString())
                return ObjectType.CustomParameterMisc;
            else
                return null;
        }

        public Shape Clone()
        {
            var shape = new Shape();
            foreach (PropertyInfo propertyInfo in typeof(Shape).GetProperties())
            {
                propertyInfo.SetValue(shape, propertyInfo.GetValue(this));
            }
            return shape;
        }
    }
}
