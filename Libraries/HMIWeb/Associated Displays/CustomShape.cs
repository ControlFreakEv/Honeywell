using System.Text.RegularExpressions;
using static Honeywell.HMIWeb.Shape;

namespace Honeywell.HMIWeb
{
    public class CustomShape
    {
        public string ShapeName { get; set; }
        public string SubShapeName { get; set; }
        public string? PointName { get; set; }
        public string? ParameterName { get; set; }
        public string? MiscProp { get; set; }
        public ObjectType? DataType { get; set; }
        public int CsvIndex { get; set; }
        private static readonly Regex regexCustomParameter = new(@"{%(?<Type>\w+)::(?<CustomProperty>\w+)%}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex regexShapeId = new(@"\bid\b=(?<ShapeId>\b\w+\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string? GetCustomProperty(string? input)
        {
            if (input == null)
                return null;

            var match = regexCustomParameter.Match(input);
            if (match.Success)
                return match.Groups["CustomProperty"].Value;
            else
                return null;
        }

        public static MatchCollection? GetCustomProperties(string? input)
        {
            if (input == null)
                return null;

            var matches = regexCustomParameter.Matches(input);
            if (matches.Any())
                return matches;
            else
                return null;
        }

        public static string? GetShapeId(string? input)
        {
            if (input == null)
                return null;

            var match = regexShapeId.Match(input);
            if (match.Success)
                return match.Groups["ShapeId"].Value;
            else
                return null;
        }
    }
}
