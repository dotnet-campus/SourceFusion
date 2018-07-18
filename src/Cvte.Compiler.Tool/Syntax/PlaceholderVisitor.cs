using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cvte.Compiler.Syntax
{
    /// <summary>
    /// 包含从语法树中解析 <see cref="Placeholder"/> 类型调用的语法树访问者。
    /// </summary>
    internal class PlaceholderVisitor : CSharpSyntaxRewriter
    {
        /// <summary>
        /// 在调用 <see cref="CSharpSyntaxRewriter.Visit"/> 方法后，可通过此属性获取从语法树中解析得到的占位符（<see cref="Placeholder"/>）信息。
        /// </summary>
        internal IReadOnlyCollection<PlaceholderInfo> Placeholders => _placeholders;

        /// <summary>
        /// 在 <see cref="PlaceholderVisitor"/> 内部使用，用于在语法树访问期间添加占位符信息。
        /// </summary>
        private readonly List<PlaceholderInfo> _placeholders = new List<PlaceholderInfo>();

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                // 取出 Placeholder.Array 片段。
                var expression = memberAccessExpression.Expression.ToString();
                if (expression == nameof(Placeholder) && memberAccessExpression.Name is GenericNameSyntax genericName)
                {
                    // Placeholder 已确认，调用泛型方法已确认。
                    var methodName = genericName.Identifier.ToString();
                    if (node.ArgumentList.Arguments.FirstOrDefault()?.Expression
                        is SimpleLambdaExpressionSyntax lambdaExpression)
                    {
                        // 参数列表为 Lambda 表达式已确认。
                        var parameter = lambdaExpression.Parameter.Identifier.ToString();
                        var body = lambdaExpression.Body.ToString();
                        _placeholders.Add(new PlaceholderInfo(node.FullSpan, methodName, parameter, body));
                    }
                }
            }

            return base.VisitInvocationExpression(node);
        }

        /// <summary>
        /// 包含占位符在语法树中的信息。
        /// </summary>
        internal class PlaceholderInfo
        {
            /// <summary>
            /// 初始化 <see cref="PlaceholderInfo"/> 的新实例。
            /// </summary>
            /// <param name="span">占位符在源代码文件中的文本区间。</param>
            /// <param name="methodName">此占位符调用的 <see cref="Placeholder"/> 类型中的方法名称。</param>
            /// <param name="invocationParameterName">占位符中需要在编译期间执行的方法参数名称。</param>
            /// <param name="invocationBody">占位符中需要在编译期间执行的方法体。</param>
            internal PlaceholderInfo(TextSpan span, string methodName,
                string invocationParameterName, string invocationBody)
            {
                Span = span;
                MethodName = methodName;
                InvocationParameterName = invocationParameterName;
                InvocationBody = invocationBody;
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
            /// 获取占位符在源代码文件中的文本区间。
            /// </summary>
            public TextSpan Span { get; }
        }
    }
}
