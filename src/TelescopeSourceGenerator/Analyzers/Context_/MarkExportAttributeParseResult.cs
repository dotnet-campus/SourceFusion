using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

readonly struct MarkExportAttributeParseResult
{
    public MarkExportAttributeParseResult(bool success, TypeInfo baseClassOrInterfaceTypeInfo,
        TypeInfo attributeTypeInfo)
    {
        Success = success;
        BaseClassOrInterfaceTypeInfo = baseClassOrInterfaceTypeInfo;
        AttributeTypeInfo = attributeTypeInfo;
    }

    public bool Success { get; }
    public TypeInfo BaseClassOrInterfaceTypeInfo { get; }

    public TypeInfo AttributeTypeInfo { get; }
}