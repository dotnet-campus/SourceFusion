using System;

namespace Cvte.Compiler.CompileTime
{
    internal class CompileProperty: ICompileProperty
    {
        /// <inheritdoc />
        public CompileProperty(string type, ICompileAttribute[] attributes , string name )
        {
            if (ReferenceEquals(attributes, null)) throw new ArgumentNullException(nameof(attributes));
            if (ReferenceEquals(name, null)) throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Attributes = attributes;
            Name = name;
        }

        public string Type { get;  }

        /// <inheritdoc />
        public ICompileAttribute[] Attributes { get; }

        public ICompileMethod GetMethod { get; set; }

        public ICompileMethod SetMethod { get; set; }

        /// <inheritdoc />
        public string Name { get; }
    }
}