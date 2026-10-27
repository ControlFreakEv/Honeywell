// Copies a SQLite database, with its -wal/-shm side files, to one or more targets.
// Usage: CopyDB <source.db> <target.db> [<target2.db> ...]
// Defaults to copying the TDC Parser output (Desktop\TDC.db) next to Mapper's build output.

var desktopDb = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TDC.db");
var sourceFile = args.Length > 0 ? args[0] : desktopDb;
var targetFiles = args.Length > 1 ? args[1..] : DefaultTargets();

if (!File.Exists(sourceFile))
{
    Console.Error.WriteLine($"Source database not found: {sourceFile}");
    return 1;
}

if (targetFiles.Length == 0)
{
    Console.Error.WriteLine("No target given and Mapper's build output folder wasn't found.");
    return 1;
}

foreach (var targetFile in targetFiles)
{
    // Stale side files from an older target would be replayed against the new database
    foreach (var suffix in new[] { "-shm", "-wal" })
        if (File.Exists(targetFile + suffix))
            File.Delete(targetFile + suffix);

    File.Copy(sourceFile, targetFile, true);
    foreach (var suffix in new[] { "-shm", "-wal" })
        if (File.Exists(sourceFile + suffix))
            File.Copy(sourceFile + suffix, targetFile + suffix, true);

    Console.WriteLine($"Copied {sourceFile} -> {targetFile}");
}

return 0;

static string[] DefaultTargets()
{
    // Walk up from this tool's output folder to the solution root
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !File.Exists(Path.Join(dir.FullName, "Honeywell.sln")))
        dir = dir.Parent;
    if (dir == null)
        return Array.Empty<string>();

    var mapperBin = Path.Join(dir.FullName, "GUI's", "Mapper", "bin");
    if (!Directory.Exists(mapperBin))
        return Array.Empty<string>();

    return Directory.GetDirectories(mapperBin, "net*-windows", SearchOption.AllDirectories)
        .Select(x => Path.Join(x, "TDC.db"))
        .ToArray();
}
