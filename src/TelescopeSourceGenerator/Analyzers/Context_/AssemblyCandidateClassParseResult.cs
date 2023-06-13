using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

readonly struct AssemblyCandidateClassParseResult
{
    public AssemblyCandidateClassParseResult(INamedTypeSymbol typeInfo, ImmutableArray<AttributeData> attributes, GeneratorSyntaxContext generatorSyntaxContext)
    {
        TypeInfo = typeInfo;
        Attributes = attributes;
        GeneratorSyntaxContext = generatorSyntaxContext;
        Success = true;
    }

    public AssemblyCandidateClassParseResult()
    {
        Success = false;
        TypeInfo = default;
        Attributes = default;
        GeneratorSyntaxContext = default;
    }

    public INamedTypeSymbol TypeInfo { get; }

    public ImmutableArray<AttributeData> Attributes { get; }

    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

    public bool Success { get; }
}