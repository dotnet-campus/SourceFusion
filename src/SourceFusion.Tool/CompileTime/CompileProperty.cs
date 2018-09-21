using System;

namespace dotnetCampus.SourceFusion.CompileTime
{
    internal class CompileProperty : CompileMember, ICompileProperty
    {
        /// <inheritdoc />
        public CompileProperty(string type, ICompileAttribute[] attributes, string name) : base(name, attributes)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Type { get; }

        public ICompileMethod GetMethod { get; set; }

        public ICompileMethod SetMethod { get; set; }
    }
}