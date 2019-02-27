using System.Collections.Generic;
using System.Linq;
using dotnetCampus.SourceFusion.Core;
using dotnetCampus.SourceFusion.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

        /// <summary>
        /// 在调用 <see cref="CSharpSyntaxRewriter.Visit"/> 方法后，可通过此属性获取此文件的所有命名空间 using 信息。
        /// </summary>
        internal IReadOnlyCollection<(string @namespace, TextSpan span)> Namespaces => _namespaces;

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                // 取出 Placeholder.MethodName 片段。
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
                            _placeholders.Add(new AttributedTypesPlaceholder(textSpan, baseType, attributeType,
                                _isNextPlaceholderAssigned));

                            break;
                        }
                        default:
                            throw new CompilingException($"Placeholder 中不包含名为 {methodName} 的方法。");
                    }

                    _isNextPlaceholderAssigned = false;
                }
            }

            return base.VisitInvocationExpression(node);
        }

        public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            if (node.Value is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax memberAccessExpression
                && memberAccessExpression.Expression.ToString() == nameof(Placeholder))
            {
                // 当这个赋值操作的等号右边是 Placeholder 的时候，标记此时正在使用 Placeholder 赋值。
                _isNextPlaceholderAssigned = true;
            }

            return base.VisitEqualsValueClause(node);
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _namespaces.Add((node.ToString().Replace("using ", "").Replace(";", "").Trim(), node.Span));
            return base.VisitUsingDirective(node);
        }

        /// <summary>
        /// 在 <see cref="PlaceholderVisitor"/> 内部使用，用于在语法树访问期间添加占位符信息。
        /// </summary>
        private readonly List<PlaceholderInfo> _placeholders = new List<PlaceholderInfo>();

        /// <summary>
        /// 在 <see cref="PlaceholderVisitor"/> 内部使用，用于修改命名空间信息。
        /// </summary>
        private readonly List<(string @namespace, TextSpan span)> _namespaces = new List<(string, TextSpan)>();

        /// <summary>
        /// 在查找的过程中，如果发现了一个 <see cref="Placeholder"/> 被赋值给了某个对象或者被用作其他用途，那么就进行标记。
        /// 这样，在实际 <see cref="Placeholder"/> 的解析过程中就可以使用到这个值进行某些特殊的判断。
        /// </summary>
        private bool _isNextPlaceholderAssigned;
    }
}
