using System;

namespace Cvte.Compiler.CompileTime
{
    public class CompileMethod : ICompileMethod
    {
        /// <inheritdoc />
        public CompileMethod(ICompileAttribute[] attributes, string name)
        {
            if (ReferenceEquals(attributes, null)) throw new ArgumentNullException(nameof(attributes));
            if (ReferenceEquals(name, null)) throw new ArgumentNullException(nameof(name));
            Attributes = attributes;
            Name = name;
        }

        /// <inheritdoc />
        public ICompileAttribute[] Attributes { get; }

        /// <inheritdoc />
        public string Name { get; }
    }
}