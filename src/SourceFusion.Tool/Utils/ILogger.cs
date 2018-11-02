using System;

namespace dotnetCampus.SourceFusion.Utils
{
    public interface ILogger
    {
        void Message(string text, ConsoleColor? color = null);
        void Warning(string text);
        void Error(string text);
        void Error(Exception exception);
    }
}
