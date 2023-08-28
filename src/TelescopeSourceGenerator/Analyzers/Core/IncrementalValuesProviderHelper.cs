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
    public static IncrementalValuesProvider<T> FilterNull<T>(this IncrementalValuesProvider<T?> provider)
    {
        return provider.Where(t => t != null).Select((t, _) => t!);
    }
}