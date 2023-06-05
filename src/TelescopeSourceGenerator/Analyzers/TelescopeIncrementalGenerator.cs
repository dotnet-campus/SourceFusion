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
        var assemblyAttributeSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (syntaxNode, cancellationToken) =>
            {
                // 预先判断是 assembly 的特性再继续
                // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
                return syntaxNode.IsKind(SyntaxKind.AttributeList) && syntaxNode.ChildNodes()
                    .Any(subNode => subNode.IsKind(SyntaxKind.AttributeTargetSpecifier));
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
                        if (typeInfo.Type is { } attributeType && attributeSyntax.ArgumentList is not null)
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
                                var attributeArgumentSyntaxes = attributeSyntax.ArgumentList.Arguments;
                                if (attributeArgumentSyntaxes.Count == 2)
                                {
                                    var baseClassOrInterfaceTypeSyntaxes = attributeArgumentSyntaxes[0];
                                    var attributeTypeSyntaxes = attributeArgumentSyntaxes[1];

                                    // 原本采用的是 GuessTypeNameByTypeOfSyntax 方式获取的，现在可以通过语义获取
                                    var baseClassOrInterfaceTypeInfo = GetTypeOfType(baseClassOrInterfaceTypeSyntaxes);
                                    var attributeTypeInfo = GetTypeOfType(attributeTypeSyntaxes);

                                    if (baseClassOrInterfaceTypeInfo is not null && attributeTypeInfo is not null)
                                    {
                                        return new MarkExportAttributeParseResult(true,
                                            baseClassOrInterfaceTypeInfo.Value, attributeTypeInfo.Value);
                                    }

                                    TypeInfo? GetTypeOfType(AttributeArgumentSyntax attributeArgumentSyntax)
                                    {
                                        if (attributeArgumentSyntax.Expression is TypeOfExpressionSyntax
                                            typeOfExpressionSyntax)
                                        {
                                            var typeSyntax = typeOfExpressionSyntax.Type;
                                            var typeOfType =
                                                generatorSyntaxContext.SemanticModel.GetTypeInfo(typeSyntax);
                                            return typeOfType;
                                        }

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 理论上不可能，因为前置判断过了
                }

                return new MarkExportAttributeParseResult(false, default, default);
            }).Where(t => t.Success).Collect();

        // 遍历整个程序集的所有代码文件
        IncrementalValuesProvider<GeneratorSyntaxContext> generatorSyntaxContextIncrementalValuesProvider =
            context.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, cancellationToken) => syntaxNode.IsKind(SyntaxKind.ClassDeclaration),
                (generatorSyntaxContext, cancellationToken) => { return generatorSyntaxContext; });

        var collect = generatorSyntaxContextIncrementalValuesProvider
            .Combine(assemblyAttributeSyntaxContextIncrementalValuesProvider).Collect();

        context.RegisterSourceOutput(collect, (sourceProductionContext, generatorSyntaxContext) =>
        {
            foreach (var syntaxContext in generatorSyntaxContext)
            {
                //var model = syntaxContext.SemanticModel;
            }
        });
    }
}

readonly struct MarkExportAttributeParseResult
{
    public MarkExportAttributeParseResult(bool success, TypeInfo baseClassOrInterfaceTypeInfo,
        TypeInfo attributeTypeInfo)
    {
        Success = success;
        BaseClassOrInterfaceTypeInfo = baseClassOrInterfaceTypeInfo;
        AttributeTypeInfo = attributeTypeInfo;
    }

    public bool Success { get; }
    public TypeInfo BaseClassOrInterfaceTypeInfo { get; }

    public TypeInfo AttributeTypeInfo { get; }
}