using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;

namespace Honeywell.GUI.ExperionGfx
{
    public static class Extensions2
    {
        public static string? ConvertToString(this ICell? cell)
        {
            if (cell == null)
                return null;

            if (cell.CellType == CellType.String)
                return cell.StringCellValue;
            else if (cell.CellType == CellType.Numeric)
                return cell.NumericCellValue.ToString();
            else if (cell.CellType == CellType.Boolean)
                return cell.BooleanCellValue.ToString();

            return null;
        }

        public static int? ConvertToInt(this ICell? cell)
        {
            if (cell == null)
                return null;

            if (cell.CellType == CellType.Numeric)
                return (int)cell.NumericCellValue;
            else if (cell.CellType == CellType.String && int.TryParse(cell.StringCellValue, out int result))
                return result;

            return null;
        }

        public static bool ContainsNoCase(this List<string?> list, string? contains)
        {
            return list.Contains(contains, StringComparer.OrdinalIgnoreCase);
        }

        public static bool ContainsNoCase(this List<string?> list, string?[] contains)
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
