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
            var attribute = new CompileAttribute(_attributeType);
            var typeInCurrentAssembly = context.Assembly.GetTypes().FirstOrDefault(x => attribute.Match(x.Name));

            if (typeInCurrentAssembly == null)
            {
                // 此特性不是本程序集中定义的特性，需要使用反射访问。
            }
            else
            {
                // 此特性是本程序集中定义的特性，需要使用 Roslyn 访问。
            }

            return Enumerable.Empty<(string, Dictionary<string, string>)>();
        }

        //private IEnumerable<(string values, Dictionary<string, string> properties)> CollectByReflection(string typeName)
        //{
            
        //}
    }
}
