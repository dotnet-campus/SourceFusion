using System.Collections.Generic;
using dotnetCampus.SourceFusion.CompileTime;

namespace dotnetCampus.SourceFusion.Core
{
    /// <summary>
    /// <see cref="T:SourceFusion.ICompilingContext" /> 的基础实现，包含编译期代码执行的上下文。
    /// </summary>
    internal class CompilingContext : ICompilingContext
    {
        /// <summary>
        /// 初始化 <see cref="CompilingContext"/> 的新实例。
        /// </summary>
        /// <param name="assembly">当前程序集的编译期信息。</param>
        public CompilingContext(CompileAssembly assembly)
        {
            _assembly = assembly;
        }

        /// <summary>
        /// 获取当前程序集的编译期信息。
        /// </summary>
        public ICompileAssembly Assembly => _assembly;

        public IReadOnlyList<string> References => _assembly.References;

        private readonly CompileAssembly _assembly;
    }
}
