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
            /*
             * ExportedTypeMetadata<TBaseClassOrInterface, TAttribute>[] ICompileTimeTypesExporter<baseClassOrInterfaceName, attributeName>.ExportTypes()
             * {
             *    return new ExportedTypeMetadata<TBaseClassOrInterface, TAttribute>[]
             *    {
             *       new ExportedTypeMetadata<TBaseClassOrInterface, TAttribute>(typeof(type), () => new {type}())
             *    }
             * }
             */
            var baseClassOrInterfaceName =
                TypeSymbolToFullName(markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo);
            var attributeName = TypeSymbolToFullName(markExportAttributeParseResult.AttributeTypeInfo);

            var exportedItemList = new List<string>();

            foreach (var markClassParseResult in markClassGroup)
            {
                // new ExportedTypeMetadata<TBaseClassOrInterface, TAttribute>(typeof(type), () => new {type}())
                var typeName = TypeSymbolToFullName(markClassParseResult.ExportedTypeSymbol);
                var markAttributeData = markClassParseResult.MatchAssemblyMarkAttributeData;
                var markAttributeSyntax = markClassParseResult.MatchAssemblyMarkAttributeSyntax;


                var itemCode = @$"new ExportedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(typeof({typeName}), () => new {typeName}())";
                exportedItemList.Add(itemCode);
            }

            var arrayExpression = $@"new ExportedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[]
            {{
                {string.Join(@",
                ", exportedItemList)}
            }}";

            var methodCode = $@"ExportedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[] ICompileTimeTypesExporter<{baseClassOrInterfaceName}, {attributeName}>.ExportTypes()
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

    /// <summary>
    /// 输出类型的完全限定名
    /// </summary>
    private static string TypeSymbolToFullName(ITypeSymbol typeSymbol)
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