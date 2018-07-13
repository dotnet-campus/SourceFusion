using System;

namespace Cvte.Compiler
{
    public static class Placeholder
    {
        [CompilerMethod]
        public static extern T[] Array<T>(Func<ICompilingContext, string> compileTimeCodeGenerator);
    }
}
