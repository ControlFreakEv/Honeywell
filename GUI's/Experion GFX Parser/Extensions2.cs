using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.ExperionGfx
{
    public static class Extensions2
    {
        public static bool ContainsNoCase(this IEnumerable<string?> list, string? contains)
        {
            return list.Contains(contains, StringComparer.OrdinalIgnoreCase);
        }

        public static bool ContainsNoCase(this IEnumerable<string?> list, string?[] contains)
        {
            foreach (var item in contains)
            {
                if (list.ContainsNoCase(item))
                    return true;
            }

            return false;
        }
    }
}
