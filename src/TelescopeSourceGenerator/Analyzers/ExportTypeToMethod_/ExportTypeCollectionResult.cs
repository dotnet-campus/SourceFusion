using System;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

class ExportTypeCollectionResult : IEquatable<ExportTypeCollectionResult>
{
    public ExportTypeCollectionResult(IMethodSymbol methodSymbol, GeneratorSyntaxContext generatorSyntaxContext)
    {
        ExportPartialMethodSymbol = methodSymbol;
        GeneratorSyntaxContext = generatorSyntaxContext;
    }

    /// <summary>
    /// 是否包含引用的程序集和 DLL 里面的类型导出。默认只导出当前程序集
    /// </summary>
    public bool IncludeReference { set; get; }
    /// <summary>
    /// 程序集里面标记了导出的分部方法，将用来生成代码
    /// </summary>
    public IMethodSymbol ExportPartialMethodSymbol { get; }
    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

    public bool Equals(ExportTypeCollectionResult? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return GeneratorSyntaxContext.Equals(other.GeneratorSyntaxContext) && SymbolEqualityComparer.Default.Equals(ExportPartialMethodSymbol, other.ExportPartialMethodSymbol);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ExportTypeCollectionResult) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (SymbolEqualityComparer.Default.GetHashCode(ExportPartialMethodSymbol) * 397) ^ GeneratorSyntaxContext.GetHashCode();
        }
    }
}