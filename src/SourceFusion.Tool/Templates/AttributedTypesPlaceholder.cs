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

            // 这里可以有很多种找法，但哪一种都很麻烦：
            // 1. 使用 Roslyn 提供的 SymbolFinder 来加载解决方案和项目，初步估计每个项目需要增加 30s+ 的编译时间；
            // 2. 去查找解决方案中所有项目里的文件，可能不准，可能找不到；
            // 3. 试图加载所有 EN 引用的程序集，通过反射去找，这会限制 SourceFusion 的目标框架。

            return new CompileFile(
                @"D:\Developments\CVTE\EasiNote\Code\Core\EasiNote.Api\EditableProperty\ElementPropertyEditorAttribute.cs",
                new[] {"DEBUG", "TRACE"}).Types.First();
        }
    }
}
