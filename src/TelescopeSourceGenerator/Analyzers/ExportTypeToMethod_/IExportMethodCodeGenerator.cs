using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

interface IExportMethodCodeGenerator
{
    string GenerateSourceCode(ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list, CancellationToken token);
}