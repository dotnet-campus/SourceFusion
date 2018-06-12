using System;

namespace Cvte.Compiler.CompileTime
{
    public interface ICompileAssembly
    {
        ICompileType[] GetTypes();

        ICompileType[] GetAttributedTypes<TAttribute>() where TAttribute : Attribute;
    }
}
