using System.Collections.Generic;
using System.Linq;
using dotnetCampus.SourceFusion.CompileTime;
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

        public override string Fill(CompilingContext context)
        {
            var collectedItems = CollectAttributedTypes(context);
            return $@"new (Type, {_attributeType})[]
            {{
                {string.Join(@",
                ", collectedItems.Select(item => $@"(typeof({item.typeName}), {item.attributeCreator})"))}
            }}";
        }

        private IEnumerable<(string typeName, string attributeCreator)> CollectAttributedTypes(
            CompilingContext context)
        {
            IEnumerable<(ICompileType type, CompileAttribute attribute)> collected =
                from type in context.Assembly.GetTypes()
                let attribute = (CompileAttribute) type.Attributes.FirstOrDefault(a => a.Match(_attributeType))
                where attribute != null
                select (type, attribute);

            return collected.Select(x => (x.type.FullName, ToCreator(x.attribute)));
        }

        private string ToCreator(CompileAttribute attribute)
        {
            return $@"new {_attributeType}({string.Join(", ", attribute.GetValues())})
                {{
                    {string.Join(@",
                    ", attribute.GetProperties().Select(x => $"{x.property} = {x.value}"))}
                }}";
        }
    }
}
