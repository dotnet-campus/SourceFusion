using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cvte.Compiler.Syntax
{
    internal class ClassDeclaration
    {
        public string Name { get; }
        public string QuilifiedName { get; }
        public IReadOnlyList<string> Attributes { get; set; }

        public ClassDeclaration(string name, string @namespace, IEnumerable<string> attributes)
        {
            Name = name;
            QuilifiedName = $"{@namespace}.{name}";
            Attributes = new ReadOnlyCollection<string>(attributes as IList<string> ?? attributes.ToList());
        }
    }
}
