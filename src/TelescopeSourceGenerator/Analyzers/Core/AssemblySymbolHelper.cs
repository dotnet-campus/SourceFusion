using Microsoft.CodeAnalysis;

using System.Collections.Generic;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class AssemblySymbolHelper
{
    /// <summary>
    /// 获取当前程序集里面的所有类型
    /// </summary>
    /// <param name="assemblySymbol"></param>
    /// <returns></returns>
    public static IEnumerable<INamedTypeSymbol> GetAllTypeSymbols(IAssemblySymbol assemblySymbol) => GetAllTypeSymbols(assemblySymbol.GlobalNamespace);

    public static IEnumerable<INamedTypeSymbol> GetAllTypeSymbols(INamespaceSymbol namespaceSymbol)
    {
        var typeMemberList = namespaceSymbol.GetTypeMembers();

        foreach (var typeSymbol in typeMemberList)
        {
            yield return typeSymbol;
        }

        foreach (var namespaceMember in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var typeSymbol in GetAllTypeSymbols(namespaceMember))
            {
                yield return typeSymbol;
            }
        }
    }

    public static bool IsReference(IAssemblySymbol currentAssemblySymbol, IAssemblySymbol requiredAssemblySymbol)
    {
        var visited = new Dictionary<IAssemblySymbol, bool /*是否引用*/>(SymbolEqualityComparer.Default);
        return IsReference(currentAssemblySymbol, requiredAssemblySymbol, visited);
    }

    public static bool IsReference(IAssemblySymbol currentAssemblySymbol, IAssemblySymbol requiredAssemblySymbol,
        Dictionary<IAssemblySymbol, bool /*是否引用*/> visited)
    {
        if (SymbolEqualityComparer.Default.Equals(currentAssemblySymbol, requiredAssemblySymbol))
        {
            // 这个就看业务了，如果两个程序集是相同的，是否判断为引用关系
            return true;
        }

        foreach (var moduleSymbol in currentAssemblySymbol.Modules)
        {
            foreach (var referencedAssemblySymbol in moduleSymbol.ReferencedAssemblySymbols)
            {
                if (SymbolEqualityComparer.Default.Equals(referencedAssemblySymbol, requiredAssemblySymbol))
                {
                    // 记录当前程序集存在引用关系
                    visited[currentAssemblySymbol] = true;
                    return true;
                }
                else if (SymbolEqualityComparer.Default.Equals(referencedAssemblySymbol, currentAssemblySymbol))
                {
                    // 循环引用，跳过
                   continue;
                }
                else
                {
                    if (visited.TryGetValue(referencedAssemblySymbol, out var isReference))
                    {
                        // 这个是访问过的，那就从字典获取缓存，不需要再访问一次
                        // 同时也能解决程序集循环引用问题
                    }
                    else
                    {
                        // 进入递归之前，先将自身设置到字典，先设置为没有引用。防止循环引用炸掉
                        visited[referencedAssemblySymbol] = false;

                        // 没有访问过的，获取引用的程序集是否存在引用关系
                        isReference = IsReference(referencedAssemblySymbol, requiredAssemblySymbol, visited);
                        visited[referencedAssemblySymbol] = isReference;
                    }

                    if (isReference)
                    {
                        // 如果这个程序集有引用，那也算上
                        return true;
                    }
                }
            }
        }

        return false;
    }
}