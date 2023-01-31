using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Honeywell.HMIWeb
{
    public class CustomParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        private static readonly Regex regexCustomParameter = new(@"(?<Type>\b\w+\b)\?(?<CustomProperty>\b\w+\b):", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public CustomParameter(string name, string type, string value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public static List<CustomParameter> GetListOfCustomParameters(IEnumerable<XElement> shapeParameters)
        {
            var customParameters = new List<CustomParameter>();
            foreach (var shapeParameter in shapeParameters)
            {
                if (shapeParameter == null)
                    continue;
                var name = shapeParameter?.Attribute("name")?.Value;
                var type = shapeParameter?.Attribute("type")?.Value;
                var value = shapeParameter?.Attribute("defaultvalue")?.Value;
                if (name != null && type != null && value != null)
                    customParameters.Add(new CustomParameter(name, type, value));
            }
            return customParameters;
        }

        public static List<CustomParameter> GetListOfCustomParameters(string htmParameters)
        {
            var customParameters = new List<CustomParameter>();
            //var shapeParameters = htmParameters.Split(';');
            var matches = regexCustomParameter.Matches(htmParameters);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var type = match.Groups["Type"].Value;
                var name = match.Groups["CustomProperty"].Value;
                string value = null;
                if (i == matches.Count - 1)
                {
                    value = htmParameters[(match.Index + match.Length)..(htmParameters.Length-1)];
                }
                else
                {
                    value = htmParameters[(match.Index + match.Length)..(matches[i+1].Index - 1)];
                }
                customParameters.Add(new CustomParameter(name, type, value));
            }
            //foreach (var shapeParameter in shapeParameters)
            //{
            //    if (string.IsNullOrWhiteSpace(shapeParameter))
            //        continue;
            //    var type = shapeParameter[..shapeParameter.IndexOf("?")];
            //    var name = shapeParameter[(type.Length + 1)..shapeParameter.IndexOf(":")];
            //    var value = shapeParameter[(shapeParameter.IndexOf(":") + 1)..];

            //    customParameters.Add(new CustomParameter(name, type, value));
            //}
            return customParameters;
        }
    }
}
