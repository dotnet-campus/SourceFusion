namespace Cvte.Compiler.CompileTime
{
    public interface ICompileType : ICompileMember
    {
        string Namespace { get; }
        string FullName { get; }
        ICompileType BaseType { get; }
        ICompileInterface[] Interfaces { get; }
        ICompileProperty[] GetProperties();
        ICompileMethod[] GetMethods();
        ICompileField[] GetFields();
    }
}
