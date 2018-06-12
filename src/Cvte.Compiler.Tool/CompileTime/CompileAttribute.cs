namespace Cvte.Compiler.CompileTime
{
    public class CompileAttribute : ICompileAttribute
    {
        public CompileAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
