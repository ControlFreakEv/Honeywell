using System.Data;
using ClosedXML.Excel;
using Honeywell.Database;
using Honeywell.Excel;
using Honeywell.HMIWeb;
using Honeywell.Parsers.TDC;
using Honeywell.TDC;

namespace Honeywell.Tests
{
    public class XlsxCellTests
    {
        [Fact]
        public void NumbersReadAsInvariantText_UnderCommaCulture()
        {
            using var _ = new CultureScope("de-DE");
            using var workbook = new XLWorkbook();
            var cell = workbook.Worksheets.Add("S").Cell(1, 1);
            cell.Value = 1.5;

            Assert.Equal("1.5", cell.TextOrNull());
        }

        [Fact]
        public void ReadsEachCellKind()
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("S");
            sheet.Cell(1, 1).Value = "007";
            sheet.Cell(1, 2).Value = 7;
            sheet.Cell(1, 3).Value = true;
            sheet.Cell(1, 4).FormulaA1 = "=1+2";
            sheet.Cell(1, 5).Value = (string?)null;

            Assert.Equal("007", sheet.Cell(1, 1).TextOrNull());
            Assert.Equal(7, sheet.Cell(1, 1).IntOrNull());
            Assert.Equal("7", sheet.Cell(1, 2).TextOrNull());
            Assert.Equal(7, sheet.Cell(1, 2).IntOrNull());
            Assert.Equal("True", sheet.Cell(1, 3).TextOrNull());
            Assert.Null(sheet.Cell(1, 5).TextOrNull());
            Assert.Null(sheet.Cell(1, 5).IntOrNull());
        }

        [Fact]
        public void FormulaCellsGiveTheirCalculatedResult_AfterSaving()
        {
            using var temp = new TempFolder();
            var path = temp.File("formula.xlsx");
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add("S").Cell(1, 1).FormulaA1 = "=1+2";
                workbook.SaveAs(path, validate: true, evaluateFormulae: true); // as Excel does
            }

