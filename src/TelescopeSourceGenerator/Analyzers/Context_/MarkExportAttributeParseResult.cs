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

    /// <summary>
    /// 获取表示失败的特性解析结果。
    /// </summary>
    public static MarkExportAttributeParseResult Failure { get; } = new MarkExportAttributeParseResult(false, default!, default!);
}