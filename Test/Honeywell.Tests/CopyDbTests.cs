using CopyDB;

namespace Honeywell.Tests
{
    public class CopyDbTests
    {
        [Fact]
        public void CopiesSideFilesNextToTarget()
        {
            using var temp = new TempFolder();
            var source = temp.File("source.db");
            var target = temp.File("target.db");
            File.WriteAllText(source, "DB");
            File.WriteAllText(source + "-wal", "WAL");
            File.WriteAllText(source + "-shm", "SHM");

            DbCopier.Copy(source, target);

            Assert.Equal("DB", File.ReadAllText(target));
            Assert.Equal("WAL", File.ReadAllText(target + "-wal"));
            Assert.Equal("SHM", File.ReadAllText(target + "-shm"));
        }

        [Fact]
        public void RemovesStaleTargetSideFiles()
        {
            using var temp = new TempFolder();
            var source = temp.File("source.db");
            var target = temp.File("target.db");
            File.WriteAllText(source, "DB");
            File.WriteAllText(target, "OLD");
            File.WriteAllText(target + "-wal", "STALE");

            DbCopier.Copy(source, target);

            Assert.Equal("DB", File.ReadAllText(target));
            Assert.False(File.Exists(target + "-wal"));
        }
    }
}
