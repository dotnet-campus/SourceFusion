using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.SourceFusion.CompileTime;
using dotnetCampus.SourceFusion.Templates;
using dotnetCampus.SourceFusion.Utils;

namespace dotnetCampus.SourceFusion
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (!Debugger.IsAttached
                && args.Any(x => x.Equals("--DebugMode", StringComparison.CurrentCultureIgnoreCase)))
            {
                Debugger.Launch();
            }

            var stopwatch = Stopwatch.StartNew();

            var exitCode = Parser.Default.ParseArguments<Options>(args)
                .MapResult(Run, HandleError);

            stopwatch.Stop();
            if (Debugger.IsAttached && !Console.IsInputRedirected)
            {
                Console.WriteLine($"预编译耗时：{stopwatch.Elapsed}");
                Console.WriteLine($"调试模式下已暂停，按任意键结束……");
                Console.Read();
            }

            return exitCode;
        }

        private static int Run(Options options)
        {
            using (var logger = new Logger())
            {
                try
                {
                    var exitCode = OnRun(options, logger);
                    return exitCode;
                }
                catch (Exception ex)
                {
                    logger.Error($"SourceFusion 内部错误：{ex}");
                    return 0;
                }
            }
        }

        private static int OnRun(Options options, ILogger logger)
        {
            try
            {
                var (workingFolder, intermediateFolder, generatedCodeFolder, compilingFiles) = DeconstructPaths(options);
                var rebuildRequired = options.RebuildRequired;
                var cachedExcludesListFile = Path.Combine(intermediateFolder, "Excludes.txt");

                // 如果可以差量编译，那么检测之前已经生成的文件，然后将其直接输出。
                if (!rebuildRequired && File.Exists(cachedExcludesListFile))
                {
                    var cachedExcludeLines = File.ReadAllLines(cachedExcludesListFile, Encoding.UTF8);
                    foreach (var exclude in cachedExcludeLines)
                    {
                        logger.Message(exclude);
                    }
                    return 0;
                }

                var assembly = new CompileAssembly(compilingFiles);

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
                    logger.Message(exclude);
                }

                File.WriteAllLines(cachedExcludesListFile, toExcludes, Encoding.UTF8);
            }
            catch (CompilingException ex)
            {
                foreach (var error in ex.Errors)
                {
                    logger.Error($"{error}");
                }
            }

            return 0;
        }

        private static int HandleError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                if (error is UnknownOptionError uoe)
                {
                    Console.WriteLine($"error: SourceFusion 参数错误：不能识别的参数 {uoe.Token}。");
                }
                else if (error is MissingRequiredOptionError mroe)
                {
                    Console.WriteLine($"error: SourceFusion 参数错误：缺少必需的参数 {mroe.NameInfo.LongName}。");
                }
                else
                {
                    Console.WriteLine($"error: SourceFusion 参数错误：{error}");
                }
            }

            return 0;
        }

        [Pure]
        private static (
            string workingFolder,
            string intermediateFolder,
            string generatedCodesFolder,
            string[] compilingFiles)
            DeconstructPaths(Options options)
        {
            var workingFolder = Path.GetFullPath(options.WorkingFolder);

            var intermediateFolder = Path.IsPathRooted(options.IntermediateFolder)
                ? Path.GetFullPath(options.IntermediateFolder)
                : Path.GetFullPath(Path.Combine(options.WorkingFolder, options.IntermediateFolder));

            var generatedCodesFolder = Path.IsPathRooted(options.GeneratedCodeFolder)
                ? Path.GetFullPath(options.GeneratedCodeFolder)
                : Path.GetFullPath(Path.Combine(options.WorkingFolder, options.GeneratedCodeFolder));

            var compilingFiles = File.ReadAllLines(options.CompilingFiles)
                .Select(x => Path.GetFullPath(Path.Combine(workingFolder, x)))
                .ToArray();

            var filterFiles = File.Exists(options.FilterFiles)
                ? File.ReadAllLines(options.FilterFiles)
                    .Select(x => Path.GetFullPath(Path.Combine(workingFolder, x)))
                    .ToArray()
                : new string[0];

            // filterFiles 是仅需扫描的文件，用 compilingFiles 取一下交集，可以避免被移除的文件也加入编译范围。
            var filteredCompilingFiles = filterFiles.Any()
                ? compilingFiles.Intersect(filterFiles).ToArray()
                : compilingFiles;

            return (workingFolder, intermediateFolder, generatedCodesFolder, filteredCompilingFiles);
        }
    }
}