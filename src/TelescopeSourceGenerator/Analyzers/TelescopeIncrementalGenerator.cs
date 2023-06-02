using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

[Generator(LanguageNames.CSharp)]
public class TelescopeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();

        // 先读取程序集特性，接着遍历整个程序集的所有代码文件，看看哪些是符合需求的，收集起来
        // 读取程序集特性
        var assemblyAttributeSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, cancellationToken) =>
        {
            // 预先判断是 assembly 的特性再继续
            // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
            return syntaxNode.IsKind(SyntaxKind.AttributeList) && syntaxNode.ChildNodes().Any(subNode=> subNode.IsKind(SyntaxKind.AttributeTargetSpecifier));
        }, (generatorSyntaxContext, cancellationToken) =>
        {
            if (generatorSyntaxContext.Node is AttributeListSyntax attributeListSyntax)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
                    // attributeSyntax：拿到 MarkExport 符号
                    // 由于只是拿到 MarkExport 符号，不等于是 `dotnetCampus.Telescope.MarkExportAttribute` 特性，需要走语义分析
                    var typeInfo = generatorSyntaxContext.SemanticModel.GetTypeInfo(attributeSyntax);
                    if (typeInfo.Type is {} attributeType)
                    {
                        // 带上 global 格式的输出 FullName 内容
                        var symbolDisplayFormat = new SymbolDisplayFormat
                        (
                            // 带上命名空间和类型名
                            SymbolDisplayGlobalNamespaceStyle.Included,
                            // 命名空间之前加上 global 防止冲突
                            SymbolDisplayTypeQualificationStyle
                                .NameAndContainingTypesAndNamespaces
                        );
                        var fullName = attributeType.ToDisplayString(symbolDisplayFormat);

                        if (fullName == "global::dotnetCampus.Telescope.MarkExportAttribute")
                        {
                            // 这个是符合预期的
                        }
                    }
                }
            }
            else
            {
                // 理论上不可能，因为前置判断过了
            }

            return generatorSyntaxContext;
        }).Collect();

        // 遍历整个程序集的所有代码文件
        IncrementalValuesProvider<GeneratorSyntaxContext> generatorSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, cancellationToken) => syntaxNode.IsKind(SyntaxKind.ClassDeclaration), (generatorSyntaxContext, cancellationToken) =>
        {
            return generatorSyntaxContext;
        });

        var collect = generatorSyntaxContextIncrementalValuesProvider.Combine(assemblyAttributeSyntaxContextIncrementalValuesProvider).Collect();

        context.RegisterSourceOutput(collect, (sourceProductionContext, generatorSyntaxContext) =>
        {
            foreach (var syntaxContext in generatorSyntaxContext)
            {
                //var model = syntaxContext.SemanticModel;
            }
        });
    }
}

class DelegateE<T> : IEqualityComparer<T>
{
    public bool Equals(T x, T y)
    {
        throw new System.NotImplementedException();
    }

    public int GetHashCode(T obj)
    {
        throw new System.NotImplementedException();
    }
}