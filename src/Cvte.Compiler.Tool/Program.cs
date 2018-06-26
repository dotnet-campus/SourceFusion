using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cvte.Compiler
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Contains("--debug-mode"))
            {
                Debugger.Launch();
            }

            if (args.Length < 6)
            {
                UsingForegroundColor(ConsoleColor.Red, () => Console.WriteLine("必须传入足够的参数。"));
                return -1;
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            try
            {
                var workingFolder = args[1];
                var intermediateFolder = args[3];
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

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            if (e.Name.Contains("Cvte.Compiler.Core"))
            {
                var baseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var targetFolder = Path.Combine(baseFolder, "../../lib");
                targetFolder = new DirectoryInfo(targetFolder).GetDirectories(
#if NETCOREAPP
                    "netstandard2.0"
#else
                    "net45"
#endif
                    , SearchOption.TopDirectoryOnly).FirstOrDefault()?.FullName;
                var dll = Path.Combine(targetFolder, "Cvte.Compiler.Core.dll");
                return Assembly.LoadFile(dll);
            }

            return null;
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
