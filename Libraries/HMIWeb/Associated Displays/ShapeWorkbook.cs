using System.Data;
using ClosedXML.Excel;
using Honeywell.Excel;

namespace Honeywell.HMIWeb
{
    /// <summary>The "Associated Displays" workbook GFX Parser exports for editing and imports back.</summary>
    public static class ShapeWorkbook
    {
        public const string SheetName = "Gfx Refs";

        private static readonly string[] NumericProperties = { nameof(Shape.CsvIndex), nameof(Shape.UniqueShapeId) };

        /// <param name="table">Shape rows, with columns in the same order as <paramref name="headers"/>.</param>
        /// <param name="headers">Shape property name to column header.</param>
        public static void Export(DataTable table, IReadOnlyDictionary<string, string> headers, string path)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(SheetName);
            worksheet.WriteHeaders(headers.Values);

            var numeric = headers.Keys.Select(NumericProperties.Contains).ToArray();
            int rowNumber = 2;
            foreach (DataRow row in table.Rows)
                worksheet.WriteRow(rowNumber++, row.ItemArray.Select((value, column) => ToCellValue(value, numeric[column])));

            worksheet.FreezeHeaderAndFilter();
            workbook.SaveAs(path);
        }

        /// <param name="headers">Shape property name to column header.</param>
        /// <param name="continueWithoutColumn">Asked about each missing header; returning false cancels the import.</param>
        /// <returns>The shapes, or null if the import was cancelled.</returns>
        /// <exception cref="InvalidDataException">The workbook has no <see cref="SheetName"/> sheet.</exception>
        public static List<Shape>? Import(string path, IReadOnlyDictionary<string, string> headers, Func<string, bool> continueWithoutColumn)
        {
            using var workbook = new XLWorkbook(path);
            if (!workbook.TryGetWorksheet(SheetName, out var worksheet))
                throw new InvalidDataException($"Could not locate worksheet {SheetName}");

            var headerColumns = worksheet.HeaderColumns();
            var columns = new Dictionary<string, int>();
            foreach (var (property, header) in headers)
            {
                if (headerColumns.TryGetValue(header, out int column))
                    columns[property] = column;
                else if (!continueWithoutColumn(header))
                    return null;
            }

            return worksheet.DataRows().Select(row => ReadShape(row, columns)).ToList();
        }

        private static XLCellValue ToCellValue(object? value, bool numeric)
        {
            var text = value?.ToString();
            return numeric && int.TryParse(text, out int number) ? number : text;
        }

        private static Shape ReadShape(IXLRow row, Dictionary<string, int> columns)
        {
            string? Text(string property) => columns.TryGetValue(property, out int column) ? row.Cell(column).TextOrNull() : null;
            int? Int(string property) => columns.TryGetValue(property, out int column) ? row.Cell(column).IntOrNull() : null;

            return new Shape
            {
                ShapeSource = Text(nameof(Shape.ShapeSource)),
                Graphic = Text(nameof(Shape.Graphic)),
                ShapeName = Text(nameof(Shape.ShapeName)),
                ShapePath = Text(nameof(Shape.ShapePath)),
                Point = Text(nameof(Shape.Point)),
                Parameter = Text(nameof(Shape.Parameter)),
                DataType = Shape.ParseObjectType(Text(nameof(Shape.DataType))),
                DataObjectId = Text(nameof(Shape.DataObjectId)),
                BindingId = Text(nameof(Shape.BindingId)),
                NewPoint = Text(nameof(Shape.NewPoint)),
                NewParameter = Text(nameof(Shape.NewParameter)),
                CustomParamPointName = Text(nameof(Shape.CustomParamPointName)),
                CustomParamParameterName = Text(nameof(Shape.CustomParamParameterName)),
                CustomParamType = Shape.ParseObjectType(Text(nameof(Shape.CustomParamType))),
                CsvIndex = Int(nameof(Shape.CsvIndex)) ?? 0,
                ReplaceShapeName = Text(nameof(Shape.ReplaceShapeName)),
                MiscPropValue = Text(nameof(Shape.MiscPropValue)),
                CustomPropMiscName = Text(nameof(Shape.CustomPropMiscName)),
                NewMiscPropValue = Text(nameof(Shape.NewMiscPropValue)),
                GfxAndShape = Text(nameof(Shape.GfxAndShape)),
                UniqueShapeId = Int(nameof(Shape.UniqueShapeId)),
                Tooltip = Text(nameof(Shape.Tooltip)),
                NewTooltip = Text(nameof(Shape.NewTooltip)),
                Css = Text(nameof(Shape.Css)),
                NewCss = Text(nameof(Shape.NewCss)),
            };
        }
    }
}
