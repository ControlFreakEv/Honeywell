using Honeywell.HMIWeb;

namespace Honeywell.Tests
{
    /// <summary>Scanning a display and writing edits back, on a small hand-made display.</summary>
    public class DisplayTests
    {
        private const string Html = """
            <html><head><link id="HDXStylesheet" href="old.css"></head>
            <body id="Page">
            <div id="alpha" hdxproperties="HDXBINDINGID:b1;" title="old tip"></div>
            <div id="bravo" hdxproperties="HDXBINDINGID:b2;"></div>
            <div id="charlie" src=".\D1_files\Custom.sha" parameters="num?Size:10;Point?Tag:FIC30;"></div>
            <script id="s1">var a = "FIC20.SP"; var b = "OTHER.PV";</script>
            </body></html>
            """;

        private const string DataSource = """
            <dsd>
              <dataobject id="d1">
                <property name="PointRefPointName">FIC10</property>
                <property name="PointRefParamName">PV</property>
              </dataobject>
              <dataobject id="d2">
                <property name="CommaDelimitedPointNames">FIC20,FIC21</property>
                <property name="CommaDelimitedParameters">SP,OP</property>
              </dataobject>
              <dataobject id="ghost">
                <property name="PointRefPointName">GONE</property>
                <property name="PointRefParamName">PV</property>
              </dataobject>
            </dsd>
            """;

        private const string Bindings = """
            <bindings>
              <binding ID="b1"><dataobject objectid="d1" /></binding>
              <binding ID="b2"><dataobject objectid="d2" /></binding>
            </bindings>
            """;

        private static string CreateDisplay(TempFolder temp, string name = "D1")
        {
            File.WriteAllText(temp.File($"{name}.htm"), Html);
            var files = Directory.CreateDirectory(temp.File($"{name}_files")).FullName;
            File.WriteAllText(Path.Join(files, "DS_datasource1.dsd"), DataSource);
            File.WriteAllText(Path.Join(files, "Bindings.xml"), Bindings);
            return temp.Path;
        }

        private static string ReadDataSource(TempFolder temp) => File.ReadAllText(Path.Join(temp.File("D1_files"), "DS_datasource1.dsd"));

        [Fact]
        public void Scan_FindsDataTabAndScriptTabReferences()
        {
            using var temp = new TempFolder();

            var shapes = DisplayScanner.Scan(CreateDisplay(temp)).Shapes;

            var data = shapes.Single(x => x.DataObjectId == "d1");
            Assert.Equal(("FIC10", "PV", "alpha", Shape.ObjectType.DataTab), (data.Point, data.Parameter, data.ShapeName, data.DataType));

            var script = shapes.Where(x => x.DataObjectId == "d2").OrderBy(x => x.CsvIndex).ToList();
            Assert.Equal(new[] { "FIC20", "FIC21" }, script.Select(x => x.Point));
            Assert.Equal(new[] { "SP", "OP" }, script.Select(x => x.Parameter));
            Assert.All(script, x => Assert.Equal("bravo", x.ShapeName));
        }

        [Fact]
        public void Scan_MarksUnboundDataObjectsAsGhosts()
        {
            using var temp = new TempFolder();

            var ghost = DisplayScanner.Scan(CreateDisplay(temp)).Shapes.Single(x => x.DataObjectId == "ghost");

            Assert.Equal(DisplayScanner.GhostShapeName, ghost.ShapeName);
        }

        [Fact]
        public void Scan_FindsStylesheetTooltipAndCustomProperties()
        {
            using var temp = new TempFolder();

            var shapes = DisplayScanner.Scan(CreateDisplay(temp)).Shapes;

            Assert.Contains(shapes, x => x.ShapeName == "HDXStylesheet" && x.Css == "old.css");
            Assert.Contains(shapes, x => x.ShapeName == "alpha" && x.Tooltip == "old tip");
            var size = shapes.Single(x => x.CustomPropMiscName == "Size");
            Assert.Equal(("charlie", "10", "Custom.sha"), (size.ShapeName, size.MiscPropValue, size.ShapeSource));
        }

        [Fact]
        public void Scan_GivesSameIdToSameShape_AndDifferentIdsToOthers()
        {
            using var temp = new TempFolder();

            var shapes = DisplayScanner.Scan(CreateDisplay(temp)).Shapes.Where(x => x.ShapeName == "alpha" || x.ShapeName == "bravo").ToList();

            Assert.Single(shapes.Where(x => x.ShapeName == "alpha").Select(x => x.UniqueShapeId).Distinct());
            Assert.NotEqual(shapes.First(x => x.ShapeName == "alpha").UniqueShapeId, shapes.First(x => x.ShapeName == "bravo").UniqueShapeId);
            Assert.All(shapes, x => Assert.Equal($"D1.{x.ShapeName}", x.GfxAndShape));
        }

