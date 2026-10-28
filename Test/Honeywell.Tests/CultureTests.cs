using Honeywell.Database;
using Honeywell.Parsers.TDC;
using Honeywell.Parsers.TDC.Logic;

namespace Honeywell.Tests
{
    /// <summary>EB values must parse the same on a PC set to a comma-decimal region (e.g. Germany).</summary>
    public class CultureTests
    {
        [Theory]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("en-US")]
        public void EbNumbers_ParseIndependentOfRegionalSettings(string culture)
        {
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));
            using var __ = new CultureScope(culture);

            Parser.ParseTdcFiles(Fixtures.EbFolder, null, null, null, null, createGraphs: true);

            using var db = new TdcContext();
            Assert.Equal("12.5", db.TdcParameters.Single(x => x.Name == "PVEUHI").Value);

            var logic = Assert.IsType<Logicnim>(Parser.TdcTags["LOG100"].Logic);
            Assert.Equal(3.75f, logic.NN1);
        }
    }
}
