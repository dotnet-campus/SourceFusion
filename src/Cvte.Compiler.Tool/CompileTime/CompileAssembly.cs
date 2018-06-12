using System;

namespace Cvte.Compiler.CompileTime
{
    internal class CompileAssembly : ICompileAssembly
    {
        public CompileFile[] GetFiles()
        {
            throw new NotImplementedException();
        }

        public ICompileType[] GetTypes()
        {
            throw new NotImplementedException();
        }

        public ICompileType[] GetAttributedTypes<TAttribute>() where TAttribute : Attribute
        {
            throw new NotImplementedException();
        }
    }
}
