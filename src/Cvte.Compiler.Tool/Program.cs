using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CommandLine;
using Cvte.Compiler.Cli;
using Cvte.Compiler.CompileTime;

namespace Cvte.Compiler
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Initialize basic command options.
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    if (options.DebugMode)
                    {
                        Debugger.Launch();
                    }

                    try
                    {
                        var (workingFolder, intermediateFolder, compilingFiles) = DeconstructPaths(options);
                        PrepairFolders(intermediateFolder);

                        var assembly = new CompileAssembly(compilingFiles);
                        var transformer = new CodeTransformer(workingFolder, intermediateFolder, assembly);
                        var excludes = transformer.Transform();
                        var toExcludes = string.Join($"{Environment.NewLine}",
                            excludes.Select(x => PathEx.MakeRelativePath(workingFolder, x)));

                        Console.WriteLine(toExcludes);
                    }
                    catch (CompilingException ex)
                    {
                        foreach (var error in ex.Errors)
                        {
                            Console.Write($"error:{error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write($"error:{ex}");
                    }
                })
                .WithNotParsed(errorList => { });

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
            var compilingFiles = options.CompilingFiles
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
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
