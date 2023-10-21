using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 采用 IEnumerable 的 ValueTuple 返回值类型的生成方法
/// </summary>
class EnumerableValueTupleExportMethodReturnTypeCodeGenerator : IExportMethodCodeGenerator
{
    public string GenerateSourceCode(ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult, List<INamedTypeSymbol> list,
        CancellationToken token)
    {
        /*
           private static partial IEnumerable<(Type, F1Attribute xx, Func<DemoLib1.F1> xxx)> ExportFooEnumerable()
           {
               yield return (typeof(CurrentFoo), new F1Attribute(), () => new CurrentFoo());
           }
        */
        if (exportMethodReturnTypeCollectionResult.ExportMethodReturnType !=
            ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator)
        {
            throw new ArgumentException($"调用错误，其他返回值类型不应该调用");
        }

        var methodCode = new StringBuilder();

        foreach (var namedTypeSymbol in list)
        {
            token.ThrowIfCancellationRequested();

            if (exportMethodReturnTypeCollectionResult.ExpectedClassAttributeType is null)
            {
                // 这是不带 Attribute 的收集
                // 以下生成格式大概如下的代码
                // yield return (typeof(CurrentFoo), () => new CurrentFoo());
                var typeName = TypeSymbolHelper.TypeSymbolToFullName(namedTypeSymbol);
                methodCode.AppendLine(SourceCodeGeneratorHelper.IndentSource(
                    $"    yield return (typeof({typeName}), () => new {typeName}());",
                    numIndentations: 1));
            }
            else
            {
                // 以下生成格式大概如下的代码
                // yield return (typeof(CurrentFoo), new F1Attribute(), () => new CurrentFoo());
                var attribute = namedTypeSymbol.GetAttributes().First(t =>
                    SymbolEqualityComparer.Default.Equals(t.AttributeClass,
                        exportMethodReturnTypeCollectionResult
                            .ExpectedClassAttributeType));
                var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(attribute);

                var typeName = TypeSymbolHelper.TypeSymbolToFullName(namedTypeSymbol);
                methodCode.AppendLine(SourceCodeGeneratorHelper.IndentSource(
                    $"    yield return (typeof({typeName}), {attributeCreatedCode}, () => new {typeName}());",
                    numIndentations: 1));
            }
        }

        var methodSource = SourceCodeGeneratorHelper.GeneratePartialMethodCode(
            exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol, methodCode.ToString(), token);

        return methodSource;
    }
}