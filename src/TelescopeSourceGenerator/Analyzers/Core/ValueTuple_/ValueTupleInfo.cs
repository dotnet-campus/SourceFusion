using System.Collections.Generic;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

/// <summary>
/// 表示 ValueTuple 的信息
/// </summary>
public class ValueTupleInfo
{
    public ValueTupleInfo(IReadOnlyList<ValueTupleItemSyntaxAndSymbolInfo> itemList)
    {
        ItemList = itemList;
    }

    public IReadOnlyList<ValueTupleItemSyntaxAndSymbolInfo> ItemList { get; }
}