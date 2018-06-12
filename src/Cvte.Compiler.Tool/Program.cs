using System;
using System.Linq;

namespace Cvte.Compiler
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 6)
            {
                UsingForegroundColor(ConsoleColor.Red, () => Console.WriteLine("必须传入足够的参数。"));
                return -1;
            }

            var workingFolder = args[1];
            var intermediateFolder = args[3];
            var compilingFiles = args[5].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var transformer = new CodeTransformer(workingFolder, intermediateFolder, compilingFiles);
            var excludes = transformer.Transform();
            var toExcludes = string.Join($"{Environment.NewLine}", excludes.Select(x => PathEx.MakeRelativePath(workingFolder, x)));

            Console.WriteLine(toExcludes);
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
