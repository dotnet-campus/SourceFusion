using System;

namespace Cvte.Compiler.CompileTime
{
    public interface ICompileAssembly
    {
        ICompileType[] GetTypes();
    }
}
