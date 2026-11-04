using System.Globalization;
using System.Text.RegularExpressions;
using Honeywell.TDC;
using Honeywell.Parsers.TDC.Graphs;
using Honeywell.Parsers.TDC.Logic;
using Microsoft.Msagl.Drawing;

namespace Honeywell.Parsers.TDC
{
    //                                                                                  FFL      Point    Parameter
    //todo: parse FFL.ZF file for free format refs (separated by spaces) e.g. NET>FFL   ANALRPT3 UY5825RP.Y_TIME(15) 
    //todo: parse LVR log
    //todo: am node should be inferred from eb file name unless file is named "FILE_xxx".
    //todo: break up devctl
    public static partial class Parser
    {
        public static List<TdcModule> Modules { get; set; } = new();
        public static List<TdcParameter> TdcParameters { get; set; } = new(100000);
        public static List<TdcCL> TdcCL { get; set; } = new(100);
        public static Dictionary<string, TdcTag> TdcTags { get; set; } = new();

        public static string GroupFilePath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Groups.xlsx");
        public static string ModulesFilePath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Modules.xlsx");

        /// <summary>Parses the files into <see cref="TdcTags"/> and the other lists, and optionally rebuilds the maps.</summary>
        public static List<TdcTag> ParseTdcFiles(string ebFolderPath, string? clFolderPath, string? jsFolderPath, string? customConnectionsPath, string? clSlotsConnectionPath, bool createGraphs, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Start from a clean slate so a second parse in the same session doesn't duplicate the first
            TdcTags.Clear();
            TdcParameters.Clear();
            TdcCL.Clear();
            Modules.Clear();

            if (createGraphs)
                GraphFactory.Clear();

            var clSlots = clSlotsConnectionPath != null ? ReadClSlots(clSlotsConnectionPath) : new List<CLSlot>();

            // References can point into any file, so they are resolved once all the files are read
            TdcParameter.TdcTagDict = TdcTags;
            foreach (var file in FilesIn(ebFolderPath, "EB", "*.eb", searchOption))
                EbParseContents(File.ReadAllText(file.FullName), file.Name);
            ApplyClSlots(clSlots);
            foreach (var parameter in TdcParameters)
                ResolveSourceParameter(parameter);
            AddRegDigHgConnections();

            if (jsFolderPath != null)
                ParseJsFolder(jsFolderPath, searchOption);

            if (clFolderPath != null)
                foreach (var file in FilesIn(clFolderPath, "CL", "*.cl", searchOption))
                    CLParseContent(File.ReadAllText(file.FullName), file.Name);

            if (createGraphs)
            {
                foreach (var tag in TdcTags.Values)
                    GraphFactory.TreeDict.TryAdd(tag.Name, new List<Graph?>());

                GraphFactory.CreateGraphs(customConnectionsPath);
            }

            return TdcTags.Values.ToList();
        }

        private static FileInfo[] FilesIn(string folder, string kind, string pattern, SearchOption searchOption)
        {
            if (!Directory.Exists(folder))
                throw new Exception($"Invalid {kind} File Path");
            return new DirectoryInfo(folder).GetFiles(pattern, searchOption);
        }

        /// <summary>Reads a CL slots report: columns separated by spaces, with the tag third and the block fourth.</summary>
        private static List<CLSlot> ReadClSlots(string path)
        {
            if (!File.Exists(path))
                throw new Exception("Invalid CL Slots File Path");

            var slots = new List<CLSlot>();
            foreach (var line in File.ReadAllLines(path))
            {
                var fields = regexEbSpace.Replace(line, " ").Trim().Split(' ');
                if (fields.Length == 4 && fields[0] != "MEDIA" && fields[0] != "---------")
                    slots.Add(new(fields[3].Trim().ToUpper(), fields[2].Trim().ToUpper()));
            }
            return slots;
        }

