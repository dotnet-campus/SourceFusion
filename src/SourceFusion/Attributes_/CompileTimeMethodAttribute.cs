using System;

namespace dotnetCampus.SourceFusion
{
    /// <inheritdoc />
    /// <summary>
    /// 标记方法为编译期间才执行的方法，此方法在编译后将移除。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class CompileTimeMethodAttribute : Attribute
    {
    }
}
