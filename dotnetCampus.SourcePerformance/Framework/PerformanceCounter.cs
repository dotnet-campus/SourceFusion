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

        public TimeSpan FrameworkLoaded { get; private set; }
        public TimeSpan ExtensionFound { get; private set; }
        public TimeSpan Completed { get; private set; }

        public void Start()
        {
            _watch.Start();
        }

        public void Framework()
        {
            FrameworkLoaded = _watch.Elapsed;
        }

        public void Extension()
        {
            ExtensionFound = _watch.Elapsed;
        }

        public void Complete()
        {
            Completed = _watch.Elapsed;
            _watch.Stop();
        }
    }
}