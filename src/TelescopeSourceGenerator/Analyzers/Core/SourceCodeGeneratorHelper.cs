using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class SourceCodeGeneratorHelper
{
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

            var generatedCodeAttributeSource = AssemblyInfo.GeneratedCodeAttribute;

            // Add the core implementation for the derived context class.
            string partialContextImplementation = $@"
{generatedCodeAttributeSource}
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
    /// <returns></returns>
    public static string IndentSource(string source, int numIndentations)
    {
        Debug.Assert(numIndentations >= 1);
        return source.Replace("\r", "").Replace("\n", $"\r\n{new string(' ', 4 * numIndentations)}");
    }
}