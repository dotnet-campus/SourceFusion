using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class IncrementalValuesProviderHelper
{
    /// <summary>
    /// 过滤掉空的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static IncrementalValuesProvider<T> ExcludeNulls<T>(this IncrementalValuesProvider<T?> provider)
    {
        return provider.Where(static t => t != null).Select(static (t, _) => t!);
    }
}