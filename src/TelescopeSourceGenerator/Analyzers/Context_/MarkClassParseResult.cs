namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

readonly struct MarkClassParseResult
{
    public MarkClassParseResult(AssemblyCandidateClassParseResult classParseResult,
        MarkExportAttributeParseResult markExportAttributeParseResult)
    {
        ClassParseResult = classParseResult;
        MarkExportAttributeParseResult = markExportAttributeParseResult;
    }

    public AssemblyCandidateClassParseResult ClassParseResult { get; }

    public MarkExportAttributeParseResult MarkExportAttributeParseResult { get; }
}