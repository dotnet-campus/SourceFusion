using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dotnetCampus.SourceFusion.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.SourceFusion.Templates
{
    /// <summary>
    /// 包含 <see cref="Placeholder.Array{T}"/> 占位符在语法树中的信息。
    /// </summary>
    internal class ArrayPlaceholder : PlaceholderInfo
    {
        /// <summary>
        /// 初始化 <see cref="ArrayPlaceholder"/> 的新实例。
        /// </summary>
        /// <param name="span">占位符在源代码文件中的文本区间。</param>
        /// <param name="methodName">此占位符调用的 <see cref="Placeholder"/> 类型中的方法名称。</param>
        /// <param name="invocationParameterName">占位符中需要在编译期间执行的方法参数名称。</param>
        /// <param name="invocationBody">占位符中需要在编译期间执行的方法体。</param>
        /// <param name="returnType">占位符中需要返回集合的返回值类型。</param>
        internal ArrayPlaceholder(TextSpan span, string methodName,
            string invocationParameterName, string invocationBody, string returnType)
            : base(span)
        {
            MethodName = methodName;
            InvocationParameterName = invocationParameterName;
            InvocationBody = invocationBody;
            ReturnType = returnType;
        }

        /// <summary>
        /// 获取源代码文件中占位符调用的是 <see cref="Placeholder"/> 类型中的哪个方法。
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// 获取占位符中需要在编译期间执行的方法参数名称。
        /// </summary>
        public string InvocationParameterName { get; }

        /// <summary>
        /// 获取占位符中需要在编译期间执行的方法体。
        /// </summary>
        public string InvocationBody { get; }

        /// <summary>
        /// 获取占位符的返回值单项类型。
        /// </summary>
        public string ReturnType { get; }

        public override string Fill(CompilingContext context)
        {
            var lambda = Compile(context);
            var codeSnippet = lambda(context);
            codeSnippet = $@"new {ReturnType}[]
{{
{codeSnippet}}}";
            return codeSnippet;
        }

        private const string ClassTemplate = @"using System;
using System.Linq;
using dotnetCampus.SourceFusion;
using dotnetCampus.SourceFusion.CompileTime;

public static class PlaceholderImpl
{
    public static string InvokePlaceholder(ICompilingContext {parameterName})
    {body}
}
";

        /// <summary>
        /// 将占位符中的在编译期执行的 Lambda 表达式编译成可执行函数。
        /// </summary>
        /// <returns>用于调用占位符中编译期可执行代码的委托。</returns>
        private Func<ICompilingContext, string> Compile(CompilingContext context)
        {
            var builder = new StringBuilder(ClassTemplate)
                .Replace("{parameterName}", InvocationParameterName)
                .Replace("{body}", InvocationBody);
            var syntaxTree = CSharpSyntaxTree.ParseText(builder.ToString());
            var types = syntaxTree.Compile(context.References, "PlaceholderInvoking.g");
            var placeholderImpl = types.First(x => x.Name == "PlaceholderImpl");
            var method = placeholderImpl.GetMethod("InvokePlaceholder");
            Debug.Assert(method != null, nameof(method) + " != null");
            var func = (Func<ICompilingContext, string>) method.CreateDelegate(typeof(Func<ICompilingContext, string>));
            return func;
        }
    }
}
