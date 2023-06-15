using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class TypeSymbolHelper
{
    /// <summary>
    /// 输出类型的完全限定名
    /// </summary>
    public static string TypeSymbolToFullName(ITypeSymbol typeSymbol)
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