using System;
using System.Linq;

namespace Cvte.Compiler.Tests
{
    [CompileTimeCode("../Fakes/ActionCommand.cs", RepeatCount = 3, KeepSourceFiles = true)]
    public class GenerateGeneric : IPlainCodeTransformer
    {
        private const string ToolName = "Cvte.Compiler.Tests";
        private const string ToolVersion = "1.0";

        private static readonly string GeneratedFooter =
            $@"";

        private static readonly string Generatedattribute =
            $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")]";

        public string Transform(string originalText, TransformingContext context)
        {
            var genericCount = context.RepeatIndex + 2;

            var content = originalText
                // 替换泛型。
                .Replace("<out T>", FromTemplate("<{0}>", "out T{n}", ", ", genericCount))
                .Replace("Task<T>", FromTemplate("Task<({0})>", "T{n}", ", ", genericCount))
                .Replace("Func<T, Task>", FromTemplate("Func<{0}, Task>", "T{n}", ", ", genericCount))
                .Replace(" T, Task>", FromTemplate(" {0}, Task>", "T{n}", ", ", genericCount))
                .Replace("(T, bool", FromTemplate("({0}, bool", "T{n}", ", ", genericCount))
                .Replace("var (t, ", FromTemplate("var ({0}, ", "t{n}", ", ", genericCount))
                .Replace(", t)", FromTemplate(", {0})", "t{n}", ", ", genericCount))
                .Replace("return (t, ", FromTemplate("return ({0}, ", "t{n}", ", ", genericCount))
                .Replace("<T>", FromTemplate("<{0}>", "T{n}", ", ", genericCount))
                .Replace("{T}", FromTemplate("{{{0}}}", "T{n}", ", ", genericCount))
                .Replace("(T value)", FromTemplate("(({0}) value)", "T{n}", ", ", genericCount))
                .Replace("(T t)", FromTemplate("({0})", "T{n} t{n}", ", ", genericCount))
                .Replace("(t)", FromTemplate("({0})", "t{n}", ", ", genericCount))
                .Replace("var t =", FromTemplate("var ({0}) =", "t{n}", ", ", genericCount))
                .Replace(" T ", FromTemplate(" ({0}) ", "T{n}", ", ", genericCount))
                .Replace(" t;", FromTemplate(" ({0});", "t{n}", ", ", genericCount))
                // 生成 [GeneratedCode]。
                .Replace("    public interface ", $"    {Generatedattribute}{Environment.NewLine}    public interface ")
                .Replace("    public class ", $"    {Generatedattribute}{Environment.NewLine}    public class ")
                .Replace("    public sealed class ", $"    {Generatedattribute}{Environment.NewLine}    public sealed class ");
            return content.Trim() + Environment.NewLine + GeneratedFooter;
        }

        private static string FromTemplate(string template, string part, string seperator, int count)
        {
            return string.Format(template,
                string.Join(seperator, Enumerable.Range(1, count).Select(x => part.Replace("{n}", x.ToString()))));
        }
    }
}
