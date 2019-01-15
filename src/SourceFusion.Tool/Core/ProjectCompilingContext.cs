using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.SourceFusion.CompileTime;

namespace dotnetCampus.SourceFusion.Core
{
    internal class ProjectCompilingContext
    {
        internal ProjectCompilingContext(Options options)
        {
            // 初始化基础属性。
            WorkingFolder = Path.GetFullPath(options.WorkingFolder);
            ToolsFolder = FullPath(options.ToolFolder);
            GeneratedCodeFolder = FullPath(options.GeneratedCodeFolder);
            PreprocessorSymbols = options.PreprocessorSymbols;

            // 初始化项目属性。
            _projectProperties = Deserialize(options.ProjectPropertyFile);

            // 初始化编译文件和引用。
            // filterFiles 是仅需扫描的文件，用 compilingFiles 取一下交集，可以避免被移除的文件也加入编译范围。
            var compilingFiles = GetItems("Compile").Select(FullPath).ToArray();
            var filterFiles = GetItems("PrecompileFilter").Select(FullPath).ToArray();
            var filteredCompilingFiles = filterFiles.Any()
                ? compilingFiles.Intersect(filterFiles).ToArray()
                : compilingFiles;
            var referencingFiles = GetItems("ReferencePath").Select(FullPath).ToArray();
            CompilingFiles = filteredCompilingFiles;
            References = referencingFiles;

            // 初始化编译程序集。
            Assembly = new CompileAssembly(CompilingFiles, References, PreprocessorSymbols);
        }

        public string WorkingFolder { get; }
        public string ToolsFolder { get; }
        public string GeneratedCodeFolder { get; }
        public IReadOnlyList<string> CompilingFiles { get; }
        public IReadOnlyList<string> References { get; }
        public string PreprocessorSymbols { get; }
        public CompileAssembly Assembly { get; }

        public string this[string property] => GetProperty(property);

        public string GetProperty(string propertyName) => _projectProperties.TryGetValue(
            propertyName ?? throw new ArgumentNullException(nameof(propertyName)), out var value)
            ? value
            : "";

        public string[] GetItems(string itemName) =>
            _projectProperties.TryGetValue(itemName ?? throw new ArgumentNullException(nameof(itemName)), out var value)
                ? value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                : new string[0];

        private readonly Dictionary<string, string> _projectProperties;

        private string FullPath(string path) => Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(WorkingFolder, path));

        private Dictionary<string, string> Deserialize(string propertyFile)
        {
            var keyValue = new Dictionary<string, string>();
            var lines = File.ReadAllLines(propertyFile);

            string currentKey = null;
            string currentValue = null;
            foreach (var line in lines)
            {
                if (line.StartsWith(">"))
                {
                    if (currentKey != null)
                    {
                        keyValue[currentKey] = currentValue;
                    }

                    currentKey = null;
                    currentValue = null;
                    continue;
                }

                if (currentKey == null)
                {
                    currentKey = line.Trim();
                }
                else
                {
                    currentValue = $@"{currentValue}
{line.Trim()}";
                }
            }

            if (currentKey != null)
            {
                keyValue[currentKey] = currentValue;
            }

            return keyValue;
        }
    }
}
