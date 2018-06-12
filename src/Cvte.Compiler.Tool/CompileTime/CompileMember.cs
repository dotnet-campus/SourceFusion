using System.Collections.Generic;
using System.Linq;

namespace Cvte.Compiler.CompileTime
{
    internal abstract class CompileMember : ICompileMember
    {
        protected CompileMember(string name, IEnumerable<ICompileAttribute> attributes)
        {
            Name = name;
            Attributes = attributes.ToArray();
        }

        public string Name { get; }
        public ICompileAttribute[] Attributes { get; }
    }
}
