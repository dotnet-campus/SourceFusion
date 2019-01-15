using System.Collections.Generic;
using System.Linq;
using dotnetCampus.SourceFusion.CompileTime;
using dotnetCampus.SourceFusion.Core;
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

        /// <summary>
        /// 获取或设置用户是否使用了 <see cref="Placeholder.AttributedTypes{T, TAttribute}"/> 的返回值中提供的额外元数据。
        /// 如果使用了额外的元数据，那么就必须生成这些元数据，否则就不要生成这些元数据。
        /// </summary>
        private readonly bool _useMetadata;

        public AttributedTypesPlaceholder(TextSpan span, string baseType, string attributeType, bool useMetadata)
            : base(span)
        {
            _baseType = baseType;
            _attributeType = attributeType;
            _useMetadata = useMetadata;
        }

        public override string Fill(CompilingContext context)
        {
            var collectedItems = CollectAttributedTypes(context);
            return _useMetadata
                // 如果使户使用了元数据，那么就生成更多的信息供用户调用。
                ? $@"new List<(Type Type, {_attributeType} Attribute, Func<{_baseType}> Creator)>
            {{
                {string.Join(@",
                ", collectedItems.Select(item => $@"(typeof({item.typeName}), {item.attributeCreator}, () => new {item.typeName}())"))}
            }}"
                // 如果用户没有使用元数据，那么就直接返回类型声明的返回值。
                : $@"new (Type, {_attributeType})[]
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
