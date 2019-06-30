using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace dotnetCampus.SourceFusion
{
    /// <summary>
    /// 为编译期生成代码提供代码片段生成功能。
    /// </summary>
    public class CompileCodeSnippet
    {
        /// <summary>
        /// 获取生成的代码片段。
        /// </summary>
        private readonly string _snippet;

        /// <summary>
        /// 创建一个编译期代码片段。
        /// 其中参数 <paramref name="template"/> 将按 <paramref name="placeholders"/> 序列个数重复多次，并将 “{0}” 替换为集合中的一项。
        /// 例如 <paramref name="template"/> 为 “new {0}(), ”，<paramref name="placeholders"/> 为 { "Foo", "Bar" }；
        /// 那么最终生成的代码片段为 “new Foo(), new Bar(), ”。
        /// </summary>
        /// <param name="template">代码片段模板，例如 “new {0}(), ”。</param>
        /// <param name="placeholders">用于填充代码模板的集合，例如 { "Foo", "Bar" }。</param>
        public CompileCodeSnippet(string template, IEnumerable<string> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v));

        public CompileCodeSnippet(string template, IEnumerable<(string, string)> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v.Item1, v.Item2));

        public CompileCodeSnippet(string template, IEnumerable<(string, string, string)> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v.Item1, v.Item2, v.Item3));

        public CompileCodeSnippet(string template, IEnumerable<(string, string, string, string)> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v.Item1, v.Item2, v.Item3, v.Item4));

        public CompileCodeSnippet(string template, IEnumerable<(string, string, string, string, string)> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v.Item1, v.Item2, v.Item3, v.Item4, v.Item5));

        public CompileCodeSnippet(string template, IEnumerable<(string, string, string, string, string, string)> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, v.Item6));

        public CompileCodeSnippet(string template, IEnumerable<(string, string, string, string, string, string, string)> placeholders)
            => _snippet = BuildSnippet(placeholders, v => string.Format(template, v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, v.Item6, v.Item7));

        /// <summary>
        /// 输出代码片段。
        /// </summary>
        [Pure]
        public override string ToString() => _snippet;

        /// <summary>
        /// 隐式转换为代码片段字符串。
        /// </summary>
        /// <param name="snippet">代码片段的实例。</param>
        public static implicit operator string(CompileCodeSnippet snippet)
        {
            return snippet._snippet;
        }

        private static string BuildSnippet<T>(IEnumerable<T> placeholders, Func<T, string> buildLine)
        {
            var builder = new StringBuilder();

            foreach (var placeholder in placeholders)
            {
                builder.Append(buildLine(placeholder));
            }

            return builder.ToString();
        }
    }
}
