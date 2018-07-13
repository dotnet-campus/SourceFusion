using System;

namespace Cvte.Compiler
{
    /// <summary>
    /// 表示一个类是一个类型模板，类中的占位符将在编译后被替换为占位符内实际计算出的代码。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CompileTimeTemplateAttribute : Attribute
    {
    }
}
