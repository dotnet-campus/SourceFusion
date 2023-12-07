using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

class ExportedTypesCodeTextGenerator
{
    public string Generate(ImmutableArray<MarkClassParseResult> markClassCollection, CancellationToken token)
    {
        if (markClassCollection.Length == 0)
        {
            // 如果没有任何需要导出的类型，那就不要创建任何代码
            return string.Empty;
        }

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
                var typeName = TypeSymbolHelper.TypeSymbolToFullName(markClassParseResult.ExportedTypeSymbol);

                var attributeCreatedCode =
                    AttributeCodeReWriter.GetAttributeCreatedCode(markClassParseResult.MatchAssemblyMarkAttributeData);

                var itemCode =
                    @$"new AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(typeof({typeName}), {attributeCreatedCode}, () => new {typeName}())";
                exportedItemList.Add(itemCode);
            }

            var arrayExpression = $@"new AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[]
            {{
                {string.Join(@",
                ", exportedItemList)}
            }}";

            var methodCode =
                $@"AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[] ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>.ExportAttributeTypes()
        {{
            return {arrayExpression};
        }}";

            exportedMethodCodes.Add(methodCode);

            exportedInterfaces.Add(
                $@"ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>");
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
    public partial class __AttributedTypesExport__ : ICompileTimeAttributedTypesExporter<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>
    {
        AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>[] ICompileTimeAttributedTypesExporter<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>.ExportAttributeTypes()
        {
            return new AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>[]
            {
                new AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>
                (
                   typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo), 
                   new global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute(1, (global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooEnum)1, typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base), null) 
                   {
                       Number2 = 2, 
                       Type2 = typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo),
                       FooEnum2 = (global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooEnum)0,
                       Type3 = null 
                   }, 
                   () => new global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo()
                )
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
        return rootSyntaxNode.NormalizeWhitespace().ToFullString();
    }
}