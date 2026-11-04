using System.Data;
using System.Data.OleDb;
using System.Runtime.Versioning;
using System.Text;
using Honeywell.Database;
using Microsoft.VisualBasic.FileIO;

namespace Honeywell.Parsers.TDC
{
    /// <summary>Reads a Doc3000 Access database. May need "Microsoft Access Database Engine 2010" repaired in add/remove programs.</summary>
    [SupportedOSPlatform("windows")]
    public static class D3kImporter
    {
        public static string EbFilePath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Mapper.eb");

        private static OleDbConnection Open(string mdbFilePath)
        {
            var connection = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={mdbFilePath};Persist Security Info=False;");
            connection.Open();
            return connection;
        }

        private static DataTable Query(OleDbConnection connection, string query)
        {
            using var adapter = new OleDbDataAdapter(query, connection);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        /// <summary>The rows of a CSV file, without its header.</summary>
        private static IEnumerable<string[]> ReadCsv(string path)
        {
            using TextFieldParser parser = new(path);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            parser.ReadLine();

            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields == null)
                    yield break;
                yield return fields;
            }
        }

        public static void ImportFileRefs(string filePath)
        {
            using var connection = Open(filePath);
            var tagTable = Query(connection, "Select [Name], [FFL References], [Group Display References], [Schematic References] From Tags");

            using var db = new TdcContext();
            var tagIds = db.TdcTags.ToList().GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Id);

            var id = 0;
            var fileRefs = new List<DbTdcFileRef>();
            foreach (DataRow row in tagTable.Rows)
            {
                if (row["Name"].ToString() is not string tagName || !tagIds.TryGetValue(tagName, out int tagId))
                    continue;

                void AddReferences(string column, Helper.FileType type)
                {
                    foreach (var reference in row[column].ToString()?.Split(',') ?? Array.Empty<string>())
                        if (!string.IsNullOrWhiteSpace(reference))
                            fileRefs.Add(new(id++, tagId, type, reference));
                }

                AddReferences("FFL References", Helper.FileType.FFL);
                AddReferences("Group Display References", Helper.FileType.Group);
                AddReferences("Schematic References", Helper.FileType.Schematic);
            }
            db.TdcFileRefs.AddRange(fileRefs);
            db.SaveChanges();
        }

        /// <summary>Writes every point in the database to one EB file at <see cref="EbFilePath"/>.</summary>
        public static void CreateEbFile(string mdbFilePath, string? cdsCsvFilePath = null, string? hgIOCsvFilePath = null)
        {
            var cds = new Dictionary<string, List<(string Parameter, string Value)>>();
            if (cdsCsvFilePath != null)
            {
                foreach (var fields in ReadCsv(cdsCsvFilePath))
                {
                    if (!cds.ContainsKey(fields[0]))
                        cds[fields[0]] = new();
                    cds[fields[0]].Add((fields[1], fields[2]));
                }
            }

            using var connection = Open(mdbFilePath);
            var tagTable = Query(connection, "Select * From Tags");

            var eb = new StringBuilder();
            var amTags = tagTable.AsEnumerable()
                .Where(x => x.Field<string>("Type")?[^2..] == "AM")
                .Select(x => new { Name = x.Field<string>("Name"), AM = x.Field<string>("Network")?.Replace("AM ", null) })
                .ToList();

            foreach (var pointType in tagTable.AsEnumerable().Select(x => x.Field<string>("Type")).Distinct())
            {
                if (pointType == null)
                    continue; // spare points have no table for their point type

                DataTable parameterTable;
                try
                {
                    parameterTable = Query(connection, $"Select * From [{pointType}] order by [name]");
                }
                catch (OleDbException)
                {
                    continue; // spare points have no table for their point type
                }

                var logicTable = pointType == "LOGICNIM" ? Query(connection, "SELECT * FROM LOGICNIM_GATES order by [name]") : null;

                for (int k = 0; k < parameterTable.Rows.Count; k++)
                {
                    var parameterRow = parameterTable.Rows[k];
                    var logicRow = logicTable?.Rows[k];

                    var isAnalogInput = pointType == "HLAI" || pointType == "LLMUX" || pointType == "STIM";
                    eb.AppendLine($"&T {(isAnalogInput ? "ANINNIM" : pointType)}");

                    string? tagName = null;
                    for (int i = 0; i < parameterTable.Columns.Count; i++)
                    {
                        var column = parameterTable.Columns[i];
                        if (column.ColumnName != "LogicBlockID")
                        {
                            var value = parameterRow[column].ToString();
                            if (i == 0)
                            {
                                tagName = value;
                                eb.AppendLine($"&N {value}");
                                if (pointType[^2..] == "AM")
                                    eb.AppendLine($"NTWKNUM-AM = \"{amTags.Where(x => x.Name == value).Select(x => x.AM).FirstOrDefault()}\"");
                            }
                            else if (!string.IsNullOrWhiteSpace(value))
                            {
                                eb.AppendLine($"{column.ColumnName} = \"{value}\"");
                            }
                        }

                        // Column 0 of the logic table is the name, already written above
                        if (i > 0 && logicRow != null && i < logicTable!.Columns.Count)
                        {
                            var logicValue = logicRow[i].ToString();
                            if (!string.IsNullOrWhiteSpace(logicValue))
                                eb.AppendLine($"{logicTable.Columns[i].ColumnName} = \"{logicValue}\"");
                        }
                    }

                    if (tagName != null && cds.TryGetValue(tagName, out var tagCds))
                        foreach (var (parameter, value) in tagCds)
                            eb.AppendLine($"{parameter} = \"{value}\"");
                }
            }

            if (hgIOCsvFilePath != null)
                foreach (var fields in ReadCsv(hgIOCsvFilePath))
                    AppendHgIoTag(eb, fields);

            File.WriteAllText(EbFilePath, eb.ToString());
        }

        /// <summary>Columns: tag, point type, highway, box in, slot in, box out, slot out.</summary>
        private static void AppendHgIoTag(StringBuilder eb, string[] fields)
        {
            eb.AppendLine($"&T {fields[1]}");
            eb.AppendLine($"&N {fields[0]}");
            eb.AppendLine("PTDESC = \"Not real TDC tag. Imported into Mapper\"");
            eb.AppendLine($"HWYNUM = \"{fields[2]}\"");

            if (!string.IsNullOrWhiteSpace(fields[3]))
            {
                eb.AppendLine($"BOXNUM = \"{fields[3]}\"");
                eb.AppendLine($"SLOTNUM = \"{fields[4]}\"");
            }
            else
            {
                eb.AppendLine($"OUTBOXNM = \"{fields[5]}\"");
                eb.AppendLine($"OUTSLTNM = \"{fields[6]}\"");
            }
        }
    }
}
