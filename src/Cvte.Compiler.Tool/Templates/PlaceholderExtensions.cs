using System;
using Cvte.Compiler.Syntax;

namespace Cvte.Compiler.Templates
{
    /// <summary>
    /// 为 <see cref="PlaceholderVisitor.PlaceholderInfo"/> 提供扩展方法。
    /// </summary>
    internal static class PlaceholderExtensions
    {
        /// <summary>
        /// 将占位符中的在编译期执行的 Lambda 表达式编译成可执行函数。
        /// </summary>
        /// <param name="placeholderInfo">包含表达式树中解析出来的占位符信息。</param>
        /// <returns>用于调用占位符中编译期可执行代码的委托。</returns>
        internal static Func<ICompilingContext, string> Compile(this PlaceholderVisitor.PlaceholderInfo placeholderInfo)
        {
            return context => @"new ModuleInfo<Cvte.Compiler.Tests.Fakes.Modules.FooModule>(),
new ModuleInfo<Cvte.Compiler.Tests.Fakes.Modules.BarModule>(),
";
        }

        /// <summary>
        /// 根据占位符中调用的 <see cref="Placeholder"/> 中方法的不同，进行不同的执行结果包装。
        /// </summary>
        /// <param name="placeholderInfo">包含表达式树中解析出来的占位符信息。</param>
        /// <param name="text">占位符中的编译期代码执行后得到的代码片段字符串。</param>
        /// <returns>被 <see cref="Placeholder"/> 中的 extern 方法执行后的实际返回代码片段字符串。</returns>
        internal static string Wrap(this PlaceholderVisitor.PlaceholderInfo placeholderInfo, string text)
        {
            switch (placeholderInfo.MethodName)
            {
                case nameof(Placeholder.Array):
                    return $@"
{{
{text}}}";
                default:
                    throw new MissingMethodException($"{nameof(Placeholder)} 中不包含名为 {placeholderInfo.MethodName} 的方法。");
            }
        }
    }
}
