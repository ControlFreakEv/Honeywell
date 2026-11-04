using System.Text;
using System.Text.RegularExpressions;

namespace Honeywell.HMIWeb
{
    /// <summary>
    /// A display script's quoted strings, with pending renames. Renames are matched against the original text,
    /// so a name that is renamed to another renamed name isn't changed twice.
    /// </summary>
    public class Script
    {
        private static readonly Regex QuotedString = new(@""".*?""", RegexOptions.IgnoreCase);

        private readonly string innerHtml;
        private readonly MatchCollection quotedStrings;
        private readonly string?[] replacements;

        public Script(string innerHtml)
        {
            this.innerHtml = innerHtml;
            quotedStrings = QuotedString.Matches(innerHtml);
            replacements = new string?[quotedStrings.Count];
        }

        public void UpdateReplacements(string oldScriptTag, string newScriptTag)
        {
            for (int i = 0; i < quotedStrings.Count; i++)
                if (replacements[i] == null && quotedStrings[i].Value.Equals(oldScriptTag, StringComparison.OrdinalIgnoreCase))
                    replacements[i] = newScriptTag;
        }

        public string UpdateScript()
        {
            var result = new StringBuilder();
            int position = 0;
            for (int i = 0; i < quotedStrings.Count; i++)
            {
                if (replacements[i] == null)
                    continue;

                result.Append(innerHtml, position, quotedStrings[i].Index - position).Append(replacements[i]);
                position = quotedStrings[i].Index + quotedStrings[i].Length;
            }
            return result.Append(innerHtml, position, innerHtml.Length - position).ToString();
        }
    }
}
