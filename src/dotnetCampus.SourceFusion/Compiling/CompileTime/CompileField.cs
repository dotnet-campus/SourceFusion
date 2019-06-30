using System.Collections.Generic;
using dotnetCampus.SourceFusion.CompileTime;

namespace dotnetCampus.Compiling.CompileTime
{
    internal class CompileField : CompileMember, ICompileField
    {
        /// <inheritdoc />
        public CompileField(string name, IEnumerable<ICompileAttribute> attributes) : base(name, attributes)
        {
        }
    }
}