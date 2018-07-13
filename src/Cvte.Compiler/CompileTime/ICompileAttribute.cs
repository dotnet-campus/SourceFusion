using System;

namespace Cvte.Compiler.CompileTime
{
    public interface ICompileAttribute
    {
        string Name { get; }

        /// <summary>
        /// 判断当前<see cref="ICompileAttribute"/>是否符合条件
        /// </summary>
        /// <param name="attributeName">传入的 attributeName 可以带 Attribute 后缀</param>
        /// <returns></returns>
        bool Match(string attributeName);
    }
}
