using System;

namespace Cvte.Compiler.CompileTime
{
    public interface ICompileAttribute
    {
        string Name { get; }
        bool Match<TAttribute>() where TAttribute : Attribute;
    }
}
