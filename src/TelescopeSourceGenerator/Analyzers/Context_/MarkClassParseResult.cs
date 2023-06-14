namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 被标记的类型的转换结果，约等于生成代码前的最终结果
/// </summary>
readonly struct MarkClassParseResult
{
    public MarkClassParseResult(AssemblyCandidateClassParseResult classParseResult,
        MarkExportAttributeParseResult markExportAttributeParseResult)
    {
        ClassParseResult = classParseResult;
        MarkExportAttributeParseResult = markExportAttributeParseResult;
    }

    /// <summary>
    /// 定义在程序集里面的类型
    /// </summary>
    public AssemblyCandidateClassParseResult ClassParseResult { get; }

    /// <summary>
    /// 程序集特性里面的定义结果
    /// </summary>
    public MarkExportAttributeParseResult MarkExportAttributeParseResult { get; }
}