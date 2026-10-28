namespace CopyDB
{
    public static class DbCopier
    {
        private static readonly string[] SideFileSuffixes = { "-shm", "-wal" };

        /// <summary>
        /// Copies a SQLite database and its -wal/-shm side files, removing stale side files at the target.
        /// </summary>
        public static void Copy(string sourceFile, string targetFile)
        {
            // Stale side files from an older target would be replayed against the new database
            foreach (var suffix in SideFileSuffixes)
                if (File.Exists(targetFile + suffix))
                    File.Delete(targetFile + suffix);

            File.Copy(sourceFile, targetFile, true);
            foreach (var suffix in SideFileSuffixes)
                if (File.Exists(sourceFile + suffix))
                    File.Copy(sourceFile + suffix, targetFile + suffix, true);
        }
    }
}
