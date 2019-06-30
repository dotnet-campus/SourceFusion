using System.Collections.Generic;
using dotnetCampus.SourceFusion.CompileTime;

namespace dotnetCampus.Compiling.CompileTime
{
    internal class CompileMethod : CompileMember, ICompileMethod
    {
        /// <inheritdoc />
        public CompileMethod(IEnumerable<ICompileAttribute> attributes, string name) : base(name, attributes)
        {
        }
    }
}