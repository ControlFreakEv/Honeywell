using Honeywell.Parsers.TDC;
using Honeywell.Parsers.TDC.Logic;
using static Honeywell.Parsers.TDC.Logic.LogicHgTypes;

namespace Honeywell.Tests
{
    /// <summary>Highway gateway logic (.js files).</summary>
    public class HgGateTests
    {
        private const string FileName = "LB010901.js";

        private static List<LogicHgGate> ParseFixture(int times = 1)
        {
            Parser.TdcTags.Clear();
            Parser.TdcParameters.Clear();

            var lines = File.ReadAllLines(Path.Join(Fixtures.JsFolder, FileName));
            var gates = new List<LogicHgGate>();
            for (int i = 0; i < times; i++)
                Parser.JSParseContent(lines, FileName, gates);
            return gates;
        }

        private static LogicHgGate Gate(List<LogicHgGate> gates, int index) => gates.Single(x => x.GateIndex == index && !x.SpecialGate);

        [Fact]
        public void StarOutput_IsMomentary()
        {
            var gates = ParseFixture();

            Assert.True(Gate(gates, 1).Output1Momentary);
            Assert.Single(gates, x => x.GateType == LogicHGBlockTypes.MOMENTARY && x.GateIndex == 1);
        }

        [Fact]
        public void PlainOutput_IsNotMomentary()
        {
            var gates = ParseFixture();

            Assert.False(Gate(gates, 2).Output1Momentary);
            Assert.DoesNotContain(gates, x => x.GateType == LogicHGBlockTypes.MOMENTARY && x.GateIndex == 2);
        }

        [Fact]
        public void DelayTime_StoredAsTime()
        {
            var gate = Gate(ParseFixture(), 3);

            Assert.Equal("10", gate.Time);
            Assert.Contains(gate.TdcTag.Params, x => x.Name == "Time" && x.Value == "10");
            Assert.DoesNotContain(gate.TdcTag.Params, x => x.Name == "Output2");
        }

        [Fact]
        public void ShortAndCommentLines_AreSkipped()
        {
            // The fixture contains "X", "-" and a "--" comment line
            var gates = ParseFixture();

            Assert.Equal(new[] { 1, 2, 3 }, gates.Where(x => !x.SpecialGate).Select(x => x.GateIndex).Order());
        }

        [Fact]
        public void SameFileTwice_DoesNotThrowOrDuplicate()
        {
            // e.g. a backup copy of the .js file in a subfolder with "include subdirectories" checked
            var gates = ParseFixture(times: 2);

            Assert.Equal(3, gates.Count(x => !x.SpecialGate));
        }
    }
}
