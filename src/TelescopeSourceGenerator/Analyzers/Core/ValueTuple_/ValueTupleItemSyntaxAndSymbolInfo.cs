using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

/// <summary>
/// 表示 ValueTuple 的每一项的定义内容
/// </summary>
public class ValueTupleItemSyntaxAndSymbolInfo
{
    public ValueTupleItemSyntaxAndSymbolInfo(ITypeSymbol itemType, string itemName)
    {
        ItemType = itemType;
        ItemName = itemName;
    }

    /// <summary>
    /// 类型
    /// </summary>
    public ITypeSymbol ItemType { get; }

    /// <summary>
    /// 命名。如果没有命名，那就是空字符串
    /// </summary>
    public string ItemName { get; }
}