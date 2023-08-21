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

    /// <summary>
    /// 判断类型继承关系
    /// </summary>
    /// <param name="currentType">当前的类型</param>
    /// <param name="requiredType">需要继承的类型</param>
    /// <returns></returns>
    public static bool IsInherit(ITypeSymbol currentType, ITypeSymbol requiredType)
    {
        var baseType = currentType.BaseType;
        while (baseType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, requiredType))
            {
                // 如果基类型是的话
                return true;
            }

            // 否则继续找基类型
            baseType = baseType.BaseType;
        }

        foreach (var currentInheritInterfaceType in currentType.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(currentInheritInterfaceType, requiredType))
            {
                // 如果继承的类型是的话
                return true;
            }
        }

        return false;
    }
}