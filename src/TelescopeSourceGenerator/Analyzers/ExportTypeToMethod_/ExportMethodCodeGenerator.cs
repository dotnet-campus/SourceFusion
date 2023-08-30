using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 生成导出类型方法的代码
/// </summary>
/// 这个类型只是一个分发工厂，由于存在不同的返回值等需要支持，于是拆分为不同的实现方法
static class ExportMethodCodeGenerator
{
    public static string GenerateSourceCode(
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult, List<INamedTypeSymbol> list,
        CancellationToken token)
    {
        IExportMethodCodeGenerator codeGenerator =
            exportMethodReturnTypeCollectionResult.ExportMethodReturnType switch
            {
                ExportMethodReturnType
                        .EnumerableValueTupleWithTypeAttributeCreator
                        =>
                    new EnumerableValueTupleExportMethodReturnTypeCodeGenerator(),
                // 还没支持其他返回值的情况
                _ => throw new NotSupportedException()
            };

        return codeGenerator.GenerateSourceCode(exportMethodReturnTypeCollectionResult, list, token);
    }
}