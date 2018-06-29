using System;

namespace Cvte.Compiler.CompileTime
{
    public class CompileAttribute : ICompileAttribute
    {
        public CompileAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool Match<TAttribute>() where TAttribute : Attribute
        {
            var attributeName = typeof(TAttribute).Name;
            if (Name == attributeName)
            {
                return true;
            }

            var index = attributeName.LastIndexOf("Attribute", StringComparison.InvariantCulture);
            if (index >= 0)
            {
                attributeName = attributeName.Substring(0, index);
            }

            return Name == attributeName;
        }
    }
}
