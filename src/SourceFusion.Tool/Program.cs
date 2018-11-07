using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
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
            var options = new Options
            {
                WorkingFolder = args[1],
                IntermediateFolder = args[3],
                GeneratedCodeFolder = args[5],
                CompilingFiles = args[7],
                FilterFiles = args[9],
                RebuildRequired = args[11]?.Equals("true", StringComparison.InvariantCultureIgnoreCase) is true,
            };

            if (options.DebugMode)
            {
                Debugger.Launch();
            }

            using (var logger = new Logger())
            {
                var exitCode = Run(options, logger);

                if (Debugger.IsAttached)
                {
                    Console.WriteLine($"预编译耗时：{logger.Elapsed}");
                    Console.WriteLine($"调试模式下已暂停，按任意键结束……");
                    Console.ReadKey();
                }

                return exitCode;
            }
        }

        private static int Run(Options options, ILogger logger)
        {
            // Initialize basic command options.
            //Parser.Default.ParseArguments<Options>(args)
            //    .WithParsed(options =>
            //    {
                    try
                    {
                        var (workingFolder, intermediateFolder, generatedCodeFolder, compilingFiles) = DeconstructPaths(options);
                        var rebuildRequired = options.RebuildRequired;
                        
                        // 如果可以差量编译，那么检测之前已经生成的文件，然后将其直接输出。
                        //if (!rebuildRequired)
                        //{
                        //    return 0;
                        //}

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
                    }
                    catch (CompilingException ex)
                    {
                        foreach (var error in ex.Errors)
                        {
                            logger.Error($"error:{error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"error:{ex}");
                    }
                //})
                //.WithNotParsed(errorList => { });

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