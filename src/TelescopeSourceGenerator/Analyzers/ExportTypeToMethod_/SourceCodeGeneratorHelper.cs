using System.Diagnostics;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

static class SourceCodeGeneratorHelper
{
    /// <summary>
    /// 提供缩进的方法
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numIndentations"></param>
    /// <returns></returns>
    public static string IndentSource(string source, int numIndentations)
    {
        Debug.Assert(numIndentations >= 1);
        return source.Replace("\r", "").Replace("\n", $"\r\n{new string(' ', 4 * numIndentations)}");
    }
}