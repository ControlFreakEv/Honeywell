using Honeywell.HMIWeb;

namespace Honeywell.Tests
{
    /// <summary>GFX Replace renames points/parameters inside display markup; it must not hit longer names.</summary>
    public class ReplaceWholeNameTests
    {
        [Theory]
        [InlineData("FIC10.PV", "FIC10", "FIC20", "FIC20.PV")]
        [InlineData("FIC101.PV", "FIC10", "FIC20", "FIC101.PV")]
        [InlineData("XFIC10.PV", "FIC10", "FIC20", "XFIC10.PV")]
        [InlineData("FIC10_A FIC10", "FIC10", "FIC20", "FIC10_A FIC20")]
        [InlineData("\"FIC10\",\"FIC10.SP\"", "FIC10", "FIC20", "\"FIC20\",\"FIC20.SP\"")]
        [InlineData("fic10.pv", "FIC10", "FIC20", "FIC20.pv")]
        [InlineData("A.PV A.PVEUHI", "PV", "SP", "A.SP A.PVEUHI")]
        public void ReplacesOnlyWholeNames(string text, string oldName, string newName, string expected)
        {
            Assert.Equal(expected, text.ReplaceWholeName(oldName, newName));
        }

        [Fact]
        public void TreatsSpecialCharactersLiterally()
        {
            // '$' in a new HG name must not be read as a regex substitution, and '.' in the old name must not match any char
            Assert.Equal("$HY01B09.LB(001)", "OLD.TAG".ReplaceWholeName("OLD.TAG", "$HY01B09.LB(001)"));
            Assert.Equal("OLDXTAG", "OLDXTAG".ReplaceWholeName("OLD.TAG", "NEW"));
        }
    }
}
