using System;
using System.Collections.Generic;

namespace Cvte.Compiler.CompileTime
{
    internal class CompileType : CompileMember, ICompileType
    {
        public CompileType(string name, string @namespace, IEnumerable<ICompileAttribute> attributes)
            : base(name, attributes)
        {
            Namespace = @namespace;
            FullName = $"{@namespace}.{name}";
        }

        public string FullName { get; }
        public string Namespace { get; }
        public ICompileType BaseType => throw new NotImplementedException();
        public ICompileInterface[] Interfaces => throw new NotImplementedException();
        public ICompileProperty[] GetProperties() => throw new NotImplementedException();
        public ICompileMethod[] GetMethods() => throw new NotImplementedException();
        public ICompileField[] GetFields() => throw new NotImplementedException();
    }
}
