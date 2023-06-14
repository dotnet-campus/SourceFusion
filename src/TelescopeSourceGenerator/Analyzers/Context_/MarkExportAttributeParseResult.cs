using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

readonly struct MarkExportAttributeParseResult
{
    public MarkExportAttributeParseResult(bool success, ITypeSymbol baseClassOrInterfaceTypeInfo,
        ITypeSymbol attributeTypeInfo)
    {
        Success = success;
        BaseClassOrInterfaceTypeInfo = baseClassOrInterfaceTypeInfo;
        AttributeTypeInfo = attributeTypeInfo;
    }

    public bool Success { get; }
    public ITypeSymbol BaseClassOrInterfaceTypeInfo { get; }

    public ITypeSymbol AttributeTypeInfo { get; }
}