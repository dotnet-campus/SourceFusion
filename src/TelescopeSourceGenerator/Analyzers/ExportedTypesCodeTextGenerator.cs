using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

class ExportedTypesCodeTextGenerator
{
    public string Generate(ImmutableArray<MarkClassParseResult> markClassCollection, CancellationToken token)
    {
        // 导出的接口
        var exportedInterfaces = new List<string>();
        // 导出的方法
        var exportedMethodCodes = new List<string>();

        foreach (var markClassGroup in markClassCollection.GroupBy(t => t.MarkExportAttributeParseResult))
        {
            token.ThrowIfCancellationRequested();

            var markExportAttributeParseResult = markClassGroup.Key;
          
            var baseClassOrInterfaceName =
                TypeSymbolHelper.TypeSymbolToFullName(markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo);
            var attributeName = TypeSymbolHelper.TypeSymbolToFullName(markExportAttributeParseResult.AttributeTypeInfo);

            var exportedItemList = new List<string>();

            foreach (var markClassParseResult in markClassGroup)
            {
                // new ExportedTypeMetadata<TBaseClassOrInterface, TAttribute>(typeof(type), () => new {type}())
                var typeName = TypeSymbolHelper.TypeSymbolToFullName(markClassParseResult.ExportedTypeSymbol);

                var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(markClassParseResult);

                var itemCode =
                    @$"new ExportedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(typeof({typeName}), () => new {typeName}())";
                exportedItemList.Add(itemCode);
            }

            var arrayExpression = $@"new ExportedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[]
            {{
                {string.Join(@",
                ", exportedItemList)}
            }}";

            var methodCode =
                $@"ExportedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[] ICompileTimeTypesExporter<{baseClassOrInterfaceName}, {attributeName}>.ExportTypes()
        {{
            return {arrayExpression};
        }}";

            exportedMethodCodes.Add(methodCode);

            exportedInterfaces.Add($@"ICompileTimeTypesExporter<{baseClassOrInterfaceName}, {attributeName}>");
        }

        var code = $@"using dotnetCampus.Telescope;

namespace dotnetCampus.Telescope
{{
    public partial class __AttributedTypesExport__ : {string.Join(", ", exportedInterfaces)}
    {{
        {string.Join(@"
        ", exportedMethodCodes)}
    }}
}}";
        code = FormatCode(code);
        // 生成的代码示例：
        /*
        using dotnetCampus.Telescope;

        namespace dotnetCampus.Telescope
        {
            public partial class __AttributedTypesExport__ : ICompileTimeTypesExporter<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>
            {
                ExportedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>[] ICompileTimeTypesExporter<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>.ExportTypes()
                {
                    return new ExportedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>[]
                    {
                        new ExportedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>(typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo), () => new global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo())
                    };
                }
            }
        }
         */
        return code;
    }

    /// <summary>
    /// 格式化代码。
    /// </summary>
    /// <param name="sourceCode">未格式化的源代码。</param>
    /// <returns>格式化的源代码。</returns>
    private static string FormatCode(string sourceCode)
    {
        var rootSyntaxNode = CSharpSyntaxTree.ParseText(sourceCode).GetRoot();
        return rootSyntaxNode.NormalizeWhitespace().SyntaxTree.GetText().ToString();
    }
}

static class TypeSymbolHelper
{
    /// <summary>
    /// 输出类型的完全限定名
    /// </summary>
    public static string TypeSymbolToFullName(ITypeSymbol typeSymbol)
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

        return typeSymbol.ToDisplayString(symbolDisplayFormat);
    }
}

static class AttributeCodeReWriter
{
    public static string GetAttributeCreatedCode(MarkClassParseResult markClassParseResult)
    {
        var markAttributeData = markClassParseResult.MatchAssemblyMarkAttributeData;
        var markAttributeSyntax = markClassParseResult.MatchAssemblyMarkAttributeSyntax;

        // 放在特性的构造函数的参数列表，例如 [Foo(1,2,3)] 将会获取到 `1` `2` `3` 三个参数
        var constructorArgumentCodeList = new List<string>();
        foreach (TypedConstant constructorArgument in markAttributeData.ConstructorArguments)
        {
            var constructorArgumentCode = TypedConstantToCodeString(constructorArgument);

            constructorArgumentCodeList.Add(constructorArgumentCode);
        }

        var namedArgumentCodeList = new List<(string propertyName, string valueCode)>();
        foreach (var keyValuePair in markAttributeData.NamedArguments)
        {
            var key = keyValuePair.Key;

            var typedConstant = keyValuePair.Value;
            var argumentCode = TypedConstantToCodeString(typedConstant);

            namedArgumentCodeList.Add((key, argumentCode));
        }

        return
            $@"new {TypeSymbolHelper.TypeSymbolToFullName(markAttributeData.AttributeClass!)}({string.Join(",", constructorArgumentCodeList)})
{{
           {string.Join(@",
                        ", namedArgumentCodeList.Select(x => $"{x.propertyName} = {x.valueCode}"))}
}}";

        static string TypedConstantToCodeString(TypedConstant typedConstant)
        {
            var constructorArgumentType = typedConstant.Type;
            var constructorArgumentValue = typedConstant.Value;

            string constructorArgumentCode;
            switch (typedConstant.Kind)
            {
                case TypedConstantKind.Enum:
                {
                    // "(Foo.Enum1) 1"
                    constructorArgumentCode =
                        $"({TypeSymbolHelper.TypeSymbolToFullName(typedConstant.Type!)}) {typedConstant.Value}";
                    break;
                }
                case TypedConstantKind.Type:
                {
                    var typeSymbol = (ITypeSymbol?)constructorArgumentValue;
                    if (typeSymbol is null)
                    {
                        constructorArgumentCode = "null";
                    }
                    else
                    {
                        constructorArgumentCode = $"typeof({TypeSymbolHelper.TypeSymbolToFullName(typeSymbol)})";
                    }

                    break;
                }
                default:
                {
                    constructorArgumentCode = typedConstant.Value?.ToString() ?? "null";
                    break;
                }
            }

            return constructorArgumentCode;
        }
    }
}