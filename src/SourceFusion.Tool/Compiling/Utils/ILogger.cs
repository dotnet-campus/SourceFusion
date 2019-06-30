using System;

namespace dotnetCampus.Compiling.Utils
{
    public interface ILogger
    {
        void Message(string text, ConsoleColor? color = null);
        void Warning(string text);
        void Error(string text);
        void Error(Exception exception);
    }
}
