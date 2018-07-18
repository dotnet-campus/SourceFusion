using System.Collections.Generic;

namespace Cvte.Compiler.CompileTime
{
    internal class CompileMethod : CompileMember, ICompileMethod
    {
        /// <inheritdoc />
        public CompileMethod(IEnumerable<ICompileAttribute> attributes, string name) : base(name, attributes)
        {
        }
    }
}