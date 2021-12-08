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
            if (string.IsNullOrEmpty(options.WorkingDirectory))
            {
                throw new ArgumentNullException(nameof(options.WorkingDirectory) + $" CommandLine: {Environment.CommandLine}");
            }

            if (string.IsNullOrEmpty(options.ToolFolder))
            {
                throw new ArgumentNullException(nameof(options.ToolFolder) + $" CommandLine: {Environment.CommandLine}");
            }

            if (string.IsNullOrEmpty(options.GeneratedCodeFolder))
            {
                throw new ArgumentNullException(nameof(options.GeneratedCodeFolder) + $" CommandLine: {Environment.CommandLine}");
            }

            WorkingFolder = Path.GetFullPath(options.WorkingDirectory);
            ToolsFolder = FullPath(options.ToolFolder);
            GeneratedCodeFolder = FullPath(options.GeneratedCodeFolder);
            PreprocessorSymbols = options.PreprocessorSymbols;

            // 初始化项目属性。
            _projectProperties = Deserialize(FullPath(options.ProjectPropertyFile));

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

        /// <summary>
        /// 获取当前的工作路径，通常是项目文件所在的文件夹。
        /// </summary>
        public string WorkingFolder { get; }

        /// <summary>
        /// 获取此程序可以使用的与 .targets 文件进行数据交换的文件夹，也可用于存放中间文件或缓存。
        /// </summary>
        public string ToolsFolder { get; }
        public string GeneratedCodeFolder { get; }
        public IReadOnlyList<string> CompilingFiles { get; }
        public IReadOnlyList<string> References { get; }
        public string PreprocessorSymbols { get; }
        public CompileAssembly Assembly { get; }

        /// <summary>
        /// 获取项目属性。如果项目没有定义这个属性，那么会得到空字符串。
        /// 这相当于 $(PropertyName)。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <returns>属性的值。</returns>
        public string GetProperty(string propertyName) => _projectProperties.TryGetValue(
            propertyName ?? throw new ArgumentNullException(nameof(propertyName)), out var value)
            ? value
            : "";

        /// <summary>
        /// 获取项目的集合。如果项目没有定义这个集合，那么会得到空字符串数组。
        /// 这相当于 @(ItemName)。
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public string[] GetItems(string itemName) =>
            _projectProperties.TryGetValue(itemName ?? throw new ArgumentNullException(nameof(itemName)), out var value)
                ? value.Split(new[] {';', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                : new string[0];

        /// <summary>
        /// 包含从属性文件中获取的所有的来自项目的属性和集合。
        /// </summary>
        private readonly Dictionary<string, string> _projectProperties;

        /// <summary>
        /// 将路径转换为绝对路径，如果路径是相对路径，则转换时将相对于当前工作路径进行转换。
        /// </summary>
        /// <param name="path">相对或绝对路径。</param>
        /// <returns>经过格式化的绝对路径。</returns>
        private string FullPath(string path)
        {
            // 在 Windows 下，`/` 和 `\` 都是合理的路径分割符；
            // 在 Linux 下，`/` 是合理的路径分割符，`\` 是合理的文件名。
            // 
            // 因为 VsProjectSystem 中无论在什么平台都传入 `\` 作为路径分割符，
            // 所以我们所有从 csproj 中收集到的路径都是用 `\` 分割，所有 Linux 系统里从 `Path` 得到的路径都是 `/` 分割。
            // 因此，我们必须将所有的 `/` 和 `\` 都格式化为系统相关才可以在各系统下正常工作。
            // 
            // 详见：https://blog.walterlv.com/post/format-mixed-path-seperators-to-platform-special

            var fullPath = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(WorkingFolder, path));
            return fullPath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 解析用于存储项目各种属性的文件，并得到项目的各种属性和集合。
        /// </summary>
        /// <param name="propertyFile">收集所有属性的文件。</param>
        /// <returns>从文件中得到的所有属性的字典。</returns>
        private static Dictionary<string, string> Deserialize(string propertyFile)
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
                        keyValue[currentKey] = currentValue ?? "";
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
                    currentValue = currentValue == null ? line.Trim() : $@"{currentValue}
{line.Trim()}";
                }
            }

            if (currentKey != null)
            {
                keyValue[currentKey] = currentValue ?? "";
            }

            return keyValue;
        }
    }
}
