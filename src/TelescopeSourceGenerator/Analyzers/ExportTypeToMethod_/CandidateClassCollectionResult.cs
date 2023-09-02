using System.Collections.Generic;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 候选收集的结果
/// </summary>
/// 包含所有的导出结果。比如项目里面有多个导出方法，每一个导出方法就是一个 CandidateClassTypeResult 对象
class CandidateClassCollectionResult
{
    public CandidateClassCollectionResult(IReadOnlyList<CandidateClassTypeResult> candidateClassTypeResultList)
    {
        CandidateClassTypeResultList = candidateClassTypeResultList;
    }

    public IReadOnlyList<CandidateClassTypeResult> CandidateClassTypeResultList { get; }
}