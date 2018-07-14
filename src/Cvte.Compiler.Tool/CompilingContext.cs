using Cvte.Compiler.CompileTime;

namespace Cvte.Compiler
{
    internal class CompilingContext : ICompilingContext
    {
        public CompilingContext(ICompileAssembly assembly)
        {
            Assembly = assembly;
        }

        public ICompileAssembly Assembly { get; }
    }
}
