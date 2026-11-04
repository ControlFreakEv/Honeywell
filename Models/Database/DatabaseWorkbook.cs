using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace Honeywell.Database
{
    /// <summary>The Excel workbook Mapper exports the database to and imports edits back from.</summary>
    public static class DatabaseWorkbook
    {
        public const string ConfigTemplates = "Config Templates";
        public const string ExperionParameters = "Module Parameters";
        public const string Projects = "Projects";
        public const string TdcCl = "CL";
        public const string TdcClRefs = "CL Connections";
        public const string TdcConnections = "Tag Connections";
        public const string TdcFileRef = "File References";
        public const string TdcModules = "Modules";
        public const string TdcNodes = "Nodes in Maps";
        public const string TdcParameters = "TDC Parameters";
        public const string TdcTags = "TDC Tags";

        private static readonly ISheet[] Sheets =
        {
            new Sheet<DbProject>(Projects, db => db.Projects),
            new Sheet<DbTdcModule>(TdcModules, db => db.TdcModules, AffectsModules: true),
            new Sheet<DbExperionParameter>(ExperionParameters, db => db.ExperionParameters),
            new Sheet<DbConfigTemplates>(ConfigTemplates, db => db.ConfigTemplates),
            new Sheet<DbTdcNode>(TdcNodes, db => db.TdcNodes, AffectsModules: true),
            new Sheet<DbTdcTag>(TdcTags, db => db.TdcTags),
            new Sheet<DbTdcParameter>(TdcParameters, db => db.TdcParameters),
            new Sheet<DbTdcConnections>(TdcConnections, db => db.ParameterConnections),
            new Sheet<DbTdcCL>(TdcCl, db => db.TdcCLs),
            new Sheet<DbTdcCLRefs>(TdcClRefs, db => db.TdcCLRefs),
            new Sheet<DbTdcFileRef>(TdcFileRef, db => db.TdcFileRefs),
        };

        /// <summary>Writes the named sheets, in the order given, to a new workbook.</summary>
        public static void Export(IEnumerable<string> sheetNames, string path)
        {
            using var workbook = new XLWorkbook();
            using var db = new TdcContext();
            foreach (var sheetName in sheetNames)
                Sheets.Single(sheet => sheet.Name == sheetName).Export(workbook, db);
            workbook.SaveAs(path);
        }

        /// <summary>Applies a workbook to the database. Sheets with unknown names are ignored.</summary>
        /// <returns>True if module or node data changed, so open map views need refreshing.</returns>
        public static bool Import(string path)
        {
            using var workbook = new XLWorkbook(path);
            using var db = new TdcContext();

            bool modulesChanged = false;
            foreach (var worksheet in workbook.Worksheets)
            {
                var sheet = Sheets.SingleOrDefault(x => x.Name == worksheet.Name);
                if (sheet == null)
                    continue;

                sheet.Import(worksheet, db);
                modulesChanged |= sheet.AffectsModules;
            }

            db.SaveChanges();
            return modulesChanged;
        }

        private interface ISheet
        {
            string Name { get; }
            bool AffectsModules { get; }
            void Export(XLWorkbook workbook, TdcContext db);
            void Import(IXLWorksheet worksheet, TdcContext db);
        }

        private sealed record Sheet<TEntity>(string Name, Func<TdcContext, DbSet<TEntity>> Table, bool AffectsModules = false) : ISheet
            where TEntity : class, IDeleteOnImport
        {
            public void Export(XLWorkbook workbook, TdcContext db) => ExcelSheet<TEntity>.Export(workbook, Name, Table(db));

            public void Import(IXLWorksheet worksheet, TdcContext db)
            {
                var table = Table(db);
                ExcelSheet<TEntity>.Import(worksheet, table);
                db.SaveChanges();
                table.RemoveRange(table.Where(entity => entity.DeleteOnImport).ToList());
            }
        }
    }
}
