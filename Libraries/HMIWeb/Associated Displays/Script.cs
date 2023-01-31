using System.Text;
using System.Text.RegularExpressions;

namespace Honeywell.HMIWeb
{
    public class Script
    {
        public string InnerHtml { get; set; }
        public string[] MatchReplacement { get; set; }
        public Regex RegexScript { get; set; }
        public MatchCollection Matches { get; set; }

        public Script(string innerHtml)
        {
            InnerHtml = innerHtml;
            Regex RegexScript = new (@""".*?""", RegexOptions.IgnoreCase); //grab everything in between quotes'
            Matches = RegexScript.Matches(InnerHtml);

            MatchReplacement = new string[Matches.Count];
        }

        public void UpdateReplacements(string oldScriptTag, string newScriptTag)
        {
            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                var replacement = MatchReplacement[i];
                if (match.Value.ToUpper() == oldScriptTag.ToUpper() && replacement == null)
                    MatchReplacement[i] = newScriptTag;
            }
        }

        public string UpdateScript()
        {
            if (Matches.Any())
            {
                StringBuilder stringBuilder = new();

                var matchReplacement = new string[MatchReplacement.Where(x => x != null).Count()];
                var matches = new Match[matchReplacement.Length];
                var k = 0;
                for (int i = 0; i < MatchReplacement.Length; i++)
                {
                    var replacement = MatchReplacement[i];
                    var match = Matches[i];
                    if (replacement != null)
                    {
                        matchReplacement[k] = replacement;
                        matches[k] = match;
                        k++;
                    }
                }

                var length = matches.Length - 1;
                for (int i = length; i >= 0; i--)
                {
                    var match = matches[i];
                    var replacement = matchReplacement[i];
                    if (i == length)
                        stringBuilder.Insert(0, replacement + InnerHtml[(match.Index + match.Length)..]);
                    else
                        stringBuilder.Insert(0, replacement + InnerHtml[(match.Index + match.Length)..matches[i + 1].Index]);
                }
                stringBuilder.Insert(0, InnerHtml[0..matches[0].Index]);
                var newInnerText = stringBuilder.ToString();
                return newInnerText;
            }
            return InnerHtml;

        }
    }
}
