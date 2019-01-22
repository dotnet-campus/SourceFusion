using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.SourceFusion.Utils;

namespace dotnetCampus.SourceFusion
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (!Debugger.IsAttached
                && args.Any(x => x.Equals("--debug-mode", StringComparison.CurrentCultureIgnoreCase)))
            {
                Debugger.Launch();
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            var stopwatch = Stopwatch.StartNew();

            var exitCode = Parser.Default.ParseArguments<Options>(args)
                .MapResult(Run, HandleError);

            stopwatch.Stop();
            if (Debugger.IsAttached && !Console.IsInputRedirected)
            {
                Console.WriteLine($"预编译耗时：{stopwatch.Elapsed}");
                Console.WriteLine($"调试模式下已暂停，按任意键结束……");
                Console.ReadKey();
            }

            return exitCode;
        }

        private static int Run(Options options)
        {
            using (var logger = new Logger())
            {
                try
                {
                    var app = new App(options, logger);
                    app.Run();
                }
                catch (Exception ex)
                {
                    logger.Error($"SourceFusion 内部错误：{ex}");
                }
            }

            return 0;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"error: SourceFusion 未处理的异常：{e.ExceptionObject}");
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine($"error: SourceFusion 未观察的任务异常：{e.Exception}");
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
    }
}
