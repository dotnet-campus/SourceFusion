using System;
using System.Diagnostics;

using dotnetCampus.Cli;
using dotnetCampus.Cli.Standard;
using dotnetCampus.MSBuildUtils;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.TelescopeTask.Tasks;

namespace dotnetCampus.TelescopeTask
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                new MSBuildMessage($"dotnetCampus.TelescopeTask：开始编译…").Message();
                CommandLine.Parse(args)
                    .AddStandardHandlers()
                    .AddHandler<Options>(o => new CompileTask().Run(o))
                    .Run();
            }
            catch (Exception ex)
            {
                new MSBuildMessage(ex.ToString().Replace(Environment.NewLine, " ")).Error();
            }
            finally
            {
                new MSBuildMessage($"dotnetCampus.TelescopeTask：编译完成，耗时 {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds} s。").Message();
            }
        }
    }
}
