using Honeywell.Parsers.TDC;

namespace Honeywell.Tests
{
    /// <summary>EB file handling that depends on more than one file or record.</summary>
    public class EbParsingTests
    {
        private static void Parse(string folder, string? clSlots = null) =>
            Parser.ParseTdcFiles(folder, null, null, null, clSlots, createGraphs: false);

        [Fact]
        public void SourceReference_ResolvesAcrossFiles_RegardlessOfOrder()
        {
            // A.eb is read first and points at a tag defined in B.eb
            using var temp = new TempFolder();
            File.WriteAllText(temp.File("A.eb"), "&T LOGICNIM\n&N LOG1\nPTDESC = \"first\"\nLISRC(1) = LOG2.LISRC(2)\n");
            File.WriteAllText(temp.File("B.eb"), "&T LOGICNIM\n&N LOG2\nPTDESC = \"second\"\nLISRC(2) = FIC1.PVHIFL\n&T REGAM\n&N FIC1\nPTDESC = \"third\"\n");

            Parse(temp.Path);

            Assert.Equal("FIC1.PVHIFL", Parser.TdcTags["LOG1"].Params.Single(x => x.Name == "LISRC(1)").Value);
        }

        [Fact]
        public void BlankValueIsFilledFromLaterFile_ButFirstValueWins()
        {
            using var temp = new TempFolder();
            File.WriteAllText(temp.File("A.eb"), "&T REGAM\n&N FIC1\nPTDESC = \"\"\nPVEUHI = 10.50\n");
            File.WriteAllText(temp.File("B.eb"), "&T REGAM\n&N FIC1\nPTDESC = \"desc\"\nPVEUHI = 99\n");

            Parse(temp.Path);

            var tag = Parser.TdcTags["FIC1"];
            Assert.Equal("desc", tag.Desc);
            Assert.Equal("10.5", tag.Params.Single(x => x.Name == "PVEUHI").Value);
            Assert.Equal("A.eb\nB.eb", tag.EbFile);
        }

        [Theory]
        [InlineData("-------")]
        [InlineData("--.--")]
        [InlineData("_______")]
        [InlineData("__.__")]
        public void PlaceholderValuesBecomeNull(string placeholder)
        {
            using var temp = new TempFolder();
            File.WriteAllText(temp.File("A.eb"), $"&T REGAM\n&N FIC1\nPTDESC = \"d\"\nPVEUHI = {placeholder}\n");

            Parse(temp.Path);

            Assert.Null(Parser.TdcTags["FIC1"].Params.Single(x => x.Name == "PVEUHI").Value);
        }

        [Fact]
        public void LeadingZerosInIndexesAreRemoved_AndKnownParametersMapped()
        {
            using var temp = new TempFolder();
            File.WriteAllText(temp.File("A.eb"), "&T REGAM\n&N FIC1\nNN(001) = 5\nNN(0) = 6\nNODENUM = 12\nPKGNAME(1) = \"Pkg\"\nBLKNAME(1) = \"blk\"\n");

            Parse(temp.Path);

            var tag = Parser.TdcTags["FIC1"];
            Assert.Contains(tag.Params, x => x.Name == "NN(1)" && x.Value == "5");
            Assert.Contains(tag.Params, x => x.Name == "NN(0)" && x.Value == "6");
            Assert.Equal("12", tag.NODENUM);
            Assert.Equal(new[] { "PKG" }, tag.Packages);
            Assert.Equal(new[] { "BLK" }, tag.Blocks);
        }

        [Fact]
        public void ClSlotsAddMissingBlocksOnce()
        {
            using var temp = new TempFolder();
            File.WriteAllText(temp.File("A.eb"), "&T REGAM\n&N FIC1\nBLKNAME(0) = \"OLD\"\n");
            File.WriteAllText(temp.File("B.eb"), "&T REGAM\n&N FIC2\nPTDESC = \"d\"\n");
            var slots = temp.File("slots.txt");
            File.WriteAllLines(slots, new[] { "MEDIA x FIC1 NEW", "--------- x x x", "x x FIC1 NEW", "x x FIC1 OLD", "x x FIC2 ONLY" });

            Parse(temp.Path, slots);

            Assert.Equal(new[] { "OLD", "NEW" }, Parser.TdcTags["FIC1"].Blocks);
            Assert.Contains(Parser.TdcTags["FIC1"].Params, x => x.Name == "BLKNAME(1)" && x.Value == "NEW");
            Assert.Equal(new[] { "ONLY" }, Parser.TdcTags["FIC2"].Blocks);
        }

        [Fact]
        public void GroupExport_WritesOneRowPerGroup()
        {
            using var temp = new TempFolder();
            File.WriteAllText(temp.File("G.eb"), "&T GROUP\n&N G1\nOENTNUMP = 7\nTITLE = \"Title\"\nTRNDBASE = 2\n&T GROUP\n&N G2\nOENTNUMP = 8\nTITLE = \"Other\"\n");
            var previous = Parser.GroupFilePath;
            Parser.GroupFilePath = temp.File("groups.xlsx");
            try
            {
                Parser.ExportGroups(temp.Path);

                using var workbook = new ClosedXML.Excel.XLWorkbook(Parser.GroupFilePath);
                var sheet = workbook.Worksheet("TDC Groups");
                Assert.Equal(new[] { "Title", "Other" }, Honeywell.Excel.Xlsx.DataRows(sheet).Select(row => Honeywell.Excel.Xlsx.TextOrNull(row.Cell(3))));
            }
            finally
            {
                Parser.GroupFilePath = previous;
            }
        }
    }
}
