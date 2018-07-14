using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cvte.Compiler.Syntax
{
    internal class PlaceholderVisitor : CSharpSyntaxRewriter
    {
        internal IReadOnlyCollection<PlaceholderInfo> Placeholders => _placeholders;

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

        internal class PlaceholderInfo
        {
            public PlaceholderInfo(TextSpan span, string methodName,
                string invocationParameterName, string invocationBody)
            {
                Span = span;
                MethodName = methodName;
                InvocationParameterName = invocationParameterName;
                InvocationBody = invocationBody;
            }

            public string MethodName { get; }

            public string InvocationParameterName { get; }

            public string InvocationBody { get; }

            public TextSpan Span { get; }
        }
    }
}
