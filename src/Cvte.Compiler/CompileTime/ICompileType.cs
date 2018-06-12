namespace Cvte.Compiler.CompileTime
{
    public interface ICompileType : ICompileMember
    {
        ICompileType BaseType { get; }
        ICompileInterface[] Interfaces { get; }
        ICompileProperty[] Properties { get; }
        ICompileMethod[] Methods { get; }
        ICompileField[] Fields { get; }
    }
}
