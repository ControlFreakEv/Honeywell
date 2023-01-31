using Honeywell.Database;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XWPF.UserModel;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Honeywell.GUI.Mapper
{
    public static class ExcelHelper<TEntity> where TEntity : class
    {
        public static void ExportSheet(IWorkbook workbook, string sheetName, DbSet<TEntity> entityList)
        {
            ISheet ws = workbook.CreateSheet(sheetName);

            int colIndex = 0;
            int rowIndex = 0;

            var propInfoArray = typeof(TEntity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ImportAndExport)));

            IRow row = ws.CreateRow(rowIndex++);

            foreach (PropertyInfo prop in propInfoArray)
                row.CreateCell(colIndex++).SetCellValue(prop.Name);

            if (!entityList.Any())
                return;

            foreach (var entity in entityList)
            {
                row = ws.CreateRow(rowIndex++);
                colIndex = 0;

                foreach (PropertyInfo prop in propInfoArray)
                {
                    var valueObject = prop.GetValue(entity);
                    var value = valueObject?.ToString();
                    if (valueObject is DbTdcModule dbTdcModule)
                        value = dbTdcModule.Name;
                    else if (valueObject is DbTdcCL dbTdcCL)
                        value = dbTdcCL.FileName;
                    else if (valueObject is DbProject dbProject)
                        value = dbProject.Name;
                    else if (valueObject is DbConfigTemplates dbConfigTemplates)
                        value = dbConfigTemplates.TypicalName;
                    else if (valueObject is DbTdcNode dbTdcNode)
                        value = dbTdcNode.NodeId;
                    else if (valueObject is DbTdcTag dbTdcTag)
                        value = dbTdcTag.Name;

                    row.CreateCell(colIndex++).SetCellValue(value);
                }
            }
        }

        public static void ImportSheet(ISheet ws, DbSet<TEntity> entityList)
        {
            if (ws == null)
                return;

            var dict = new Dictionary<string, int>();
            var headerRow = ws.GetRow(0);
            for (int k = 0; k < headerRow.Cells.Count; k++)
                dict[headerRow.Cells[k].StringCellValue] = k;

            var keyProp = typeof(TEntity).GetProperties().First(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
            var properties = typeof(TEntity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ImportAndExport)) && ((ImportAndExport)prop.GetCustomAttribute(typeof(ImportAndExport))).Import);

            int? newId = null;
            for (int k = 1; k <= ws.LastRowNum; k++)
            {
                var row = ws.GetRow(k);

                var idCell = row.Cells.FirstOrDefault(x => x.ColumnIndex == dict[keyProp.Name]);

                int? id = null;
                if (idCell?.CellType == CellType.Numeric)
                    id = (int?)idCell?.NumericCellValue;
                else if (idCell?.CellType == CellType.String)
                    if (int.TryParse(idCell?.StringCellValue, out int tempId))
                        id = tempId;

                if (!entityList.Any())
                {
                    id = 0;
                    newId = 1;
                }
                else if (id == null)
                {
                    if (newId != null)
                        id = ++newId;
                    else
                    {
                        newId = 0;
                        foreach (var ent in entityList)
                        {
                            var entId = (int?)keyProp.GetValue(ent);
                            if (entId != null && entId > newId)
                                newId = entId;
                        }
                        id = ++newId;
                    }
                }

                var entity = entityList.Find(id);
                if (entity == null)
                {
                    entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                    keyProp.SetValue(entity, id);
                    entityList.Add(entity);
                }

                foreach (var prop in properties)
                {
                    var cell = row.Cells.FirstOrDefault(x => x.ColumnIndex == dict[prop.Name]);
                    cell?.SetCellType(CellType.String);

                    var value = cell?.StringCellValue;
                    if (string.IsNullOrWhiteSpace(value))
                        value = null;

                    var convertedValue = ChangeType(value, prop);
                    prop.SetValue(entity, convertedValue);
                }
            }  
        }

        public static object? ChangeType(object? value, PropertyInfo prop)
        {
            var type = prop.PropertyType;
            NullabilityInfoContext context = new();
            var nullability = context.Create(prop).WriteState;
            
            if (nullability == NullabilityState.Nullable) //if value is null and it is nullable
            {
                if (value == null)
                    return null;

                type = Nullable.GetUnderlyingType(type) ?? type;
            }
            else if (value == null) //if value is null and it is not nullable
            {
                if (type == typeof(string))
                    value = string.Empty;
                else
                    value = Activator.CreateInstance(type);
            }

            return Convert.ChangeType(value, type);
        }

        private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes)
        {
            if (memberType.IsValueType)
                return Nullable.GetUnderlyingType(memberType) != null;

            var nullable = customAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            if (nullable != null && nullable.ConstructorArguments.Count == 1)
            {
                var attributeArgument = nullable.ConstructorArguments[0];
                if (attributeArgument.ArgumentType == typeof(byte[]))
                {
                    var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                    if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                    {
                        return (byte)args[0].Value! == 2;
                    }
                }
                else if (attributeArgument.ArgumentType == typeof(byte))
                {
                    return (byte)attributeArgument.Value! == 2;
                }
            }

            for (var type = declaringType; type != null; type = type.DeclaringType)
            {
                var context = type.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (context != null &&
                    context.ConstructorArguments.Count == 1 &&
                    context.ConstructorArguments[0].ArgumentType == typeof(byte))
                {
                    return (byte)context.ConstructorArguments[0].Value! == 2;
                }
            }

            // Couldn't find a suitable attribute
            return false;
        }
    }
}
