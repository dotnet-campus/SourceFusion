using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 被标记的类型的转换结果，约等于生成代码前的最终结果
/// </summary>
readonly struct MarkClassParseResult
{
    public MarkClassParseResult(INamedTypeSymbol exportedTypeSymbol, ClassDeclarationSyntax exportedTypeClassDeclarationSyntax, 
        AttributeData matchAssemblyMarkAttributeData, AttributeSyntax matchAssemblyMarkAttributeSyntax, 
        MarkExportAttributeParseResult markExportAttributeParseResult,
        GeneratorSyntaxContext generatorSyntaxContext)
    {
        ExportedTypeSymbol = exportedTypeSymbol;
        ExportedTypeClassDeclarationSyntax = exportedTypeClassDeclarationSyntax;
        MatchAssemblyMarkAttributeData = matchAssemblyMarkAttributeData;
        MatchAssemblyMarkAttributeSyntax = matchAssemblyMarkAttributeSyntax;
        MarkExportAttributeParseResult = markExportAttributeParseResult;
        GeneratorSyntaxContext = generatorSyntaxContext;
    }

    /// <summary>
    /// 导出的 class 类型的语义
    /// </summary>
    public INamedTypeSymbol ExportedTypeSymbol { get; }
    /// <summary>
    /// 导出的 class 类型的语法
    /// </summary>
    public ClassDeclarationSyntax ExportedTypeClassDeclarationSyntax { get; }
    /// <summary>
    /// 类型上标记的程序集指定特性的语义
    /// </summary>
    public AttributeData MatchAssemblyMarkAttributeData { get; }
    /// <summary>
    /// 类型上标记的程序集指定特性的语法
    /// </summary>
    public AttributeSyntax MatchAssemblyMarkAttributeSyntax { get; }

    /// <summary>
    /// 程序集特性里面的定义结果
    /// </summary>
    public MarkExportAttributeParseResult MarkExportAttributeParseResult { get; }

    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }
}