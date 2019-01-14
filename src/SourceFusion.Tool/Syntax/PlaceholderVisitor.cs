using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using dotnetCampus.SourceFusion.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.SourceFusion.Syntax
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

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                // 取出 Placeholder.Array 片段。
                var expression = memberAccessExpression.Expression.ToString();
                if (expression == nameof(Placeholder) && memberAccessExpression.Name is GenericNameSyntax genericName)
                {
                    var textSpan = node.FullSpan;

                    // Placeholder 已确认，调用泛型方法已确认。
                    var methodName = genericName.Identifier.ToString();

                    switch (methodName)
                    {
                        case nameof(Placeholder.Array):
                        {
                            var returnType = ((IdentifierNameSyntax) genericName.TypeArgumentList.Arguments.First())
                                .Identifier.ToString();
                            if (node.ArgumentList.Arguments.FirstOrDefault()?.Expression
                                is SimpleLambdaExpressionSyntax lambdaExpression)
                            {
                                // 参数列表为 Lambda 表达式已确认。
                                var parameter = lambdaExpression.Parameter.Identifier.ToString();
                                var body = lambdaExpression.Body.ToString();
                                _placeholders.Add(new ArrayPlaceholder(textSpan, methodName, parameter, body, returnType));
                            }

                            break;
                        }
                        case nameof(Placeholder.AttributedTypes):
                        {
                            var genericTypes = genericName.TypeArgumentList.Arguments
                                .Select(x => x.ToString()).ToList();
                            // 这里的 2 是 AttributedTypes 方法的两个泛型参数，只要不是两个泛型参数就是错误。
                            if (genericTypes.Count != 2)
                            {
                                throw new CompilingException("Placeholder.AttributedTypes<T, Attribute> 必须有两个类型参数。");
                            }

                            var baseType = genericTypes[0];
                            var attributeType = genericTypes[1];
                            _placeholders.Add(new AttributedTypesPlaceholder(textSpan, baseType, attributeType));

                            break;
                        }
                        default:
                            throw new CompilingException($"Placeholder 中不包含名为 {methodName} 的方法。");
                    }
                }
            }

            return base.VisitInvocationExpression(node);
        }


        /// <summary>
        /// 在 <see cref="PlaceholderVisitor"/> 内部使用，用于在语法树访问期间添加占位符信息。
        /// </summary>
        private readonly List<PlaceholderInfo> _placeholders = new List<PlaceholderInfo>();
    }
}
