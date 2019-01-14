using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.SourceFusion.Templates
{
    /// <summary>
    /// 包含 <see cref="Placeholder.AttributedTypes{T, TAttribute}"/> 占位符在语法树中的信息。
    /// </summary>
    internal class AttributedTypesPlaceholder : PlaceholderInfo
    {
        private readonly string _baseType;
        private readonly string _attributeType;

        public AttributedTypesPlaceholder(TextSpan span, string baseType, string attributeType) : base(span)
        {
            _baseType = baseType;
            _attributeType = attributeType;
        }

        public override string Execute(CompilingContext context)
        {
            var collectedItems = CollectAttributedTypes();
            return $@"new (Func<{_baseType}>, {_attributeType})[]
            {{
                {string.Join(@",
            ", collectedItems.Select(item => $@"    (() => new object(), new {_attributeType}({item.values})
                {{
                    {string.Join(@",
            ", item.properties.Select(p => $@"        {p.Key} = {p.Value}"))}
                }}"))}
            }}";
        }

        private IEnumerable<(string values, Dictionary<string, string> properties)> CollectAttributedTypes()
        {
            return Enumerable.Empty<(string, Dictionary<string, string>)>();
        }
    }
}
