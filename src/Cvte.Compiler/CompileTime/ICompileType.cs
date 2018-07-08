namespace Cvte.Compiler.CompileTime
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
        ICompileType BaseType { get; }
        /// <summary>
        /// 类型继承的接口
        /// </summary>
        ICompileInterface[] Interfaces { get; }
        /// <summary>
        /// 类型包含的属性
        /// </summary>
        /// <returns></returns>
        ICompileProperty[] GetProperties();
        /// <summary>
        /// 类型的所有成员
        /// </summary>
        /// <returns></returns>
        ICompileMethod[] GetMethods();
        /// <summary>
        /// 类型的字段
        /// </summary>
        /// <returns></returns>
        ICompileField[] GetFields();
    }
}
