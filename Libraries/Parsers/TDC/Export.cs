using System.Data;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using Honeywell.TDC;

namespace Honeywell.Parsers.TDC
{
    public class Export
    {
        public static void ExportTdcToXlsx(List<TdcTag> tdcTags, string savePath)
        {
            IWorkbook workbook = new XSSFWorkbook();

            //create TDC tag worksheet
            int tagCol = 0;
            int tagRow = 0;
            ISheet tagWs = workbook.CreateSheet("TDC Tags");
            IRow row = tagWs.CreateRow(tagRow++);
            row.CreateCell(tagCol++).SetCellValue("Module");
            row.CreateCell(tagCol++).SetCellValue("TDC Tag");
            row.CreateCell(tagCol++).SetCellValue("Point Type");
            row.CreateCell(tagCol++).SetCellValue("Module Description");
            row.CreateCell(tagCol++).SetCellValue("TDC Tag Description");
            row.CreateCell(tagCol++).SetCellValue("EB File");
            row.CreateCell(tagCol++).SetCellValue("NTWKNUM");
            row.CreateCell(tagCol++).SetCellValue("NODENUM");
            row.CreateCell(tagCol++).SetCellValue("SLOTNUM");
            row.CreateCell(tagCol++).SetCellValue("MODNUM");
            row.CreateCell(tagCol++).SetCellValue("NODETYP");
            row.CreateCell(tagCol++).SetCellValue("PVALGID");
            row.CreateCell(tagCol++).SetCellValue("CTLALGID");

            //create parameter worksheet
            int parameterCol = 0;
            int parameterRow = 0;
            ISheet paramWs = workbook.CreateSheet("Parameters");
            row = paramWs.CreateRow(parameterRow++);
            row.CreateCell(parameterCol++).SetCellValue("TDC Tag");
            row.CreateCell(parameterCol++).SetCellValue("Parameter");
            row.CreateCell(parameterCol++).SetCellValue("Value");

            for (int i = 0; i < tdcTags.Count; i++)
            {
                TdcTag? tag = tdcTags[i];
                tagCol = 0;
                row = tagWs.CreateRow(tagRow++);

                row.CreateCell(tagCol++).SetCellValue(tag.Module?.Name);
                row.CreateCell(tagCol++).SetCellValue(tag.Name);
                row.CreateCell(tagCol++).SetCellValue(tag.PointType);
                row.CreateCell(tagCol++).SetCellValue(tag.Module?.Desc);
                row.CreateCell(tagCol++).SetCellValue(tag.Desc);
                row.CreateCell(tagCol++).SetCellValue(tag.EbFile);
                row.CreateCell(tagCol++).SetCellValue(tag.NTWKNUM);
                row.CreateCell(tagCol++).SetCellValue(tag.NODENUM);
                row.CreateCell(tagCol++).SetCellValue(tag.SLOTNUM);
                row.CreateCell(tagCol++).SetCellValue(tag.MODNUM);
                row.CreateCell(tagCol++).SetCellValue(tag.NODETYP);
                row.CreateCell(tagCol++).SetCellValue(tag.PVALGID);
                row.CreateCell(tagCol++).SetCellValue(tag.CTLALGID);

                foreach (var parameter in tag.Params)
                {
                    if (parameter != null)
                    {
                        parameterCol = 0;
                        row = paramWs.CreateRow(parameterRow++);

                        row.CreateCell(parameterCol++).SetCellValue(parameter.ParentTag.Name);
                        row.CreateCell(parameterCol++).SetCellValue(parameter.Name);
                        row.CreateCell(parameterCol++).SetCellValue(parameter.Value);
                    }
                }
            }

            //format sheets
            tagWs.CreateFreezePane(0, 1, 0, 1);
            tagWs.SetAutoFilter(new CellRangeAddress(0, --tagRow, 0, --tagCol));
            for (int i = 0; i <= tagCol; i++)
                tagWs.AutoSizeColumn(i);

            paramWs.CreateFreezePane(0, 1, 0, 1);
            paramWs.SetAutoFilter(new CellRangeAddress(0, --parameterRow, 0, --parameterCol));
            for (int i = 0; i <= parameterCol; i++)
                paramWs.AutoSizeColumn(i);

            //save file
            using FileStream stream = new(savePath, FileMode.Create, FileAccess.Write);
            workbook.Write(stream);
        }

        public static void ExportGroups(List<TdcGroup> tdcGroups, string savePath)
        {
            if (tdcGroups.Count == 0)
                return;

            IWorkbook workbook = new XSSFWorkbook();

            //create TDC group worksheet
            int groupCol = 0;
            int groupRow = 0;
            ISheet groupWs = workbook.CreateSheet("TDC Groups");
            IRow row = groupWs.CreateRow(groupRow++);
            row.CreateCell(groupCol++).SetCellValue("EbFile");
            row.CreateCell(groupCol++).SetCellValue("OENTNUMP");
            row.CreateCell(groupCol++).SetCellValue("TITLE");

            for (int i = 0; i < tdcGroups[0].EXTIDLST.Length; i++)
                row.CreateCell(groupCol++).SetCellValue($"EXTIDLST[{i + TdcGroup.OffsetEXTIDLST}]");

            for (int i = 0; i < tdcGroups[0].TRNDSET.Length; i++)
                row.CreateCell(groupCol++).SetCellValue($"TRNDSET[{i + TdcGroup.OffsetTRNDSET}]");

            row.CreateCell(groupCol++).SetCellValue("TRNDBASE");

            for (int i = 0; i < tdcGroups[0].SCREENUM.Length; i++)
                row.CreateCell(groupCol++).SetCellValue($"SCREENUM[{+TdcGroup.OffsetSCREENUM}]");

            for (int i = 0; i < tdcGroups[0].DISPID.Length; i++)
                row.CreateCell(groupCol++).SetCellValue($"DISPID[{+TdcGroup.OffsetDISPID}]");

            //populate data
            foreach (TdcGroup group in tdcGroups)
            {
                groupCol = 0;
                row = groupWs.CreateRow(groupRow++);

                row.CreateCell(groupCol++).SetCellValue(group.EbFile);
                row.CreateCell(groupCol++).SetCellValue(group.OENTNUMP);
                row.CreateCell(groupCol++).SetCellValue(group.TITLE);

                for (int i = 0; i < tdcGroups[0].EXTIDLST.Length; i++)
                    row.CreateCell(groupCol++).SetCellValue(group.EXTIDLST[i]);

                for (int i = 0; i < tdcGroups[0].TRNDSET.Length; i++)
                    row.CreateCell(groupCol++).SetCellValue(group.TRNDSET[i]);

                row.CreateCell(groupCol++).SetCellValue(group.TRNDBASE);

                for (int i = 0; i < tdcGroups[0].SCREENUM.Length; i++)
                    row.CreateCell(groupCol++).SetCellValue(group.SCREENUM[i]);

                for (int i = 0; i < tdcGroups[0].DISPID.Length; i++)
                    row.CreateCell(groupCol++).SetCellValue(group.DISPID[i]);
            }

            //format sheets
            groupWs.CreateFreezePane(0, 1, 0, 1);
            groupWs.SetAutoFilter(new CellRangeAddress(0, --groupRow, 0, --groupCol));
            //for (int i = 0; i <= groupCol; i++)
            //    groupWs.AutoSizeColumn(i);

            //save file
            using FileStream stream = new(savePath, FileMode.Create, FileAccess.Write);
            workbook.Write(stream);
        }
    }
}