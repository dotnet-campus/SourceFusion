using System;

namespace Cvte.Compiler.CompileTime
{
    public interface ICompileMember : ICompileAttributeProvider
    {
        string Name { get; }

        MemberModifiers MemberModifiers { get; }
    }

    [Flags]
    public enum MemberModifiers
    {
        Unset = 0,
        Private = 1,
        Protected = 1 << 1,
        Public = 1 << 2,
        Internal = 1 << 3,
    }
}