        [Fact]
        public void Scan_ReportsMissingFiles_AndKeepsScanningOthers()
        {
            using var temp = new TempFolder();
            CreateDisplay(temp);
            File.WriteAllText(temp.File("Broken.htm"), "<html></html>");

            var scan = DisplayScanner.Scan(temp.Path);

            Assert.Contains(scan.Problems, x => x.Contains("Broken_files"));
            Assert.Contains(scan.Shapes, x => x.Graphic == "D1");
            Assert.DoesNotContain(scan.Shapes, x => x.Graphic == "Broken");
        }

        [Fact]
        public void Replace_DataTabPointAndParameter()
        {
            using var temp = new TempFolder();
            var folder = CreateDisplay(temp);
            var shapes = DisplayScanner.Scan(folder).Shapes;
            var shape = shapes.Single(x => x.DataObjectId == "d1");
            shape.NewPoint = "FIC99";
            shape.NewParameter = "OP";

            DisplayReplacer.ReplaceReferences(folder, shapes, deleteGhostReferences: false);

            var dsd = ReadDataSource(temp);
            Assert.Contains(">FIC99<", dsd);
            Assert.Contains(">OP<", dsd);
            Assert.DoesNotContain(">FIC10<", dsd);
            Assert.Contains(">GONE<", dsd);
        }

        [Fact]
        public void Replace_ScriptTabUpdatesListAndScriptText()
        {
            using var temp = new TempFolder();
            var folder = CreateDisplay(temp);
            var shapes = DisplayScanner.Scan(folder).Shapes;
            shapes.Single(x => x.Point == "FIC20").NewPoint = "FIC88";

            DisplayReplacer.ReplaceReferences(folder, shapes, deleteGhostReferences: false);

            Assert.Contains("FIC88,FIC21", ReadDataSource(temp));
            var html = File.ReadAllText(temp.File("D1.htm"));
            Assert.Contains("\"FIC88.SP\"", html);
            Assert.Contains("\"OTHER.PV\"", html);
            Assert.DoesNotContain("\"FIC20.SP\"", html);
        }

        [Fact]
        public void Replace_TooltipStylesheetAndCustomProperty()
        {
            using var temp = new TempFolder();
            var folder = CreateDisplay(temp);
            var shapes = DisplayScanner.Scan(folder).Shapes;
            shapes.Single(x => x.Tooltip == "old tip").NewTooltip = "new tip";
            shapes.Single(x => x.Css == "old.css").NewCss = "new.css";
            shapes.Single(x => x.CustomPropMiscName == "Size").NewMiscPropValue = "20";

            DisplayReplacer.ReplaceReferences(folder, shapes, deleteGhostReferences: false);

            var html = File.ReadAllText(temp.File("D1.htm"));
            Assert.Contains("title=\"new tip\"", html);
            Assert.Contains("href=\"new.css\"", html);
            Assert.Contains("?Size:20;", html);
            Assert.Contains("?Tag:FIC30;", html);
        }

        [Theory]
        [InlineData("$null", "")]
        [InlineData("$NULL", "")]
        [InlineData(@"\$null", "$null")]
        public void Replace_DeleteKeywords(string typed, string expected)
        {
            using var temp = new TempFolder();
            var folder = CreateDisplay(temp);
            var shapes = DisplayScanner.Scan(folder).Shapes;
            shapes.Single(x => x.Tooltip == "old tip").NewTooltip = typed;

            DisplayReplacer.ReplaceReferences(folder, shapes, deleteGhostReferences: false);

            Assert.Contains($"title=\"{expected}\"", File.ReadAllText(temp.File("D1.htm")));
        }

        [Fact]
        public void Replace_RemovesGhostsOnlyWhenAsked()
        {
            using var temp = new TempFolder();
            var folder = CreateDisplay(temp);
            var shapes = DisplayScanner.Scan(folder).Shapes;

            Assert.Throws<DisplayEditException>(() => DisplayReplacer.ReplaceReferences(folder, shapes, deleteGhostReferences: false));
            Assert.Contains(">GONE<", ReadDataSource(temp));

            DisplayReplacer.ReplaceReferences(folder, shapes, deleteGhostReferences: true);
            Assert.DoesNotContain(">GONE<", ReadDataSource(temp));
            Assert.Contains(">FIC10<", ReadDataSource(temp));
        }

        [Fact]
        public void Replace_MissingDisplay_IsReported()
        {
            using var temp = new TempFolder();
            var shape = new Shape { Graphic = "Nope", ShapeName = "x", NewTooltip = "y" };

            Assert.Throws<DisplayEditException>(() => DisplayReplacer.ReplaceReferences(temp.Path, new[] { shape }, false));
        }

        [Fact]
        public void Script_LeavesTextAloneWhenNothingMatched()
        {
            var script = new Script("var a = \"X.Y\";");
            script.UpdateReplacements("\"Q.R\"", "\"S.T\"");

            Assert.Equal("var a = \"X.Y\";", script.UpdateScript());
        }

        [Fact]
        public void Script_ReplacesOnlyMatchingQuotedNames()
        {
            var script = new Script("a(\"A.B\", \"C.D\", \"a.b\");");
            script.UpdateReplacements("\"A.B\"", "\"N.M\"");

            Assert.Equal("a(\"N.M\", \"C.D\", \"N.M\");", script.UpdateScript());
        }
    }
}
