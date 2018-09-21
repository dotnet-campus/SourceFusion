using System.Collections.Generic;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// 编译时找到的类型
    /// </summary>
    internal class CompileType : CompileMember, ICompileType
    {
        /// <summary>
        /// 创建编译时找到的类型
        /// </summary>
        /// <param name="name">类的名字</param>
        /// <param name="namespace">类的命名空间</param>
        /// <param name="attributes">类的特性</param>
        /// <param name="baseTypeList"></param>
        /// <param name="usingNamespaceList"></param>
        public CompileType(string name, string @namespace, IEnumerable<ICompileAttribute> attributes,
            List<string> baseTypeList, List<string> usingNamespaceList)
            : base(name, attributes)
        {
            Namespace = @namespace;
            BaseTypeList = baseTypeList;
            UsingNamespaceList = usingNamespaceList;
            FullName = $"{@namespace}.{name}";
        }

        public void AddCompileProperty(ICompileProperty property)
        {
            _properties.Add(property);
        }

        public void AddCompileMethod(ICompileMethod member)
        {
            _methods.Add(member);
        }

        public void AddCompileField(ICompileField field)
        {
            _fields.Add(field);
        }

        /// <inheritdoc />
        /// <summary>
        /// 获取类的全名
        /// </summary>
        public string FullName { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> BaseTypeList { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> UsingNamespaceList { get; }

        /// <inheritdoc />
        public IReadOnlyList<ICompileProperty> Properties => _properties;

        /// <inheritdoc />
        public IReadOnlyList<ICompileMethod> Methods => _methods;

        /// <inheritdoc />
        public IReadOnlyList<ICompileField> Fields => _fields;

        /// <summary>
        /// 类型的命名空间
        /// </summary>
        public string Namespace { get; }

        private readonly List<ICompileField> _fields = new List<ICompileField>();
        private readonly List<ICompileMethod> _methods = new List<ICompileMethod>();

        private readonly List<ICompileProperty> _properties = new List<ICompileProperty>();
    }
}