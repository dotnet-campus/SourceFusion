using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

[Generator(LanguageNames.CSharp)]
public class TelescopeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 先读取程序集特性，接着遍历整个程序集的所有代码文件，看看哪些是符合需求的，收集起来
        // 读取程序集特性

        var assemblyAttributeSyntaxContextIncrementalValuesProvider =
            context.SyntaxProvider.CreateSyntaxProvider
                (
                    // 语法分析，过滤只有是程序集特性
                    (syntaxNode, cancellationToken) =>
                    {
                        // 预先判断是 assembly 的特性再继续
                        // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
                        return syntaxNode.IsKind(SyntaxKind.AttributeList) && syntaxNode.ChildNodes()
                            .Any(subNode => subNode.IsKind(SyntaxKind.AttributeTargetSpecifier));
                    },
                    // 获取只有是属于程序集标记的特性才使用
                    ParseMarkExportAttribute
                )
                .Where(t => t.Success)
                .Collect();

        // 遍历整个程序集的所有代码文件
        // 获取出所有标记了特性的类型，用来在下一步判断是否属于导出的类型
        var assemblyClassIncrementalValuesProvider =
            context.SyntaxProvider.CreateSyntaxProvider
                (
                    // 语法分析，只有是 class 类型定义的才可能满足需求
                    (syntaxNode, cancellationToken) => syntaxNode.IsKind(SyntaxKind.ClassDeclaration),

                    // 加上语义分析，了解当前的类型是否有添加任何标记
                    (generatorSyntaxContext, cancellationToken) =>
                    {
                        var classDeclarationSyntax = (ClassDeclarationSyntax) generatorSyntaxContext.Node;

                        // 从语法转换为语义，用于后续判断是否标记了特性
                        INamedTypeSymbol? namedTypeSymbol =
                            generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

                        // 如果可以获取到语义的类型，则尝试获取其标记的特性
                        if (namedTypeSymbol is not null 
                            // 抽象类不应该被加入创建
                            && !namedTypeSymbol.IsAbstract)
                        {
                            var attributes = namedTypeSymbol.GetAttributes();

                            if (attributes.Length > 0)
                            {
                                return new AssemblyCandidateClassParseResult(namedTypeSymbol, attributes,
                                    generatorSyntaxContext);
                            }
                            else
                            {
                                // 只有标记了特性的，才是可能候选的类型
                            }
                        }

                        return new AssemblyCandidateClassParseResult();
                    }
                )
                // 过滤掉不符合条件的类型
                .Where(t => t.Success);

        // 将程序集特性和类型组合一起，看看哪些类型符合程序集特性的要求，将其拼装到一起
        IncrementalValueProvider<ImmutableArray<MarkClassParseResult>> collectionClass =
            assemblyClassIncrementalValuesProvider
                .Combine(assemblyAttributeSyntaxContextIncrementalValuesProvider)
                .Select((tuple, token) => { return ParseMarkClassList(tuple.Left, tuple.Right); })
                .SelectMany((list, _) => list)
                .Collect();

        // 参考 AttributedTypesExportFileGenerator 逻辑生成代码
        IncrementalValueProvider<string> generatedCodeProvider = collectionClass.Select((markClassCollection, token) =>
        {
            var attributedTypesExportGenerator = new ExportedTypesCodeTextGenerator();
            string generatedCode = attributedTypesExportGenerator.Generate(markClassCollection, token);
            return generatedCode;
        });

        // 注册到输出
        context.RegisterSourceOutput(generatedCodeProvider,
            (sourceProductionContext, generatedCode) =>
            {
                if(!string.IsNullOrEmpty(generatedCode))
                {
                    sourceProductionContext.AddSource("__AttributedTypesExport__", generatedCode);
                }
            });
    }

    /// <summary>
    /// 转换被标记的类型的信息
    /// </summary>
    /// <param name="classParseResult">程序集里面的类型</param>
    /// <param name="markExportAttributeParseResultList">程序集里面的各个标记</param>
    /// <returns></returns>
    private List<MarkClassParseResult> ParseMarkClassList(AssemblyCandidateClassParseResult classParseResult,
        ImmutableArray<MarkExportAttributeParseResult> markExportAttributeParseResultList)
    {
        var list = new List<MarkClassParseResult>();
        foreach (MarkExportAttributeParseResult markExportAttributeParseResult in markExportAttributeParseResultList)
        {
            var result = ParseMarkClass(classParseResult, markExportAttributeParseResult);
            if (result != null)
            {
                list.Add(result.Value);
            }
        }

        return list;
    }

    /// <summary>
    /// 转换被标记的类型的信息，判断当前的类型是否满足当前所选的程序集特性
    /// </summary>
    /// <param name="classParseResult"></param>
    /// <param name="markExportAttributeParseResult"></param>
    /// <returns></returns>
    private MarkClassParseResult? ParseMarkClass(AssemblyCandidateClassParseResult classParseResult,
        MarkExportAttributeParseResult markExportAttributeParseResult)
    {
        // 先判断满足的类型
        var matchAssemblyMarkAttributeData = classParseResult.Attributes.FirstOrDefault(t =>
            SymbolEqualityComparer.Default.Equals(t.AttributeClass,
                markExportAttributeParseResult.AttributeTypeInfo));

        if (matchAssemblyMarkAttributeData?.ApplicationSyntaxReference is null)
        {
            // 找不到匹配的特性，表示这个类型不应该被收集
            return default;
        }

        // 同时获取其语法
        AttributeSyntax? markAttributeSyntax = classParseResult
            .ExportedTypeClassDeclarationSyntax
            .AttributeLists
            .SelectMany(t => t.Attributes)
            // 理论上 Span 是相同的，这里用 Contains 或 == 都应该是相同的结果
            .FirstOrDefault(t => t.Span.Contains(matchAssemblyMarkAttributeData.ApplicationSyntaxReference.Span));

        if (markAttributeSyntax is null)
        {
            // 理论上不可能是空，因为已找到其特性
            return default;
        }

        // 再判断继承类型
        var requiredBaseClassOrInterfaceType = markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo;

        if (TypeSymbolHelper.IsInherit(classParseResult.ExportedTypeSymbol, requiredBaseClassOrInterfaceType))
        {
            return new MarkClassParseResult(classParseResult.ExportedTypeSymbol,
                classParseResult.ExportedTypeClassDeclarationSyntax, matchAssemblyMarkAttributeData, markAttributeSyntax,
                markExportAttributeParseResult, classParseResult.GeneratorSyntaxContext);
        }

        return null;
    }

    /// <summary>
    /// 解析出定义在程序集里面的特性
    /// </summary>
    /// <param name="generatorSyntaxContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private MarkExportAttributeParseResult ParseMarkExportAttribute(GeneratorSyntaxContext generatorSyntaxContext,
        CancellationToken cancellationToken)
    {
        if (generatorSyntaxContext.Node is not AttributeListSyntax attributeListSyntax)
        {
            return MarkExportAttributeParseResult.Failure;
        }

        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
            // attributeSyntax：拿到 MarkExport 符号
            // 由于只是拿到 MarkExport 符号，不等于是 `dotnetCampus.Telescope.MarkExportAttribute` 特性，需要走语义分析
            var typeInfo = generatorSyntaxContext.SemanticModel.GetTypeInfo(attributeSyntax);
            if (typeInfo.Type is { } attributeType && attributeSyntax.ArgumentList is not null)
            {
                // 带上 global 格式的输出 FullName 内容
                var symbolDisplayFormat = new SymbolDisplayFormat(
                    // 带上命名空间和类型名
                    SymbolDisplayGlobalNamespaceStyle.Included,
                    // 命名空间之前加上 global 防止冲突
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
                var fullName = attributeType.ToDisplayString(symbolDisplayFormat);

                if (fullName == "global::dotnetCampus.Telescope.MarkExportAttribute")
                {
                    // 这个是符合预期的
                    var attributeArgumentSyntaxList = attributeSyntax.ArgumentList.Arguments;
                    if (attributeArgumentSyntaxList.Count == 2)
                    {
                        var baseClassOrInterfaceTypeSyntax = attributeArgumentSyntaxList[0];
                        var attributeTypeSyntax = attributeArgumentSyntaxList[1];

                        // 原本采用的是 GuessTypeNameByTypeOfSyntax 方式获取的，现在可以通过语义获取
                        var baseClassOrInterfaceTypeInfo = GetTypeInfoFromArgumentTypeOfSyntax(baseClassOrInterfaceTypeSyntax);
                        var attributeTypeInfo = GetTypeInfoFromArgumentTypeOfSyntax(attributeTypeSyntax);

                        if (baseClassOrInterfaceTypeInfo?.Type is not null && attributeTypeInfo?.Type is not null)
                        {
                            return new MarkExportAttributeParseResult(true, baseClassOrInterfaceTypeInfo.Value.Type,
                                attributeTypeInfo.Value.Type);
                        }

                        TypeInfo? GetTypeInfoFromArgumentTypeOfSyntax(AttributeArgumentSyntax attributeArgumentSyntax)
                        {
                            if (attributeArgumentSyntax.Expression is TypeOfExpressionSyntax typeOfExpressionSyntax)
                            {
                                var typeSyntax = typeOfExpressionSyntax.Type;
                                var typeOfType = generatorSyntaxContext.SemanticModel.GetTypeInfo(typeSyntax);
                                return typeOfType;
                            }

                            return null;
                        }
                    }
                }
            }
        }

        return MarkExportAttributeParseResult.Failure;
    }
}