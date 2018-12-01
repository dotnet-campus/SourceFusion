using System;
using System.Diagnostics;

namespace dotnetCampus.SourcePerformance.Framework
{
    public sealed class PerformanceCounter
    {
        private readonly Stopwatch _watch;

        public PerformanceCounter(bool start = true)
        {
            _watch = new Stopwatch();
            if (start)
            {
                Start();
            }
        }

        public TimeSpan Elapsed => _watch.Elapsed;

        public void Start()
        {
            _watch.Start();
        }
    }
}