        private static void ParseJsFolder(string folder, SearchOption searchOption)
        {
            List<LogicHgGate> gates = new();
            foreach (var file in FilesIn(folder, "JS", "*.js", searchOption))
                JSParseContent(File.ReadAllLines(file.FullName), file.Name, gates);

            foreach (var gate in gates)
            {
                AddConnectionsToLogicHg(gate, gate.Input1, "HGSRC(1)");
                AddConnectionsToLogicHg(gate, gate.Input2, "HGSRC(2)");
                AddConnectionsToLogicHg(gate, gate.Input3, "HGSRC(3)");
                AddConnectionsToLogicHg(gate, gate.Output1, "HGDSTN(1)");
                AddConnectionsToLogicHg(gate, gate.Output2, "HGDSTN(2)");
            }
        }

        /// <summary>Adds a parameter the parser worked out itself, so it has no separate raw text.</summary>
        private static void AddParameter(TdcTag tag, string name, string? value)
        {
            var parameter = new TdcParameter(tag, name, value, value);
            tag.Params.Add(parameter);
            TdcParameters.Add(parameter);
        }

        public static List<TdcTag> GetModules(string ebFolderPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return GetModules(ParseTdcFiles(ebFolderPath, null, null, null, null, false, searchOption));
        }

        public static List<TdcTag> GetModules()
        {
            return GetModules(TdcTags.Values.ToList());
        }

        public static List<TdcTag> GetModules(List<TdcTag> tdcTags)
        {
            var parentModuleTypes = new List<string>() { "REGCLNIM", "DICMPNIM", "DEVCTL", "ANLCMPHG", "DIGCMPHG", "REGAM", "REGHG" };
            var singleModuleTypes = new List<string>() { "ANINNIM", "ANLINHG", "ANOUTNIM", "ANLOUTHG", "DIINNIM", "DIGINHG", "DIOUTNIM", "DIGOUTHG", "REGPVNIM", "PRMODNIM", "PRCMODHG", "CUSTOMAM", "FLAGNIM", "FLAGAM" };
            var ParentConnectionTypes = new List<string>() { "CISRC", "CODSTN", "DISRC", "DODSTN", "HGSRC", "HGDSTN" };

            var parentTags = tdcTags.Where(x => parentModuleTypes.Contains(x.PointType));

            foreach (var parentTag in parentTags)
            {
                var module = new TdcModule(parentTag.Name, parentTag.Desc);
                module.TdcTags.Add(parentTag);
                parentTag.Module = module;

                foreach (var parameter in parentTag.Params)
                {
                    foreach (var connectionType in ParentConnectionTypes)
                    {
                        var index = parameter.Name.IndexOf(connectionType);
                        if (index == 0)
                    {
                            var tagParam = parameter.Value?.Split('.');
                            if (tagParam?.Length > 0)
                            {
                                var childTagName = tagParam[0].ToUpper();
                                var childTag = tdcTags.FirstOrDefault(x => x.Name == childTagName);
                                if (childTag != null && !parentModuleTypes.Contains(childTag.PointType))
                                {
                                    childTag.Module = module;
                                    module.TdcTags.Add(childTag);
                                }
                            }
                        }
                    }                  
                }

                Modules.Add(module);
            }

            var otherTags = tdcTags.Where(x => x.Module == null);
            foreach (var tag in otherTags)
            {
                if (singleModuleTypes.Contains(tag.PointType))
                {
                    var module = new TdcModule(tag.Name, tag.Desc);
                    tag.Module = module;
                    Modules.Add(module);
                }
                    
            }

            return tdcTags;
        }

        public static void ExportModules(string ebFolderPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var modules = GetModules(ebFolderPath, searchOption);
            ExportModules(modules, ModulesFilePath);
        }

        public static void ExportModules(List<TdcTag> tdcTags, string savePath)
        {
            Export.ExportTdcToXlsx(new List<TdcTag>(tdcTags), savePath);
        }

        public static void ExportGroups(string ebFolderPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            List<TdcGroup> groups = new(1000);
            foreach (var file in FilesIn(ebFolderPath, "EB", "*.eb", searchOption))
                EbParseGroups(File.ReadAllText(file.FullName), file.Name, groups);

            Export.ExportGroups(groups, GroupFilePath);
        }
    }
}
