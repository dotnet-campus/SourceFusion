using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 生成导出类型方法的代码
/// </summary>
/// 这个类型只是一个分发工厂，由于存在不同的返回值等需要支持，于是拆分为不同的实现方法
static class ExportMethodCodeGenerator
{
    public static string GenerateSourceCode(ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult, List<INamedTypeSymbol> list, CancellationToken token)
    {
        IExportMethodCodeGenerator codeGenerator;

        if (exportMethodReturnTypeCollectionResult.ExportMethodReturnTypeInfo is ValueTupleExportMethodReturnTypeInfo
            valueTupleExportMethodReturnTypeInfo)
        {
            if (valueTupleExportMethodReturnTypeInfo.IsIEnumerable)
            {
                codeGenerator = new EnumerableValueTupleExportMethodReturnTypeCodeGenerator();
            }
            else
            {
                // 还没支持其他返回值的情况
                throw new NotSupportedException();
            }
        }
        else
        {
            // 还没支持其他返回值的情况
            throw new NotSupportedException();
        }

        return codeGenerator.GenerateSourceCode(exportMethodReturnTypeCollectionResult, list, token);
    }
}