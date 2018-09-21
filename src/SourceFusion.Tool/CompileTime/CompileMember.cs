using System.Collections.Generic;
using System.Linq;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// 编译找到的对象，可以是类型、属性
    /// </summary>
    internal abstract class CompileMember : ICompileMember
    {
        protected CompileMember(string name, IEnumerable<ICompileAttribute> attributes)
        {
            Name = name;
            Attributes = attributes.ToArray();
        }

        /// <summary>
        /// 找到的对象名字
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public MemberModifiers MemberModifiers { set; get; }

        /// <summary>
        /// 对象的特性
        /// </summary>
        public ICompileAttribute[] Attributes { get; }
    }
}