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
    }

    internal abstract class PlaceholderInfo
    {
        protected PlaceholderInfo(TextSpan span)
        {
            Span = span;
        }

        /// <summary>
        /// 获取占位符在源代码文件中的文本区间。
        /// </summary>
        public TextSpan Span { get; }

        public string Execute(CompilingContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
