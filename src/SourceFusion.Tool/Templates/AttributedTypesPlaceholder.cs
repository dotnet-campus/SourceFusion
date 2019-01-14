﻿using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.SourceFusion.Templates
{
    /// <summary>
    /// 包含 <see cref="Placeholder.AttributedTypes{T, TAttribute}"/> 占位符在语法树中的信息。
    /// </summary>
    internal class AttributedTypesPlaceholder : PlaceholderInfo
    {
        private readonly string _attributeType;

        public AttributedTypesPlaceholder(TextSpan span, string attributeType) : base(span)
        {
            _attributeType = attributeType;
        }

        public override string Execute(CompilingContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}