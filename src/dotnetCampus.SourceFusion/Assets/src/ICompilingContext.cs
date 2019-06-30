using dotnetCampus.SourceFusion.CompileTime;

namespace dotnetCampus.SourceFusion
{
    /// <summary>
    /// 包含编译期代码执行的上下文。
    /// </summary>
    internal interface ICompilingContext
    {
        /// <summary>
        /// 获取编译期程序集信息。
        /// </summary>
        ICompileAssembly Assembly { get; }
    }
}
