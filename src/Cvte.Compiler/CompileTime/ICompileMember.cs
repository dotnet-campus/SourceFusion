namespace Cvte.Compiler.CompileTime
{
    public interface ICompileMember : ICompileAttributeProvider
    {
        string Name { get; }
    }
}
