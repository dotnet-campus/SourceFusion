using System;
using System.Diagnostics;

namespace dotnetCampus.SourceFusion.Utils
{
    internal sealed class Logger : ILogger, IDisposable
    {
        private readonly Stopwatch _watch;
        private bool _isDisposed;

        public Logger()
        {
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void Message(string text, ConsoleColor? color = null)
        {
            if (color == null)
            {
                Console.WriteLine(text);
            }
            else
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.WriteLine(text);
                Console.ForegroundColor = oldColor;
            }
        }

        public void Time(string progressAbove)
        {

        }

        public void Warning(string text)
        {
            Message($"warning:{text}", ConsoleColor.Yellow);
        }

        public void Error(string text)
        {
            Message($"warning:{text}", ConsoleColor.Red);
        }

        public void Error(Exception exception)
        {
            if (exception is CompilingException ce)
            {
                foreach (var error in ce.Errors)
                {
                    Error(ce);
                }
            }
            else if (exception is AggregateException ae)
            {
                ae.Flatten();
                foreach (var ie in ae.InnerExceptions)
                {
                    Error(ie);
                }
            }
            else
            {
                Error(exception);
            }
        }

        internal TimeSpan Elapsed => _watch.Elapsed;

        ~Logger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (isDisposing)
            {
                _watch.Stop();
            }

            _isDisposed = true;
        }
    }
}
