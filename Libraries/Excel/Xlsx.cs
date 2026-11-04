using System.Globalization;
using ClosedXML.Excel;

namespace Honeywell.Excel
{
    /// <summary>The worksheet conventions shared by every export and import: one header row, then data.</summary>
    public static class Xlsx
    {
        private const int HeaderRow = 1;

        public static void WriteRow(this IXLWorksheet worksheet, int rowNumber, IEnumerable<XLCellValue> values)
        {
            int column = 1;
            foreach (var value in values)
                worksheet.Cell(rowNumber, column++).Value = value;
        }

        public static void WriteHeaders(this IXLWorksheet worksheet, IEnumerable<string> headers) =>
            worksheet.WriteRow(HeaderRow, headers.Select(header => (XLCellValue)header));

        /// <summary>Freezes the header row and puts a filter over the data.</summary>
        public static void FreezeHeaderAndFilter(this IXLWorksheet worksheet)
        {
            worksheet.SheetView.FreezeRows(HeaderRow);
            worksheet.RangeUsed()?.SetAutoFilter();
        }

        /// <summary>Maps each header's text to its column number.</summary>
        public static Dictionary<string, int> HeaderColumns(this IXLWorksheet worksheet)
        {
            var columns = new Dictionary<string, int>();
            foreach (var cell in worksheet.Row(HeaderRow).CellsUsed())
                if (cell.TextOrNull() is string header)
                    columns[header] = cell.Address.ColumnNumber;
            return columns;
        }

        /// <summary>Every row below the header that has content. Blank rows are skipped.</summary>
        public static IEnumerable<IXLRow> DataRows(this IXLWorksheet worksheet) =>
            worksheet.RowsUsed().Where(row => row.RowNumber() > HeaderRow);

        /// <summary>
        /// A cell's value as text, with numbers in invariant culture. Formula cells give their last calculated
        /// result; blank and error cells give null.
        /// </summary>
        public static string? TextOrNull(this IXLCell cell)
        {
            var value = cell.CachedValue;
            if (value.IsText)
                return value.GetText();
            if (value.IsBoolean)
                return value.GetBoolean().ToString();
            if (value.IsUnifiedNumber)
                return value.GetUnifiedNumber().ToString(CultureInfo.InvariantCulture);
            return null;
        }

        /// <summary>A cell's value as an int, whether it is stored as a number or as text.</summary>
        public static int? IntOrNull(this IXLCell cell)
        {
            var value = cell.CachedValue;
            if (value.IsUnifiedNumber)
                return (int)value.GetUnifiedNumber();
            if (value.IsText && int.TryParse(value.GetText(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                return result;
            return null;
        }
    }
}
