using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.SourceFusion.Templates
{
    internal abstract class PlaceholderInfo
    {
        protected PlaceholderInfo(TextSpan span)
        {
            Span = span;
        }

        /// <summary>
        /// 获取占位符在源代码文件中的文本区间。
        /// </summary>
        public TextSpan Span { get; }

        public abstract string Execute(CompilingContext context);
    }
}
