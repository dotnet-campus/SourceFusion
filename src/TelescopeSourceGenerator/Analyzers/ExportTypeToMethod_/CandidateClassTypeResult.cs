using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 候选的导出类型结果
/// </summary>
/// 包含标记导出的方法信息，以及程序集里面导出的类型
class CandidateClassTypeResult
{
    public CandidateClassTypeResult(ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult, IReadOnlyList<INamedTypeSymbol> assemblyClassTypeSymbolList)
    {
        ExportMethodReturnTypeCollectionResult = exportMethodReturnTypeCollectionResult;
        AssemblyClassTypeSymbolList = assemblyClassTypeSymbolList;
    }

    public ExportMethodReturnTypeCollectionResult ExportMethodReturnTypeCollectionResult { get; }

    public IReadOnlyList<INamedTypeSymbol> AssemblyClassTypeSymbolList { get; }
}