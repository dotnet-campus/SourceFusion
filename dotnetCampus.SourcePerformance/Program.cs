using System;
using dotnetCampus.SourcePerformance.Framework;

namespace dotnetCampus.SourcePerformance
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Services.PerformanceCounter.Start();

            var app = new App();
            app.Run();
        }
    }
}