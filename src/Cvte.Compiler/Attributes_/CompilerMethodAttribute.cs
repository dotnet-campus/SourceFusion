using System;

namespace Cvte.Compiler
{
    /// <inheritdoc />
    /// <summary>
    /// 标记方法为编译期动态实现的方法。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class CompilerMethodAttribute : Attribute
    {
    }
}
