using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class AttributeCodeReWriter
{
    /// <summary>
    /// 从 <paramref name="attributeData"/> 转换为特性生成代码。从 `[Foo(xx, xxx)]` 语义转换为 `new Foo(xx, xxx)` 的生成代码
    /// </summary>
    /// <param name="attributeData"></param>
    /// <returns></returns>
    public static string GetAttributeCreatedCode(AttributeData attributeData)
    {
        // 放在特性的构造函数的参数列表，例如 [Foo(1,2,3)] 将会获取到 `1` `2` `3` 三个参数
        var constructorArgumentCodeList = new List<string>();
        foreach (TypedConstant constructorArgument in attributeData.ConstructorArguments)
        {
            var constructorArgumentCode = TypedConstantToCodeString(constructorArgument);

            constructorArgumentCodeList.Add(constructorArgumentCode);
        }

        var namedArgumentCodeList = new List<(string propertyName, string valueCode)>();
        foreach (var keyValuePair in attributeData.NamedArguments)
        {
            var key = keyValuePair.Key;

            var typedConstant = keyValuePair.Value;
            var argumentCode = TypedConstantToCodeString(typedConstant);

            namedArgumentCodeList.Add((key, argumentCode));
        }

        return
            $@"new {TypeSymbolHelper.TypeSymbolToFullName(attributeData.AttributeClass!)}({string.Join(",", constructorArgumentCodeList)})
{{
           {string.Join(@",
           ", namedArgumentCodeList.Select(x => $"{x.propertyName} = {x.valueCode}"))}
}}";

        static string TypedConstantToCodeString(TypedConstant typedConstant)
        {
            var constructorArgumentValue = typedConstant.Value;

            string constructorArgumentCode;
            switch (typedConstant.Kind)
            {
                case TypedConstantKind.Enum:
                {
                    // "(Foo.Enum1) 1"
                    constructorArgumentCode =
                        $"({TypeSymbolHelper.TypeSymbolToFullName(typedConstant.Type!)}) {typedConstant.Value}";
                    break;
                }
                case TypedConstantKind.Type:
                {
                    var typeSymbol = (ITypeSymbol?)constructorArgumentValue;
                    if (typeSymbol is null)
                    {
                        constructorArgumentCode = "null";
                    }
                    else
                    {
                        constructorArgumentCode = $"typeof({TypeSymbolHelper.TypeSymbolToFullName(typeSymbol)})";
                    }

                    break;
                }
                case TypedConstantKind.Primitive:
                {
                    if (typedConstant.Value is string text)
                    {
                        constructorArgumentCode = "\"" + text + "\"";
                    }
                    else if (typedConstant.Value is true)
                    {
                        constructorArgumentCode = "true";
                    }
                    else if (typedConstant.Value is false)
                    {
                        constructorArgumentCode = "false";
                    }
                    else
                    {
                        constructorArgumentCode = typedConstant.Value?.ToString() ?? "null";
                    }

                    break;
                }
                default:
                {
                    constructorArgumentCode = typedConstant.Value?.ToString() ?? "null";
                    break;
                }
            }

            return constructorArgumentCode;
        }
    }
}