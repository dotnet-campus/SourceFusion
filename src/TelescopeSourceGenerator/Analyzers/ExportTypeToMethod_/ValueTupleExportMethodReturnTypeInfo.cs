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
}