using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cvte.Compiler.Command;
using McMaster.Extensions.CommandLineUtils;

namespace Cvte.Compiler
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Initialize basic command options.
            var app = new CommandLineApplication
            {
                Name = "Cvte.Compiler"
            };

            // Config command line from command tasks assembly.
            app.ReflectFrom(typeof(CommandTask).Assembly);

            // Execute the app.
            var exitCode = app.Execute(args);
            return exitCode;


            if (args.Contains("--debug-mode"))
            {
                Debugger.Launch();
            }

            if (args.Length < 6)
            {
                //-p ..\\..\\tests\\Cvte.Compiler.Tests -i obj\\GeneratedCode -c Sample.cs

                UsingForegroundColor(ConsoleColor.Red, () => Console.WriteLine("必须传入足够的参数。"));
                return -1;
            }

            try
            {
                //转换源码的工作路径
                var workingFolder = Path.GetFullPath(args[1]);
                //中间文件的生成路径
                var intermediateFolder = args[3];
                // 所有参与编译的文件
                var compilingFiles = args[5].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var transformer = new CodeTransformer(workingFolder, intermediateFolder, compilingFiles);
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
            return 0;
        }

        private static void UsingForegroundColor(ConsoleColor color, Action action)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            try
            {
                action();
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

      
    }


}
