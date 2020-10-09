using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnetCampus.SourceFusion.CompileTime;

namespace dotnetCampus.TelescopeTask.CodeGenerating
{
    internal static class AttributedTypesExportFileGenerator
    {
        internal static string BuildClassImplementation(
            IEnumerable<string> exportedInterfaces,
            IEnumerable<string> exportedMethodCodes,
            IEnumerable<string> usings)
        {
            return $@"{string.Join(@"
", usings.Select(x => $"using {x};"))}

namespace dotnetCampus.Telescope
{{
    public partial class __AttributedTypesExport__ : {string.Join(", ", exportedInterfaces)}
    {{
        {string.Join(@"
        ", exportedMethodCodes)}
    }}
}}";
        }

        internal static string BuildExplicitMethodImplementation(IEnumerable<CompileType> types,
            string baseClassOrInterfaceName, string attributeName)
        {
            var exportedTypes = types.Where(x => GuessTypeMatch(x, baseClassOrInterfaceName, attributeName));
            var arrayExpression = $@"new AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[]
            {{
                {string.Join(@",
                ", exportedTypes
                .Select(x => BuildTypeAttributeMetadata(
                    x,
                    (CompileAttribute)x.Attributes.First(x => x.Match(attributeName)),
                    baseClassOrInterfaceName,
                    attributeName)))}
            }}";
            return $@"AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[] ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>.ExportAttributeTypes()
        {{
            return {arrayExpression};
        }}";
        }

        internal static string BuildTypeAttributeMetadata(CompileType type, CompileAttribute attribute,
            string baseClassOrInterfaceName, string attributeName)
        {
            return $@"new AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(
                    typeof({type.FullName}),
                    new {attributeName}({string.Join(", ", attribute.GetValues())})
                    {{
                        {string.Join(@",
                        ", attribute.GetProperties().Select(x => $"{x.property} = {x.value}"))}
                    }},
                    () => new {type.FullName}()
                )";
        }

        /// <summary>
        /// 判断一个编译期类型是否继承自基类 <paramref name="baseClassOrInterfaceName"/> 且标记了 <paramref name="attributeName"/> 特性。0
        /// </summary>
        /// <param name="compileType"></param>
        /// <param name="baseClassOrInterfaceName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private static bool GuessTypeMatch(ICompileType compileType, string baseClassOrInterfaceName, string attributeName)
        {
            return compileType.Attributes.Any(x => x.Match(attributeName));
        }
    }
}
