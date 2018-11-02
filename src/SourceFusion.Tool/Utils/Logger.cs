using System;

namespace dotnetCampus.SourceFusion.Utils
{
    internal class Logger : ILogger
    {
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
    }
}
