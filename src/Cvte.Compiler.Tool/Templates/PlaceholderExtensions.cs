using System;
using System.Linq;
using System.Text;
using Cvte.Compiler.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Cvte.Compiler.Syntax.PlaceholderVisitor;

namespace Cvte.Compiler.Templates
{
    /// <summary>
    /// 为 <see cref="PlaceholderInfo"/> 提供扩展方法。
    /// </summary>
    internal static class PlaceholderExtensions
    {
        private const string ClassTemplate = @"using System.Linq;
using Cvte.Compiler;
using Cvte.Compiler.CompileTime;

public static class PlaceholderImpl
{
    public static string InvokePlaceholder(ICompilingContext {parameterName})
    {body}
}
";

        /// <summary>
        /// 将占位符中的在编译期执行的 Lambda 表达式编译成可执行函数。
        /// </summary>
        /// <param name="placeholderInfo">包含表达式树中解析出来的占位符信息。</param>
        /// <returns>用于调用占位符中编译期可执行代码的委托。</returns>
        private static Func<ICompilingContext, string> Compile(this PlaceholderInfo placeholderInfo)
        {
            var builder = new StringBuilder(ClassTemplate)
                .Replace("{parameterName}", placeholderInfo.InvocationParameterName)
                .Replace("{body}", placeholderInfo.InvocationBody);
            var syntaxTree = CSharpSyntaxTree.ParseText(builder.ToString());
            var types = syntaxTree.Compile("PlaceholderInvoking.g");
            var placeholderImpl = types.First(x => x.Name == "PlaceholderImpl");
            var method = placeholderImpl.GetMethod("InvokePlaceholder");
            var func = (Func<ICompilingContext, string>) method.CreateDelegate(typeof(Func<ICompilingContext, string>));
            return func;
        }

        /// <summary>
        /// 根据占位符中调用的 <see cref="Placeholder"/> 中方法的不同，进行不同的执行结果包装。
        /// </summary>
        /// <param name="placeholderInfo">包含表达式树中解析出来的占位符信息。</param>
        /// <param name="text">占位符中的编译期代码执行后得到的代码片段字符串。</param>
        /// <returns>被 <see cref="Placeholder"/> 中的 extern 方法执行后的实际返回代码片段字符串。</returns>
        private static string Wrap(this PlaceholderInfo placeholderInfo, string text)
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

        /// <summary>
        /// 执行占位符对 <see cref="Placeholder"/> 中外部方法（extern）的调用，得到代码片段字符串。
        /// </summary>
        /// <param name="placeholderInfo">包含表达式树中解析出来的占位符信息。</param>
        /// <param name="context">编译期代码执行所需的上下文信息。</param>
        /// <returns>占位符中的编译期代码执行后得到的代码片段字符串。</returns>
        internal static string Execute(this PlaceholderInfo placeholderInfo, ICompilingContext context)
        {
            var lambda = placeholderInfo.Compile();
            var codeSnippet = lambda(context);
            codeSnippet = placeholderInfo.Wrap(codeSnippet);
            return codeSnippet;
        }
    }
}