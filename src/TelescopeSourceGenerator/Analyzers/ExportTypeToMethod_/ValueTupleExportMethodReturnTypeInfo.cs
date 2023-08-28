using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 导出类型的返回类型信息
/// </summary>
public class ValueTupleExportMethodReturnTypeInfo : IExportMethodReturnTypeInfo
{
    public ValueTupleExportMethodReturnTypeInfo(ValueTupleInfo valueTupleInfo)
    {
        ValueTupleInfo = valueTupleInfo;
    }

    public ValueTupleInfo ValueTupleInfo { get; }

    /// <summary>
    /// 是否采用 global::System.Collections.Generic.IEnumerable 返回值
    /// <code>IEnumerable&lt;(Type, F1Attribute xx, Func&lt;DemoLib1.F1&gt; xxx)&gt;</code>
    /// </summary>
    public bool IsIEnumerable { set; get; }
}