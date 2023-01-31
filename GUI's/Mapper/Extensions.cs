using Honeywell.Database;
using Mapper.Samples;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zuby.ADGV;

namespace Honeywell.GUI.Mapper
{
    public static class Extensions
    {
        public static bool Like(this string input, string pattern)
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool Contains(this string[] source, string toCheck, StringComparison comp)
        {
            foreach (var item in source)
            {
                if (toCheck == null)
                    return false;

                 if (item.Contains(toCheck, comp))
                    return true;
            }
            return false;
        }

        public static string RemoveLiterals(this string s)
        {
            return s.Replace("[%]", "%").Replace("[[]", "[").Replace("[]]", "]");
        }

        public static void Search(this AdvancedDataGridView dgv, AdvancedDataGridViewSearchToolBarSearchEventArgs e)
        {
            bool restartsearch = true;
            int startColumn = 0;
            int startRow = 0;
            if (!e.FromBegin)
            {
                bool endcol = dgv.CurrentCell.ColumnIndex + 1 >= dgv.ColumnCount;
                bool endrow = dgv.CurrentCell.RowIndex + 1 >= dgv.RowCount;

                if (endcol && endrow)
                {
                    startColumn = dgv.CurrentCell.ColumnIndex;
                    startRow = dgv.CurrentCell.RowIndex;
                }
                else
                {
                    startColumn = endcol ? 0 : dgv.CurrentCell.ColumnIndex + 1;
                    startRow = dgv.CurrentCell.RowIndex + (endcol ? 1 : 0);
                }
            }
            DataGridViewCell c = dgv.FindCell(e.ValueToSearch.Trim(), e.ColumnToSearch != null ? e.ColumnToSearch.Name : null, startRow, startColumn, e.WholeWord, e.CaseSensitive);
            if (c == null && restartsearch)
                c = dgv.FindCell(e.ValueToSearch.Trim(), e.ColumnToSearch != null ? e.ColumnToSearch.Name : null, 0, 0, e.WholeWord, e.CaseSensitive);
            if (c != null)
                dgv.CurrentCell = c;
        }

        public static string RegexReplace(this string input, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }
    }
}
