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
        var methodSource = new StringBuilder();

        if (exportMethodReturnTypeCollectionResult.ExportMethodReturnTypeInfo is ValueTupleExportMethodReturnTypeInfo valueTupleExportMethodReturnTypeInfo)
        {
            var exportPartialMethodSymbol = exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol;

            var accessibilityCode =
                exportPartialMethodSymbol.DeclaredAccessibility.ToCSharpCode();
            methodSource.Append(accessibilityCode).Append(' ');

            if (exportPartialMethodSymbol.IsStatic)
            {
                methodSource.Append("static ");
            }

            methodSource.Append("partial ");

            if (!valueTupleExportMethodReturnTypeInfo.IsIEnumerable)
            {
                // 还没支持其他返回值的情况
                throw new NotSupportedException();
            }

            methodSource.Append("global::System.Collections.Generic.IEnumerable<");
            methodSource.Append('(');
            var valueTupleInfo = valueTupleExportMethodReturnTypeInfo.ValueTupleInfo;
            for (var i = 0; i < valueTupleInfo.ItemList.Count; i++)
            {
                var info = valueTupleInfo.ItemList[i];

                if (i != valueTupleInfo.ItemList.Count - 1)
                {
                    var type = TypeSymbolHelper.TypeSymbolToFullName(info.ItemType);
                    methodSource.Append(type).Append(' ');
                    methodSource.Append(info.ItemName);

                    methodSource.Append(',').Append(' ');
                }
                else
                {
                    var type = TypeSymbolHelper.TypeSymbolToFullName(exportMethodReturnTypeCollectionResult
                        .ExpectedClassBaseType);
                    methodSource.Append($"global::System.Func<{type}> {info.ItemName}");
                }
            }

            methodSource.Append(')');
            methodSource.Append('>');
            methodSource.Append(' ');
            methodSource.Append(exportPartialMethodSymbol.Name);
            methodSource.AppendLine("()");
            methodSource.AppendLine("{");

            foreach (var namedTypeSymbol in list)
            {
                token.ThrowIfCancellationRequested();
                // yield return (typeof(CurrentFoo), new F1Attribute(), () => new CurrentFoo());

                var attribute = namedTypeSymbol.GetAttributes().First(t =>
                    SymbolEqualityComparer.Default.Equals(t.AttributeClass,
                        exportMethodReturnTypeCollectionResult
                            .ExpectedClassAttributeType));
                var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(attribute);

                var typeName = TypeSymbolHelper.TypeSymbolToFullName(namedTypeSymbol);
                methodSource.AppendLine(ExportMethodCodeGenerator.IndentSource($"    yield return (typeof({typeName}), {attributeCreatedCode}, () => new {typeName}());",
                    numIndentations: 1));
            }
            methodSource.AppendLine("}");
        }
        else
        {
            throw new ArgumentException($"调用错误，其他返回值类型不应该调用");
        }

        return methodSource.ToString();
    }
}