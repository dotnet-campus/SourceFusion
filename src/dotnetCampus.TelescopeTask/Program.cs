using System;

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
            try
            {
                CommandLine.Parse(args)
                    .AddStandardHandlers()
                    .AddHandler<Options>(o => new CompileTask().Run(o))
                    .Run();
            }
            catch (Exception ex)
            {
                new MSBuildMessage(ex.ToString().Replace(Environment.NewLine, " ")).Error();
            }
        }
    }
}
