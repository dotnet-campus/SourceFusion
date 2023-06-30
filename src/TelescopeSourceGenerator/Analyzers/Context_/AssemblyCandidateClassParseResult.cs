using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 程序集里可能被标记导出的类型
/// </summary>
readonly struct AssemblyCandidateClassParseResult
{
    public AssemblyCandidateClassParseResult(INamedTypeSymbol exportedTypeSymbol, ImmutableArray<AttributeData> attributes, GeneratorSyntaxContext generatorSyntaxContext)
    {
        ExportedTypeSymbol = exportedTypeSymbol;
        Attributes = attributes;
        GeneratorSyntaxContext = generatorSyntaxContext;
        Success = true;
    }

    public AssemblyCandidateClassParseResult()
    {
        Success = false;
        ExportedTypeSymbol = default!; // 这个瞬间就被过滤掉了，先不考虑可空的复杂写法了
        Attributes = default;
        GeneratorSyntaxContext = default;
    }

    /// <summary>
    /// 导出类型的语义符号
    /// </summary>
    public INamedTypeSymbol ExportedTypeSymbol { get; }

    /// <summary>
    /// 导出类型的语法符号
    /// </summary>
    public ClassDeclarationSyntax ExportedTypeClassDeclarationSyntax => (ClassDeclarationSyntax) GeneratorSyntaxContext.Node;

    /// <summary>
    /// 类型标记的特性
    /// </summary>
    public ImmutableArray<AttributeData> Attributes { get; }

    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

    /// <summary>
    /// 是否成功，用于过滤掉不满足条件的对象
    /// </summary>
    public bool Success { get; }
}