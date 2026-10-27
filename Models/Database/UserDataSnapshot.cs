using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Honeywell.Database
{
    /// <summary>
    /// Data entered in Mapper (by hand or by Excel import) that a TDC re-parse must not destroy.
    /// The parser rebuilds the database and regenerates every Id, so this is captured before the rebuild
    /// and re-applied afterwards by natural key (tag name, module name, CL file name, graph node id).
    /// </summary>
    public class UserDataSnapshot
    {
        public record TagData(int? ProjectId, string? Notes);
        public record CLData(int? ProjectId, string? Notes, string OriginalContent, string Content, string? Indicators);

        public List<DbProject> Projects { get; } = new();
        public List<DbConfigTemplates> ConfigTemplates { get; } = new();
        public List<DbExperionFunctionBlock> ExperionFunctionBlocks { get; } = new();
        public List<DbExperionParameter> ExperionParameters { get; } = new();
        public Dictionary<string, DbTdcModule> Modules { get; } = new();
        public Dictionary<string, TagData> Tags { get; } = new();
        public Dictionary<string, CLData> CLs { get; } = new();
        /// <summary>Graph node id → name of the module the node is assigned to (null = no module).</summary>
        public Dictionary<string, string?> NodeModules { get; } = new();

        public string? BackupPath { get; private set; }

        /// <summary>
        /// Backs up the current database (SQLite only) and reads its user data.
        /// Throws without modifying anything if the existing database can't be read.
        /// </summary>
        public static UserDataSnapshot Capture()
        {
            var snapshot = new UserDataSnapshot();

            if (!TdcContext.Server)
            {
                var dbPath = string.IsNullOrWhiteSpace(TdcContext.ConnectionString) ? TdcContext.ParserConnectionString : TdcContext.ConnectionString;
                if (!File.Exists(dbPath))
                    return snapshot;

                snapshot.BackupPath = $"{dbPath}.bak";
                BackupSqlite(dbPath, snapshot.BackupPath);
            }

            try
            {
                using var db = new TdcContext();

                // Copied into plain instances: the context's lazy-loading proxies can't be attached to another context
                snapshot.Projects.AddRange(db.Projects.AsNoTracking().Select(x => new DbProject { Id = x.Id, Name = x.Name, DeleteOnImport = x.DeleteOnImport }));
                snapshot.ConfigTemplates.AddRange(db.ConfigTemplates.AsNoTracking().Select(x => new DbConfigTemplates { Id = x.Id, TypicalName = x.TypicalName, DeleteOnImport = x.DeleteOnImport }));
                snapshot.ExperionFunctionBlocks.AddRange(db.ExperionFunctionBlocks.AsNoTracking().Select(x => new DbExperionFunctionBlock { Id = x.Id, ConfigTemplateId = x.ConfigTemplateId, Name = x.Name, NewName = x.NewName, Type = x.Type, DeleteOnImport = x.DeleteOnImport }));
                snapshot.ExperionParameters.AddRange(db.ExperionParameters.AsNoTracking().Select(x => new DbExperionParameter { Id = x.Id, FunctionBlockId = x.FunctionBlockId, Name = x.Name, Value = x.Value, DeleteOnImport = x.DeleteOnImport }));

                foreach (var module in db.TdcModules.AsNoTracking().Select(x => new DbTdcModule
                {
                    Id = x.Id, Name = x.Name, Desc = x.Desc, ConfigTemplateId = x.ConfigTemplateId, CEE = x.CEE, Notes = x.Notes, Interlocks = x.Interlocks,
                    Permissives = x.Permissives, MiscLogic = x.MiscLogic, Alarms = x.Alarms, AlarmSuppression = x.AlarmSuppression
                }))
                    snapshot.Modules.TryAdd(module.Name, module);

                foreach (var tag in db.TdcTags.AsNoTracking().Where(x => x.ProjectId != null || x.Notes != null).Select(x => new { x.Name, x.ProjectId, x.Notes }))
                    snapshot.Tags.TryAdd(tag.Name, new(tag.ProjectId, tag.Notes));

                foreach (var cl in db.TdcCLs.AsNoTracking().Select(x => new { x.FileName, x.ProjectId, x.Notes, x.OriginalContent, x.Content, x.Indicators }))
                    snapshot.CLs.TryAdd(cl.FileName, new(cl.ProjectId, cl.Notes, cl.OriginalContent, cl.Content, cl.Indicators));

                foreach (var node in db.TdcNodes.AsNoTracking().Select(x => new { x.NodeId, ModuleName = x.Module == null ? null : x.Module.Name }))
                    snapshot.NodeModules.TryAdd(node.NodeId, node.ModuleName);
            }
            catch (Exception ex)
            {
                var backup = snapshot.BackupPath == null ? "" : $" A backup was saved to {snapshot.BackupPath}.";
                throw new InvalidOperationException(
                    "Could not read user data (projects, templates, notes, CL edits) from the existing database, so it was left unchanged." +
                    $"{backup} If the database is from an older version, rename or delete it and parse again. Details: {ex.Message}", ex);
            }

            return snapshot;
        }

        private static void BackupSqlite(string sourcePath, string backupPath)
        {
            // SQLite's backup API copies a consistent image including anything still in the -wal file
            using var source = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = sourcePath, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
            using var target = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = backupPath, Pooling = false }.ToString());
            source.Open();
            target.Open();
            source.BackupDatabase(target);
        }

        /// <summary>
        /// Re-adds user-owned tables and copies user fields onto freshly parsed tags, modules and CLs.
        /// Call after the parser's modules are added to <paramref name="db"/> and before nodes are created,
        /// so <see cref="TryGetModuleIdForNode"/> can resolve modules that only exist in the snapshot.
        /// </summary>
        public void Restore(TdcContext db, IEnumerable<DbTdcTag> tags, IEnumerable<DbTdcModule> parsedModules, IEnumerable<DbTdcCL> cls)
        {
            db.Projects.AddRange(Projects);
            db.ConfigTemplates.AddRange(ConfigTemplates);
            db.ExperionFunctionBlocks.AddRange(ExperionFunctionBlocks);
            db.ExperionParameters.AddRange(ExperionParameters);

            var projectIds = Projects.Select(x => x.Id).ToHashSet();
            var templateIds = ConfigTemplates.Select(x => x.Id).ToHashSet();

            foreach (var tag in tags)
            {
                if (!Tags.TryGetValue(tag.Name, out var saved))
                    continue;
                tag.ProjectId = saved.ProjectId is int p && projectIds.Contains(p) ? p : null;
                tag.Notes = saved.Notes;
            }

            moduleIdsByName = new();
            foreach (var module in parsedModules)
            {
                moduleIdsByName.TryAdd(module.Name, module.Id);
                if (Modules.TryGetValue(module.Name, out var saved))
                    CopyModuleUserFields(saved, module, templateIds);
            }

            // Modules created by hand in the graph view don't come from the parser; recreate the ones still in use
            var nextModuleId = moduleIdsByName.Count == 0 ? 1 : moduleIdsByName.Values.Max() + 1;
            var usedModuleNames = NodeModules.Values.Where(x => x != null).ToHashSet();
            foreach (var saved in Modules.Values.Where(x => !moduleIdsByName.ContainsKey(x.Name) && usedModuleNames.Contains(x.Name)))
            {
                var module = new DbTdcModule { Id = nextModuleId++, Name = saved.Name, Desc = saved.Desc };
                CopyModuleUserFields(saved, module, templateIds);
                db.TdcModules.Add(module);
                moduleIdsByName[module.Name] = module.Id;
            }

            foreach (var cl in cls)
            {
                if (!CLs.TryGetValue(cl.FileName, out var saved))
                    continue;
                cl.ProjectId = saved.ProjectId is int p && projectIds.Contains(p) ? p : null;
                cl.Notes = saved.Notes;
                // Edits only carry over if they were made against the same source; a changed source CL wins
                if (saved.OriginalContent == cl.OriginalContent)
                {
                    cl.Content = saved.Content;
                    cl.Indicators = saved.Indicators;
                }
            }
        }

        private Dictionary<string, int> moduleIdsByName = new();

        /// <summary>
        /// Module assignment the user had for this graph node, if the node existed before.
        /// Returns false when the node is new, in which case the parser's assignment stands.
        /// </summary>
        public bool TryGetModuleIdForNode(string nodeId, out int? moduleId)
        {
            moduleId = null;
            if (!NodeModules.TryGetValue(nodeId, out var moduleName))
                return false;
            if (moduleName == null)
                return true;
            if (!moduleIdsByName.TryGetValue(moduleName, out var id))
                return false;
            moduleId = id;
            return true;
        }

        private static void CopyModuleUserFields(DbTdcModule from, DbTdcModule to, HashSet<int> templateIds)
        {
            to.ConfigTemplateId = from.ConfigTemplateId is int t && templateIds.Contains(t) ? t : null;
            to.CEE = from.CEE;
            to.Notes = from.Notes;
            to.Interlocks = from.Interlocks;
            to.Permissives = from.Permissives;
            to.MiscLogic = from.MiscLogic;
            to.Alarms = from.Alarms;
            to.AlarmSuppression = from.AlarmSuppression;
        }
    }
}
