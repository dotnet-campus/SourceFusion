using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using Cvte.Compiler.Command;

namespace Cvte.Compiler
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Initialize basic command options.
            CommandLine.Parser.Default.ParseArguments<Command.Command>(args)
                .WithParsed(options =>
                {
                    if (options.DebugMode)
                    {
                        Debugger.Launch();
                    }

                    try
                    {
                        var transformer = new CodeTransformer(options.WorkFolder, options.IntermediateFolder,
                            options.CompilingFiles.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries));

                        var excludes = transformer.Transform();
                        var toExcludes = string.Join($"{Environment.NewLine}",
                            excludes.Select(x => PathEx.MakeRelativePath(options.WorkFolder, x)));

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
    }
}