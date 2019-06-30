using System.Collections.Generic;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// 编译时的类型
    /// </summary>
    public interface ICompileType : ICompileMember
    {
        /// <summary>
        /// 类型的命名空间
        /// </summary>
        string Namespace { get; }
        /// <summary>
        /// 类型的全名
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// 类型的基类
        /// </summary>
        IReadOnlyList<string> BaseTypeList { get; }
        /// <summary>
        /// 引用的命名空间
        /// </summary>
        IReadOnlyList<string> UsingNamespaceList { get; }

        /// <summary>
        /// 类型包含的属性
        /// </summary>
        IReadOnlyList<ICompileProperty> Properties { get; }

        /// <summary>
        /// 类型的方法
        /// </summary>
        IReadOnlyList<ICompileMethod> Methods { get; }

        /// <summary>
        /// 类型的字段
        /// </summary>
        IReadOnlyList<ICompileField> Fields { get; }
    }
}
