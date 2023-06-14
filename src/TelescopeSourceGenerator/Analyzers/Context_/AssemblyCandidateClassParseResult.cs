using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

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
        ExportedTypeSymbol = default;
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

    public ImmutableArray<AttributeData> Attributes { get; }

    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

    public bool Success { get; }
}