            using var reopened = new XLWorkbook(path);
            Assert.Equal("3", reopened.Worksheet("S").Cell(1, 1).TextOrNull());
        }
    }

    public class ShapeWorkbookTests
    {
        private static readonly Dictionary<string, string> Headers = new()
        {
            { nameof(Shape.UniqueShapeId), "Id" },
            { nameof(Shape.Graphic), "Graphic" },
            { nameof(Shape.Point), "Point" },
            { nameof(Shape.NewPoint), "New Point" },
            { nameof(Shape.DataType), "Type" },
            { nameof(Shape.CsvIndex), "Index" },
        };

        private static DataTable Table()
        {
            var table = new DataTable();
            foreach (var header in Headers.Values)
                table.Columns.Add(header);
            table.Rows.Add("12", "G1.htm", "FIC10", DBNull.Value, "DataTab", "3");
            table.Rows.Add("13", "G2.htm", "0012", "FIC20", "ScriptTab", "4");
            return table;
        }

        [Fact]
        public void RoundTripsValues()
        {
            using var temp = new TempFolder();
            var path = temp.File("shapes.xlsx");

            ShapeWorkbook.Export(Table(), Headers, path);
            var shapes = ShapeWorkbook.Import(path, Headers, _ => throw new Exception("no header should be missing"))!;

            Assert.Equal(2, shapes.Count);
            Assert.Equal(12, shapes[0].UniqueShapeId);
            Assert.Equal("G1.htm", shapes[0].Graphic);
            Assert.Equal("FIC10", shapes[0].Point);
            Assert.True(string.IsNullOrEmpty(shapes[0].NewPoint));
            Assert.Equal(Shape.ObjectType.DataTab, shapes[0].DataType);
            Assert.Equal(3, shapes[0].CsvIndex);
            // A point name that looks like a number must stay text
            Assert.Equal("0012", shapes[1].Point);
            Assert.Equal("FIC20", shapes[1].NewPoint);
        }

        [Fact]
        public void FileOpenInAnotherProgram_RaisesIOException()
        {
            // The forms turn this into "Close the file and try again"
            using var temp = new TempFolder();
            var path = temp.File("shapes.xlsx");
            ShapeWorkbook.Export(Table(), Headers, path);

            using var excel = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            Assert.ThrowsAny<IOException>(() => ShapeWorkbook.Export(Table(), Headers, path));
            Assert.ThrowsAny<IOException>(() => ShapeWorkbook.Import(path, Headers, _ => true));
        }

        [Fact]
        public void IdAndIndexAreNumericCells_WithFrozenHeaderAndFilter()
        {
            using var temp = new TempFolder();
            var path = temp.File("shapes.xlsx");

            ShapeWorkbook.Export(Table(), Headers, path);

            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheet(ShapeWorkbook.SheetName);
            Assert.True(sheet.Cell(2, 1).Value.IsNumber);
            Assert.True(sheet.Cell(2, 6).Value.IsNumber);
            Assert.True(sheet.Cell(3, 3).Value.IsText);
            Assert.Equal(1, sheet.SheetView.SplitRow);
            Assert.True(sheet.AutoFilter.IsEnabled);
        }

        [Fact]
        public void MissingHeader_CanContinueOrCancel()
        {
            using var temp = new TempFolder();
            var path = temp.File("shapes.xlsx");
            ShapeWorkbook.Export(Table(), Headers, path);
            var moreHeaders = new Dictionary<string, string>(Headers) { { nameof(Shape.Css), "Css" } };

            var asked = new List<string>();
            var shapes = ShapeWorkbook.Import(path, moreHeaders, header => { asked.Add(header); return true; })!;
            Assert.Equal(new[] { "Css" }, asked);
            Assert.Null(shapes[0].Css);
            Assert.Equal("FIC10", shapes[0].Point);

            Assert.Null(ShapeWorkbook.Import(path, moreHeaders, _ => false));
        }

        [Fact]
        public void BlankRowsAreSkipped_AndMissingSheetIsReported()
        {
            using var temp = new TempFolder();
            var path = temp.File("shapes.xlsx");
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add(ShapeWorkbook.SheetName);
                sheet.WriteHeaders(Headers.Values);
                sheet.Cell(2, 2).Value = "G1.htm";
                sheet.Cell(4, 2).Value = "G2.htm";
                workbook.SaveAs(path);
            }
            Assert.Equal(new[] { "G1.htm", "G2.htm" }, ShapeWorkbook.Import(path, Headers, _ => true)!.Select(x => x.Graphic));

            var other = temp.File("other.xlsx");
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add("Other");
                workbook.SaveAs(other);
            }
            Assert.Throws<InvalidDataException>(() => ShapeWorkbook.Import(other, Headers, _ => true));
        }
    }

    public class DatabaseWorkbookTests
    {
        private static void CreateDatabase()
        {
            using var db = new TdcContext();
            db.Database.EnsureCreated();
            db.Projects.Add(new DbProject { Id = 5, Name = "P5" });
            db.Projects.Add(new DbProject { Id = 9, Name = "P9" });
            db.SaveChanges();
        }

        private static string Export(TempFolder temp)
        {
            var path = temp.File("db.xlsx");
            DatabaseWorkbook.Export(new[] { DatabaseWorkbook.Projects, DatabaseWorkbook.TdcTags }, path);
            return path;
        }

        private static void EditProjects(string path, Action<IXLWorksheet, Dictionary<string, int>> edit)
        {
            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheet(DatabaseWorkbook.Projects);
            edit(sheet, sheet.HeaderColumns());
            workbook.SaveAs(path);
        }

        [Fact]
        public void ExportWritesSheetsInOrder_WithPropertyHeaders()
        {
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));
            CreateDatabase();

            using var workbook = new XLWorkbook(Export(temp));

            Assert.Equal(new[] { DatabaseWorkbook.Projects, DatabaseWorkbook.TdcTags }, workbook.Worksheets.Select(x => x.Name));
            var columns = workbook.Worksheet(DatabaseWorkbook.Projects).HeaderColumns();
            Assert.Contains(nameof(DbProject.Id), columns.Keys);
            Assert.Contains(nameof(DbProject.Name), columns.Keys);
        }

        [Fact]
        public void ImportAppliesEdits_NewRows_AndDeleteFlags()
        {
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));
            CreateDatabase();
            var path = Export(temp);

            EditProjects(path, (sheet, columns) =>
            {
                sheet.Cell(2, columns[nameof(DbProject.Name)]).Value = "renamed";
                sheet.Cell(3, columns[nameof(DbProject.DeleteOnImport)]).Value = "True";
                sheet.Cell(4, columns[nameof(DbProject.Name)]).Value = "brand new";
            });

            Assert.False(DatabaseWorkbook.Import(path));

            using var check = new TdcContext();
            Assert.Equal("renamed", check.Projects.Single(x => x.Id == 5).Name);
            Assert.DoesNotContain(check.Projects, x => x.Id == 9);
            Assert.Equal("brand new", check.Projects.Single(x => x.Id == 10).Name);
        }

        [Fact]
        public void ImportWithoutKeyColumn_IsReportedAndChangesNothing()
        {
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));
            CreateDatabase();
            var path = temp.File("bad.xlsx");
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add(DatabaseWorkbook.Projects);
                sheet.WriteHeaders(new[] { nameof(DbProject.Name) });
                sheet.Cell(2, 1).Value = "x";
                workbook.SaveAs(path);
            }

            Assert.Throws<InvalidDataException>(() => DatabaseWorkbook.Import(path));

            using var check = new TdcContext();
            Assert.Equal(2, check.Projects.Count());
        }

        [Fact]
        public void NumericCellImportsAsInvariantText_UnderCommaCulture()
        {
            using var _culture = new CultureScope("de-DE");
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));
            CreateDatabase();
            var path = Export(temp);

            EditProjects(path, (sheet, columns) => sheet.Cell(2, columns[nameof(DbProject.Name)]).Value = 1.5);
            DatabaseWorkbook.Import(path);

            using var check = new TdcContext();
            Assert.Equal("1.5", check.Projects.Single(x => x.Id == 5).Name);
        }
    }

    public class TdcExportTests
    {
        [Fact]
        public void ExportsTagsAndParameters()
        {
            using var temp = new TempFolder();
            var path = temp.File("modules.xlsx");
            Parser.TdcTags.Clear();
            Parser.TdcParameters.Clear();
            var tags = Parser.GetModules(Fixtures.EbFolder, SearchOption.TopDirectoryOnly);

            Export.ExportTdcToXlsx(tags, path);

            using var workbook = new XLWorkbook(path);
            var tagSheet = workbook.Worksheet("TDC Tags");
            int nameColumn = tagSheet.HeaderColumns()["TDC Tag"];
            Assert.Contains("FIC100", tagSheet.DataRows().Select(row => row.Cell(nameColumn).TextOrNull()));
            Assert.True(tagSheet.AutoFilter.IsEnabled);
            Assert.Equal(tags.Sum(x => x.Params.Count), workbook.Worksheet("Parameters").DataRows().Count());
        }

        [Fact]
        public void GroupHeadersAreIndexed()
        {
            using var temp = new TempFolder();
            var path = temp.File("groups.xlsx");
            var group = new TdcGroup();

            Export.ExportGroups(new List<TdcGroup> { group }, path);

            using var workbook = new XLWorkbook(path);
            var headers = workbook.Worksheet("TDC Groups").HeaderColumns();
            Assert.Contains($"SCREENUM[{TdcGroup.OffsetSCREENUM}]", headers.Keys);
            Assert.Contains($"SCREENUM[{TdcGroup.OffsetSCREENUM + group.SCREENUM.Length - 1}]", headers.Keys);
            Assert.Contains($"DISPID[{TdcGroup.OffsetDISPID + group.DISPID.Length - 1}]", headers.Keys);
        }
    }
}
