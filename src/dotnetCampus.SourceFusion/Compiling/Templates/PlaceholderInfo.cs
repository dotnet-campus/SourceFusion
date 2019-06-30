using System.Collections.Generic;
using dotnetCampus.Compiling.Core;
using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.Compiling.Templates
{
    internal abstract class PlaceholderInfo
    {
        private readonly List<string> _requiredNamespaces = new List<string>();

        protected PlaceholderInfo(TextSpan span)
        {
            Span = span;
        }

        /// <summary>
        /// 获取占位符在源代码文件中的文本区间。
        /// </summary>
        public TextSpan Span { get; }

        public IReadOnlyCollection<string> RequiredNamespaces => _requiredNamespaces;

        public abstract string Fill(CompilingContext context);

        protected void RequireNamespaces(IEnumerable<string> namespaces)
        {
            foreach (var ns in namespaces)
            {
                if (!_requiredNamespaces.Contains(ns))
                {
                    _requiredNamespaces.Add(ns);
                }
            }
        }
    }
}
