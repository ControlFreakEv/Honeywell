var sourceFile = @"C:\Users\ebmiller\OneDrive - Hargrove Engineers + Constructors\Visual Studio Code\Honeywell\GUI's\Parser\bin\Debug\net7.0-windows\TDC.db";
var targetFile = @"C:\Users\ebmiller\OneDrive - Hargrove Engineers + Constructors\Visual Studio Code\Honeywell\GUI's\Mapper\bin\Debug\net7.0-windows\TDC.db";
var targetFile2 = @"C:\Users\ebmiller\OneDrive - Hargrove Engineers + Constructors\Visual Studio Code\Honeywell\GUI's\Mapper\bin\Release\net7.0-windows\TDC.db";

if (File.Exists($"{targetFile}-shm"))
    File.Delete($"{targetFile}-shm");
if (File.Exists($"{targetFile}-wal"))
    File.Delete($"{targetFile}-wal");


File.Copy(sourceFile, targetFile, true);
if (File.Exists($"{sourceFile}-shm"))
    File.Copy($"{sourceFile}-shm", targetFile, true);
if (File.Exists($"{sourceFile}-wal"))
    File.Copy($"{sourceFile}-wal", targetFile, true);

File.Copy(sourceFile, targetFile2, true);
if (File.Exists($"{sourceFile}-shm"))
    File.Copy($"{sourceFile}-shm", targetFile2, true);
if (File.Exists($"{sourceFile}-wal"))
    File.Copy($"{sourceFile}-wal", targetFile2, true);
