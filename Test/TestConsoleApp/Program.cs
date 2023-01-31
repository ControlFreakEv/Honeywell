using System.Text.RegularExpressions;

var input = "= -999  - 9 -- comment";
input = Regex.Replace(input, "[^-=](?<minus>-)\\s+|[^=]\\\\s+(?<minus>-)[^-]|[^=]\\s+(?<minus>-)\\s+", " - ", RegexOptions.Multiline);

Console.WriteLine(input);
Console.ReadLine();