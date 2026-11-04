using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using ClosedXML.Excel;
using Honeywell.Excel;
using Microsoft.EntityFrameworkCore;

namespace Honeywell.Database
{
    /// <summary>Copies a table to and from a worksheet using the properties marked [ImportAndExport].</summary>
    public static class ExcelSheet<TEntity> where TEntity : class
    {
        private static readonly PropertyInfo[] Exported = typeof(TEntity).GetProperties()
            .Where(prop => Attribute.IsDefined(prop, typeof(ImportAndExport)))
            .ToArray();

        private static readonly PropertyInfo[] Imported = Exported
            .Where(prop => prop.GetCustomAttribute<ImportAndExport>()?.Import == true)
            .ToArray();

        private static readonly PropertyInfo KeyProperty = typeof(TEntity).GetProperties()
            .First(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));

        public static void Export(XLWorkbook workbook, string sheetName, DbSet<TEntity> entities)
        {
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.WriteHeaders(Exported.Select(prop => prop.Name));

            int rowNumber = 2;
            foreach (var entity in entities)
                worksheet.WriteRow(rowNumber++, Exported.Select(prop => (XLCellValue)ExportText(prop.GetValue(entity))));
        }

        public static void Import(IXLWorksheet worksheet, DbSet<TEntity> entities)
        {
            var columns = worksheet.HeaderColumns();
            if (!columns.TryGetValue(KeyProperty.Name, out int keyColumn))
                throw new InvalidDataException($"Sheet '{worksheet.Name}' has no {KeyProperty.Name} column.");

            int? lastAssignedId = null;
            foreach (var row in worksheet.DataRows())
            {
                int? id = row.Cell(keyColumn).IntOrNull();

                if (!entities.Any())
                {
                    id = 0;
                    lastAssignedId = 1;
                }
                else if (id == null)
                {
                    lastAssignedId = (lastAssignedId ?? HighestId(entities)) + 1;
                    id = lastAssignedId;
                }

                var entity = entities.Find(id);
                if (entity == null)
                {
                    entity = Activator.CreateInstance<TEntity>();
                    KeyProperty.SetValue(entity, id);
                    entities.Add(entity);
                }

                foreach (var prop in Imported)
                {
                    // A column left out of the sheet leaves that value as it is
                    if (!columns.TryGetValue(prop.Name, out int column))
                        continue;

                    var text = row.Cell(column).TextOrNull();
                    prop.SetValue(entity, ConvertToProperty(string.IsNullOrWhiteSpace(text) ? null : text, prop));
                }
            }
        }

        private static string? ExportText(object? value) => value switch
        {
            DbTdcModule module => module.Name,
            DbTdcCL cl => cl.FileName,
            DbProject project => project.Name,
            DbConfigTemplates template => template.TypicalName,
            DbTdcNode node => node.NodeId,
            DbTdcTag tag => tag.Name,
            _ => value?.ToString(),
        };

        private static int HighestId(DbSet<TEntity> entities)
        {
            int highest = 0;
            foreach (var entity in entities)
                highest = Math.Max(highest, (int?)KeyProperty.GetValue(entity) ?? 0);
            return highest;
        }

        private static object? ConvertToProperty(string? text, PropertyInfo prop)
        {
            var type = prop.PropertyType;
            if (new NullabilityInfoContext().Create(prop).WriteState == NullabilityState.Nullable)
            {
                if (text == null)
                    return null;
                type = Nullable.GetUnderlyingType(type) ?? type;
            }
            else if (text == null)
            {
                return type == typeof(string) ? string.Empty : Activator.CreateInstance(type);
            }

            return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
        }
    }
}
