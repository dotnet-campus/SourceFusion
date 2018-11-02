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
                CompilingFiles = args[5],
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
                        var (workingFolder, intermediateFolder, compilingFiles) = DeconstructPaths(options);
                        PrepairFolders(intermediateFolder);

                        var assembly = new CompileAssembly(compilingFiles);

                        // 分析 IPlainCodeTransformer。
                        var transformer = new CodeTransformer(workingFolder, intermediateFolder, assembly);
                        var excludes = transformer.Transform();

                        // 分析 CompileTimeTemplate。
                        var templateTransformer = new TemplateTransformer(workingFolder, intermediateFolder, assembly);
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
            string[] compilingFiles)
            DeconstructPaths(Options options)
        {
            var workingFolder = Path.GetFullPath(options.WorkingFolder);
            var intermediateFolder = Path.IsPathRooted(options.IntermediateFolder)
                ? Path.GetFullPath(options.IntermediateFolder)
                : Path.GetFullPath(Path.Combine(options.WorkingFolder, options.IntermediateFolder));
            var files = File.ReadAllLines(options.CompilingFiles);
            var compilingFiles = files
                .Select(x => Path.GetFullPath(Path.Combine(workingFolder, x)))
                .ToArray();

            return (workingFolder, intermediateFolder, compilingFiles);
        }

        private static void PrepairFolders(params string[] folders)
        {
            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }
    }
}