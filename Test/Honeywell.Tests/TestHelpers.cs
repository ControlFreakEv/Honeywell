using System.Globalization;
using Honeywell.Database;
using Microsoft.Data.Sqlite;

// The parser and TdcContext keep their state in statics, so tests can't run in parallel
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace Honeywell.Tests
{
    internal static class Fixtures
    {
        public static string Root => Path.Join(AppContext.BaseDirectory, "Fixtures");
        public static string EbFolder => Path.Join(Root, "EB");
        public static string ClFolder => Path.Join(Root, "CL");
        public static string JsFolder => Path.Join(Root, "JS");
    }

    /// <summary>A scratch folder deleted when the test ends.</summary>
    internal sealed class TempFolder : IDisposable
    {
        public string Path { get; } = System.IO.Path.Join(System.IO.Path.GetTempPath(), $"honeywell-tests-{Guid.NewGuid():N}");

        public TempFolder() => Directory.CreateDirectory(Path);

        public string File(string name) => System.IO.Path.Join(Path, name);

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            try { Directory.Delete(Path, true); } catch (IOException) { }
        }
    }

    /// <summary>Points TdcContext at a database file for the duration of a test.</summary>
    internal sealed class TestDatabase : IDisposable
    {
        private readonly string? previousConnectionString = TdcContext.ConnectionString;
        private readonly bool previousServer = TdcContext.Server;

        public TestDatabase(string path)
        {
            TdcContext.Server = false;
            TdcContext.ConnectionString = path;
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            TdcContext.ConnectionString = previousConnectionString;
            TdcContext.Server = previousServer;
        }
    }

    /// <summary>Runs code under a culture that uses ',' as the decimal separator.</summary>
    internal sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo previous = CultureInfo.CurrentCulture;

        public CultureScope(string name) => CultureInfo.CurrentCulture = new CultureInfo(name);

        public void Dispose() => CultureInfo.CurrentCulture = previous;
    }
}
