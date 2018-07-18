using System.Collections.Generic;

namespace Cvte.Compiler.CompileTime
{
    internal class CompileField : CompileMember, ICompileField
    {
        /// <inheritdoc />
        public CompileField(string name, IEnumerable<ICompileAttribute> attributes) : base(name, attributes)
        {
        }
    }
}