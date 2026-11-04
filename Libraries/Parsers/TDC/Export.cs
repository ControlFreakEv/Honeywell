using ClosedXML.Excel;
using Honeywell.Excel;
using Honeywell.TDC;

namespace Honeywell.Parsers.TDC
{
    public class Export
    {
        public static void ExportTdcToXlsx(List<TdcTag> tdcTags, string savePath)
        {
            using var workbook = new XLWorkbook();

            var tagSheet = workbook.Worksheets.Add("TDC Tags");
            tagSheet.WriteHeaders(new[]
            {
                "Module", "TDC Tag", "Point Type", "Module Description", "TDC Tag Description", "EB File",
                "NTWKNUM", "NODENUM", "SLOTNUM", "MODNUM", "NODETYP", "PVALGID", "CTLALGID",
            });

            var parameterSheet = workbook.Worksheets.Add("Parameters");
            parameterSheet.WriteHeaders(new[] { "TDC Tag", "Parameter", "Value" });

            int tagRow = 2;
            int parameterRow = 2;
            foreach (var tag in tdcTags)
            {
                tagSheet.WriteRow(tagRow++, new XLCellValue[]
                {
                    tag.Module?.Name, tag.Name, tag.PointType, tag.Module?.Desc, tag.Desc, tag.EbFile,
                    tag.NTWKNUM, tag.NODENUM, tag.SLOTNUM, tag.MODNUM, tag.NODETYP, tag.PVALGID, tag.CTLALGID,
                });

                foreach (var parameter in tag.Params)
                    if (parameter != null)
                        parameterSheet.WriteRow(parameterRow++, new XLCellValue[] { parameter.ParentTag.Name, parameter.Name, parameter.Value });
            }

            foreach (var sheet in new[] { tagSheet, parameterSheet })
            {
                sheet.FreezeHeaderAndFilter();
                sheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(savePath);
        }

        public static void ExportGroups(List<TdcGroup> tdcGroups, string savePath)
        {
            if (tdcGroups.Count == 0)
                return;

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("TDC Groups");

            // Every group has the same array lengths, so the first one sizes the columns
            var first = tdcGroups[0];
            var headers = new List<string> { "EbFile", "OENTNUMP", "TITLE" };
            headers.AddRange(IndexedHeaders("EXTIDLST", first.EXTIDLST.Length, TdcGroup.OffsetEXTIDLST));
            headers.AddRange(IndexedHeaders("TRNDSET", first.TRNDSET.Length, TdcGroup.OffsetTRNDSET));
            headers.Add("TRNDBASE");
            headers.AddRange(IndexedHeaders("SCREENUM", first.SCREENUM.Length, TdcGroup.OffsetSCREENUM));
            headers.AddRange(IndexedHeaders("DISPID", first.DISPID.Length, TdcGroup.OffsetDISPID));
            sheet.WriteHeaders(headers);

            int rowNumber = 2;
            foreach (var group in tdcGroups)
            {
                var values = new List<XLCellValue> { group.EbFile, group.OENTNUMP, group.TITLE };
                values.AddRange(group.EXTIDLST.Select(value => (XLCellValue)value));
                values.AddRange(group.TRNDSET.Select(value => (XLCellValue)value));
                values.Add(group.TRNDBASE);
                values.AddRange(group.SCREENUM.Select(value => (XLCellValue)value));
                values.AddRange(group.DISPID.Select(value => (XLCellValue)value));
                sheet.WriteRow(rowNumber++, values);
            }

            sheet.FreezeHeaderAndFilter();
            workbook.SaveAs(savePath);
        }

        private static IEnumerable<string> IndexedHeaders(string name, int count, int offset) =>
            Enumerable.Range(offset, count).Select(index => $"{name}[{index}]");
    }
}
