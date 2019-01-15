using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.SourceFusion.CompileTime;
using dotnetCampus.SourceFusion.Core;
using dotnetCampus.SourceFusion.Templates;
using dotnetCampus.SourceFusion.Transforming;
using dotnetCampus.SourceFusion.Utils;

namespace dotnetCampus.SourceFusion
{
    internal class App
    {
        private readonly Options _options;
        private readonly ILogger _logger;

        public App(Options options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public int Run()
        {
            try
            {
                RunCore();
            }
            catch (CompilingException ex)
            {
                foreach (var error in ex.Errors)
                {
                    _logger.Error($"{error}");
                }
            }

            return 0;
        }

        private void RunCore()
        {
            var (workingFolder, intermediateFolder, generatedCodeFolder, compilingFiles, referencingFiles) =
                DeconstructPaths(_options);
            var rebuildRequired = _options.RebuildRequired;
            var cachedExcludesListFile = Path.Combine(intermediateFolder, "Excludes.txt");

            // 如果可以差量编译，那么检测之前已经生成的文件，然后将其直接输出。
            if (!rebuildRequired && File.Exists(cachedExcludesListFile))
            {
                var cachedExcludeLines = File.ReadAllLines(cachedExcludesListFile, Encoding.UTF8);
                foreach (var exclude in cachedExcludeLines)
                {
                    _logger.Message(exclude);
                }

                return;
            }

            var assembly = new CompileAssembly(compilingFiles, referencingFiles, _options.PreprocessorSymbols);

            // 分析 IPlainCodeTransformer。
            var transformer = new CodeTransformer(workingFolder, generatedCodeFolder, assembly);
            var excludes = transformer.Transform();

            // 分析 CompileTimeTemplate。
            var templateTransformer = new TemplateTransformer(workingFolder, generatedCodeFolder, assembly);
            var templateExcludes = templateTransformer.Transform();

            var toExcludes = excludes.Union(templateExcludes)
                .Select(x => PathEx.MakeRelativePath(workingFolder, x)).ToArray();

            foreach (var exclude in toExcludes)
            {
                _logger.Message(exclude);
            }

            File.WriteAllLines(cachedExcludesListFile, toExcludes, Encoding.UTF8);
        }

        [Pure]
        private static (
            string workingFolder,
            string intermediateFolder,
            string generatedCodesFolder,
            string[] compilingFiles,
            string[] referencingFiles)
            DeconstructPaths(Options options)
        {
            var workingFolder = Path.GetFullPath(options.WorkingFolder);

            var intermediateFolder = FullPath(options.IntermediateFolder);
            var generatedCodesFolder = FullPath(options.GeneratedCodeFolder);

            var compilingFiles = File.ReadAllLines(options.CompilingFiles).Select(FullPath).ToArray();
            var filterFiles = File.Exists(options.FilterFiles)
                ? File.ReadAllLines(options.FilterFiles).Select(FullPath).ToArray()
                : new string[0];

            // filterFiles 是仅需扫描的文件，用 compilingFiles 取一下交集，可以避免被移除的文件也加入编译范围。
            var filteredCompilingFiles = filterFiles.Any()
                ? compilingFiles.Intersect(filterFiles).ToArray()
                : compilingFiles;

            var referencingFiles = File.Exists(options.References)
                ? File.ReadAllLines(options.References).Select(FullPath).ToArray()
                : new string[0];

            return (workingFolder, intermediateFolder, generatedCodesFolder, filteredCompilingFiles, referencingFiles);

            string FullPath(string path) => Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(options.WorkingFolder, path));
        }
    }
}
