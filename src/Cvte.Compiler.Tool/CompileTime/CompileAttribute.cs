using System;

namespace Cvte.Compiler.CompileTime
{
    /// <summary>
    /// 编译时的特性
    /// </summary>
    public class CompileAttribute : ICompileAttribute
    {
        /// <summary>
        /// 创建编译时的特性
        /// </summary>
        /// <param name="name"></param>
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

        /// <inheritdoc />
        public bool Match(string attributeName)
        {
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
