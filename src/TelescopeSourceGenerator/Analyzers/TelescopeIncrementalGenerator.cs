using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

[Generator(LanguageNames.CSharp)]
public class TelescopeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        Debugger.Launch();
#endif

        // 先读取程序集特性，接着遍历整个程序集的所有代码文件，看看哪些是符合需求的，收集起来
        // 读取程序集特性

        var assemblyAttributeSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (syntaxNode, cancellationToken) =>
            {
                // 预先判断是 assembly 的特性再继续
                // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
                return syntaxNode.IsKind(SyntaxKind.AttributeList) && syntaxNode.ChildNodes()
                    .Any(subNode => subNode.IsKind(SyntaxKind.AttributeTargetSpecifier));
            }, ParseMarkExportAttribute).Where(t => t.Success).Collect();

        // 遍历整个程序集的所有代码文件
        var generatorSyntaxContextIncrementalValuesProvider =
            context.SyntaxProvider.CreateSyntaxProvider
                (
                    (syntaxNode, cancellationToken) => syntaxNode.IsKind(SyntaxKind.ClassDeclaration),
                    (generatorSyntaxContext, cancellationToken) =>
                    {
                        var classDeclarationSyntax = (ClassDeclarationSyntax)generatorSyntaxContext.Node;

                        INamedTypeSymbol namedTypeSymbol =
                            generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

                        if (namedTypeSymbol is not null)
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
                .Where(t => t.Success);

        // 将程序集特性和类型组合一起，看看哪些类型符合程序集特性的要求，将其拼装到一起
        var collectionClass = generatorSyntaxContextIncrementalValuesProvider
            .Combine(assemblyAttributeSyntaxContextIncrementalValuesProvider)
            .Select((tuple, token) =>
            {
                return ParseMarkClassList(tuple.Left, tuple.Right);
            })
            .SelectMany((list, _) => list)
            .Collect();

        // 参考 AttributedTypesExportFileGenerator 逻辑生成代码
        IncrementalValueProvider<string> generationCodeProvider = collectionClass.Select((markClassCollection, token) =>
        {
            var attributedTypesExportGenerator = new ExportedTypesCodeTextGenerator();
            string generationCode = attributedTypesExportGenerator.Generate(markClassCollection, token);
            return generationCode;
        });

        context.RegisterSourceOutput(generationCodeProvider, (sourceProductionContext, generationCode) =>
        {
            sourceProductionContext.AddSource("__AttributedTypesExport__", generationCode);
        });
    }

    /// <summary>
    /// 转换被标记的类型的信息
    /// </summary>
    /// <param name="classParseResult">程序集里面的类型</param>
    /// <param name="markExportAttributeParseResultList">程序集里面的各个标记</param>
    /// <returns></returns>
    private List<MarkClassParseResult> ParseMarkClassList(AssemblyCandidateClassParseResult classParseResult, ImmutableArray<MarkExportAttributeParseResult> markExportAttributeParseResultList)
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
        if (classParseResult.Attributes.Any(t => SymbolEqualityComparer.Default.Equals(t.AttributeClass, markExportAttributeParseResult.AttributeTypeInfo.Type)))
        {
            // 再判断继承类型
            var requiredBaseClassOrInterfaceType = markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo.Type;

            if (IsInherit(classParseResult.ExportedTypeSymbol, requiredBaseClassOrInterfaceType))
            {
                return new MarkClassParseResult(classParseResult, markExportAttributeParseResult);
            }
        }

        return null;

        // 判断类型继承关系
        static bool IsInherit(ITypeSymbol currentType, ITypeSymbol requiredType)
        {
            var baseType = currentType.BaseType;
            while (baseType is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(baseType, requiredType))
                {
                    // 如果基类型是的话
                    return true;
                }

                // 否则继续找基类型
                baseType = baseType.BaseType;
            }

            foreach (var currentInheritInterfaceType in currentType.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(currentInheritInterfaceType, requiredType))
                {
                    // 如果继承的类型是的话
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 解析出定义在程序集里面的特性
    /// </summary>
    /// <param name="generatorSyntaxContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private MarkExportAttributeParseResult ParseMarkExportAttribute(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        if (generatorSyntaxContext.Node is not AttributeListSyntax attributeListSyntax)
        {
            return ReturnFalse();
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
                            return new MarkExportAttributeParseResult(true, baseClassOrInterfaceTypeInfo.Value,
                                attributeTypeInfo.Value);
                        }

                        TypeInfo? GetTypeOfType(AttributeArgumentSyntax attributeArgumentSyntax)
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

        return ReturnFalse();

        MarkExportAttributeParseResult ReturnFalse() => new MarkExportAttributeParseResult(false, default, default);
    }
}