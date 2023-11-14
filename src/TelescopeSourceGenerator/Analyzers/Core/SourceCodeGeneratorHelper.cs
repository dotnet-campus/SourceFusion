using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class SourceCodeGeneratorHelper
{
    /// <summary>
    /// 创建分部方法的代码
    /// </summary>
    /// <param name="partialMethodSymbol"></param>
    /// <param name="methodCode">放在方法内的代码</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static string GeneratePartialMethodCode(IMethodSymbol partialMethodSymbol, string methodCode, CancellationToken token)
    {
        var methodSource = new StringBuilder();

        var accessibilityCode =
            partialMethodSymbol.DeclaredAccessibility.ToCSharpCode();
        methodSource.Append(accessibilityCode).Append(' ');

        if (partialMethodSymbol.IsStatic)
        {
            methodSource.Append("static ");
        }

        if (partialMethodSymbol.IsPartialDefinition)
        {
            methodSource.Append("partial ");
        }

        var returnType = partialMethodSymbol.ReturnType;
        GenerateTypeCode(returnType, methodSource, token);

        methodSource.Append(' ');
        methodSource.Append(partialMethodSymbol.Name);
        methodSource.AppendLine("()"); // 暂时只支持无参函数
        methodSource.AppendLine("{");
        methodSource.AppendLine(IndentSource(methodCode, 1, shouldFirstLine: true));
        methodSource.Append('}');

        return methodSource.ToString();
    }

    /// <summary>
    /// 根据传入的类型创建代码
    /// </summary>
    /// <returns></returns>
    public static void GenerateTypeCode(ITypeSymbol typeSymbol, StringBuilder output, CancellationToken token)
    {
        var returnTypeCode = TypeSymbolHelper.TypeSymbolToFullName(typeSymbol);
        output.Append(returnTypeCode);
        if (typeSymbol is INamedTypeSymbol returnTypeNamedTypeSymbol && returnTypeNamedTypeSymbol.IsGenericType)
        {
            output.Append('<');
            for (var i = 0; i < returnTypeNamedTypeSymbol.TypeArguments.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var typeArgument = returnTypeNamedTypeSymbol.TypeArguments[i];

                if (typeArgument.IsTupleType &&
                    ValueTupleInfoParser.TryParse(typeArgument, token, out var valueTupleInfo))
                {
                    output.Append('(');
                    for (var index = 0; index < valueTupleInfo.ItemList.Count; index++)
                    {
                        var info = valueTupleInfo.ItemList[index];

                        GenerateTypeCode(info.ItemType, output, token);

                        if (!string.IsNullOrEmpty(info.ItemName))
                        {
                            output.Append(' ')
                                .Append(info.ItemName);
                        }

                        if (index != valueTupleInfo.ItemList.Count - 1)
                        {
                            output.Append(',').Append(' ');
                        }
                    }

                    output.Append(')');
                }
                else
                {
                    GenerateTypeCode(typeArgument, output, token);
                }

                if (i != returnTypeNamedTypeSymbol.TypeArguments.Length - 1)
                {
                    output.Append(',');
                }
            }

            output.Append('>');
        }
    }

    /// <summary>
    /// 创建 partial 类型的代码
    /// </summary>
    /// <param name="partialClassType"></param>
    /// <param name="memberCode">放在类型里面的代码</param>
    /// <returns></returns>
    public static string GeneratePartialClassCode(INamedTypeSymbol partialClassType, string memberCode)
    {
        var symbolDisplayFormat = new SymbolDisplayFormat
        (
            // 带上命名空间和类型名
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            // 命名空间之前加上 global 防止冲突
            SymbolDisplayTypeQualificationStyle
                .NameAndContainingTypesAndNamespaces
        );
        var @namespace = partialClassType.ContainingNamespace?.ToDisplayString(symbolDisplayFormat);

        if (TryGetClassDeclarationList(partialClassType, out var declarationList))
        {
            int declarationCount = declarationList!.Count;
            /* 以下代码用来解决嵌套类型
            for (int i = 0; i < declarationCount - 1; i++)
            {
                string declarationSource = $@"
            {declarationList[declarationCount - 1 - i]}
            {{";
                sb.Append($@"
            {IndentSource(declarationSource, numIndentations: i + 1)}
            ");
             }
             */

            var isIncludeNamespace = !string.IsNullOrEmpty(@namespace);
            var stringBuilder = new StringBuilder(AssemblyInfo.GeneratedCodeComment);

            if (isIncludeNamespace)
            {
                stringBuilder.Append(@$"

namespace {@namespace}
{{");
            }

            // 不要带上 System.CodeDom.Compiler.GeneratedCodeAttribute 特性，因为一个类型如果是分部类型，可能有多个逻辑都在生成在不同的文件，如果这些文件同时都加上 GeneratedCodeAttribute 特性，将会导致 error CS0579: “global::System.CodeDom.Compiler.GeneratedCode”特性重复
            //var generatedCodeAttributeSource = AssemblyInfo.GeneratedCodeAttribute;

            // Add the core implementation for the derived context class.
            string partialContextImplementation = $@"
{declarationList[0]}
{{
    {IndentSource(memberCode, Math.Max(1, declarationCount - 1))}
}}";
            stringBuilder.AppendLine(IndentSource(partialContextImplementation, numIndentations: declarationCount));
            if (isIncludeNamespace)
            {
                stringBuilder.AppendLine("}");
            }

            return stringBuilder.ToString();
        }
        else
        {
            throw new ArgumentException($"无法为 {partialClassType} 创建代码");
        }
    }

    /// <summary>
    /// 尝试获取类型的定义
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <param name="classDeclarationList">嵌套类的定义</param>
    /// <returns></returns>
    private static bool TryGetClassDeclarationList(INamedTypeSymbol typeSymbol, out List<string>? classDeclarationList)
    {
        INamedTypeSymbol currentSymbol = typeSymbol;
        classDeclarationList = null;

        while (currentSymbol != null)
        {
            ClassDeclarationSyntax? classDeclarationSyntax = currentSymbol.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;

            if (classDeclarationSyntax != null)
            {
                SyntaxTokenList tokenList = classDeclarationSyntax.Modifiers;
                int tokenCount = tokenList.Count;

                bool isPartial = false;

                string[] declarationElements = new string[tokenCount + 2];

                for (int i = 0; i < tokenCount; i++)
                {
                    SyntaxToken token = tokenList[i];
                    declarationElements[i] = token.Text;

                    if (token.IsKind(SyntaxKind.PartialKeyword))
                    {
                        isPartial = true;
                    }
                }

                if (!isPartial)
                {
                    classDeclarationList = null;
                    return false;
                }

                declarationElements[tokenCount] = "class";
                declarationElements[tokenCount + 1] = currentSymbol.Name;

                (classDeclarationList ??= new List<string>()).Add(string.Join(" ", declarationElements));
            }

            currentSymbol = currentSymbol.ContainingType;
        }

        Debug.Assert(classDeclarationList != null);
        Debug.Assert(classDeclarationList!.Count > 0);
        return true;
    }

    /// <summary>
    /// 提供缩进的方法
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numIndentations"></param>
    /// <param name="shouldFirstLine"></param>
    /// <returns></returns>
    public static string IndentSource(string source, int numIndentations, bool shouldFirstLine = false)
    {
        Debug.Assert(numIndentations >= 1);
        var result = source.Replace("\r", "").Replace("\n", $"\r\n{new string(' ', 4 * numIndentations)}");

        if (shouldFirstLine)
        {
            result = new string(' ', 4 * numIndentations) + result;
        }

        return result;
    }
}