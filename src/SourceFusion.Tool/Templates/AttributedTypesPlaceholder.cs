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

        private IEnumerable<(string values, Dictionary<string, string> properties)> CollectAttributedTypes(
            CompilingContext context)
        {
            var file = FindAttributeFile(context, _attributeType);

            return Enumerable.Empty<(string, Dictionary<string, string>)>();
        }

        private ICompileType FindAttributeFile(CompilingContext context, string typeName)
        {
            var attribute = new CompileAttribute(typeName);
            var typeInCurrentAssembly = context.Assembly.GetTypes().FirstOrDefault(x => attribute.Match(x.Name));
            if (typeInCurrentAssembly != null)
            {
                return typeInCurrentAssembly;
            }

            return new CompileFile(
                @"D:\Developments\CVTE\EasiNote\Code\Core\EasiNote.Api\EditableProperty\ElementPropertyEditorAttribute.cs",
                new[] {"DEBUG", "TRACE"}).Types.First();
        }
    }
}
