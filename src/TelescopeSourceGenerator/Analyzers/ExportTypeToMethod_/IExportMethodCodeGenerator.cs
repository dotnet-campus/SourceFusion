using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 导入方法的生成器
/// </summary>
interface IExportMethodCodeGenerator
{
    string GenerateSourceCode(ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list, CancellationToken token);
}