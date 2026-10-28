using Honeywell.Database;
using Honeywell.Parsers.TDC;

namespace Honeywell.Tests
{
    /// <summary>Re-parsing TDC files rebuilds the database; data entered in Mapper must survive it.</summary>
    public class ReparseTests
    {
        private static void Parse() => Parser.ParseTdcFiles(Fixtures.EbFolder, Fixtures.ClFolder, null, null, null, createGraphs: true);

        private static void AddUserData()
        {
            using var db = new TdcContext();
            db.Projects.Add(new DbProject { Id = 1, Name = "P1" });
            db.ConfigTemplates.Add(new DbConfigTemplates { Id = 1, TypicalName = "T1" });
            db.ExperionFunctionBlocks.Add(new DbExperionFunctionBlock { Id = 1, ConfigTemplateId = 1, Name = "FB", NewName = "FB2", Type = "PID" });
            db.ExperionParameters.Add(new DbExperionParameter { Id = 1, FunctionBlockId = 1, Name = "P", Value = "V" });

            var tag = db.TdcTags.Single(x => x.Name == "FIC100");
            tag.ProjectId = 1;
            tag.Notes = "tag note";

            var cl = db.TdcCLs.Single();
            cl.Notes = "cl note";
            cl.Content += "\n--edited";
            cl.Indicators = "1;0;5";

            var parsedModule = db.TdcModules.Single(x => x.Name == "FIC100");
            parsedModule.Notes = "parsed module note";
            parsedModule.Alarms = "alarms";
            parsedModule.ConfigTemplateId = 1;

            // A module made by hand in the graph view, with a node moved into it
            var userModule = new DbTdcModule { Id = db.TdcModules.Max(x => x.Id) + 1, Name = "USERMOD", Interlocks = "interlocks" };
            db.TdcModules.Add(userModule);
            db.TdcNodes.Single(x => x.NodeId == "LOG100").ModuleId = userModule.Id;

            db.SaveChanges();
        }

        [Fact]
        public void Reparse_KeepsUserEnteredData()
        {
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));

            Parse();
            AddUserData();
            Parse();

            using var db = new TdcContext();
            Assert.Equal("P1", Assert.Single(db.Projects).Name);
            Assert.Equal("T1", Assert.Single(db.ConfigTemplates).TypicalName);
            Assert.Equal("FB2", Assert.Single(db.ExperionFunctionBlocks).NewName);
            Assert.Equal("V", Assert.Single(db.ExperionParameters).Value);

            var tag = db.TdcTags.Single(x => x.Name == "FIC100");
            Assert.Equal(1, tag.ProjectId);
            Assert.Equal("tag note", tag.Notes);

            var cl = db.TdcCLs.Single();
            Assert.Equal("cl note", cl.Notes);
            Assert.EndsWith("--edited", cl.Content);
            Assert.Equal("1;0;5", cl.Indicators);

            var parsedModule = db.TdcModules.Single(x => x.Name == "FIC100");
            Assert.Equal("parsed module note", parsedModule.Notes);
            Assert.Equal("alarms", parsedModule.Alarms);
            Assert.Equal(1, parsedModule.ConfigTemplateId);

            var node = db.TdcNodes.Single(x => x.NodeId == "LOG100");
            Assert.Equal("USERMOD", node.Module?.Name);
            Assert.Equal("interlocks", node.Module?.Interlocks);
        }

        [Fact]
        public void Reparse_DoesNotDuplicateParsedData()
        {
            using var temp = new TempFolder();
            using var _ = new TestDatabase(temp.File("TDC.db"));

            Parse();
            int tags, parameters, modules;
            using (var db = new TdcContext())
                (tags, parameters, modules) = (db.TdcTags.Count(), db.TdcParameters.Count(), db.TdcModules.Count());

            Parse();
            using var db2 = new TdcContext();
            Assert.Equal(tags, db2.TdcTags.Count());
            Assert.Equal(parameters, db2.TdcParameters.Count());
            Assert.Equal(modules, db2.TdcModules.Count());
        }

        [Fact]
        public void Reparse_BacksUpExistingDatabase()
        {
            using var temp = new TempFolder();
            var dbPath = temp.File("TDC.db");
            using var _ = new TestDatabase(dbPath);

            Parse();
            Parse();

            Assert.True(File.Exists(dbPath + ".bak"));
        }

        [Fact]
        public void Reparse_LeavesUnreadableDatabaseUntouched()
        {
            using var temp = new TempFolder();
            var dbPath = temp.File("TDC.db");

            // A database from some other schema: the snapshot can't read it
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Projects (Id INTEGER); INSERT INTO Projects VALUES (42);";
                command.ExecuteNonQuery();
            }
            var before = File.ReadAllBytes(dbPath);

            using var _ = new TestDatabase(dbPath);
            Assert.Throws<InvalidOperationException>(Parse);
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            Assert.Equal(before, File.ReadAllBytes(dbPath));
        }
    }
}
