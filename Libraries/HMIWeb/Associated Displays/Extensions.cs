using System.Text.RegularExpressions;

namespace Honeywell.HMIWeb
{
    public static class Extensions
    {
        /// <summary>
        /// Replaces <paramref name="oldName"/> only where it appears as a whole point/parameter name,
        /// so renaming FIC10 leaves FIC101 alone. Case-insensitive, like Experion names.
        /// </summary>
        public static string ReplaceWholeName(this string text, string oldName, string newName)
        {
            var pattern = $@"(?<![A-Za-z0-9_$]){Regex.Escape(oldName)}(?![A-Za-z0-9_$])";
            // MatchEvaluator so '$' in the new name isn't treated as a substitution
            return Regex.Replace(text, pattern, _ => newName, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public static string? ToStringOrNull(this object obj)
        {
            var value = obj.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value;
        }
    }
}
