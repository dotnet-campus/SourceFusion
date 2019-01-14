﻿using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.SourceFusion.Templates
{
    /// <summary>
    /// 包含 <see cref="Placeholder.AttributedTypes{T, TAttribute}"/> 占位符在语法树中的信息。
    /// </summary>
    internal class AttributedTypesPlaceholder : PlaceholderInfo
    {
        public AttributedTypesPlaceholder(TextSpan span) : base(span)
        {
        }
    }
}
