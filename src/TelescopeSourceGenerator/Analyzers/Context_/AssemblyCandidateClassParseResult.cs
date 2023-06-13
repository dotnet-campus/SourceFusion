using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

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

    public ImmutableArray<AttributeData> Attributes { get; }

    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

    public bool Success { get; }
}