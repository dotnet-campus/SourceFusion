using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 导出的方法的导出类型返回值结果
/// </summary>
class ExportMethodReturnTypeCollectionResult : IExportMethodReturnTypeCollectionResult
{
    public ExportMethodReturnTypeCollectionResult(ITypeSymbol expectedClassBaseType, ITypeSymbol? expectedClassAttributeType, ExportTypeCollectionResult exportTypeCollectionResult, ExportMethodReturnType exportMethodReturnType)
    {
        ExpectedClassBaseType = expectedClassBaseType;
        ExpectedClassAttributeType = expectedClassAttributeType;
        ExportTypeCollectionResult = exportTypeCollectionResult;
        ExportMethodReturnType= exportMethodReturnType;
    }

    /// <summary>
    /// 期望收集的类型所继承的基础类型
    /// </summary>
    public ITypeSymbol ExpectedClassBaseType { get; }

    /// <summary>
    /// 期望类型标记的特性，可选
    /// </summary>
    public ITypeSymbol? ExpectedClassAttributeType { get; }

    public ExportTypeCollectionResult ExportTypeCollectionResult { get; }

    /// <summary>
    /// 程序集里面标记了导出的分部方法，将用来生成代码
    /// </summary>
    public IMethodSymbol ExportPartialMethodSymbol => ExportTypeCollectionResult.ExportPartialMethodSymbol;

    /// <summary>
    /// 导出类型的返回类型信息
    /// </summary>
    public ExportMethodReturnType ExportMethodReturnType { get; }

    /// <summary>
    /// 判断传入的程序集类型满足当前的要求条件
    /// </summary>
    /// <param name="assemblyClassTypeSymbol"></param>
    /// <returns></returns>
    public bool IsMatch(INamedTypeSymbol assemblyClassTypeSymbol)
    {
        if (assemblyClassTypeSymbol.IsAbstract)
        {
            // 抽象类不能提供
            return false;
        }

        // 先判断是否继承，再判断是否标记特性
        if (!TypeSymbolHelper.IsInherit(assemblyClassTypeSymbol, ExpectedClassBaseType))
        {
            // 没有继承基类，那就是不符合了
            return false;
        }

        if (ExpectedClassAttributeType is null)
        {
            // 如果没有特性要求，那就返回符合
            return true;
        }

        foreach (var attributeData in assemblyClassTypeSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, ExpectedClassAttributeType))
            {
                return true;
            }
        }

        return false;
    }
}