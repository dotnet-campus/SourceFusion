using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 从标记的方法导出类型
/// </summary>
[Generator(LanguageNames.CSharp)]
public class TelescopeExportTypeToMethodIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        Debugger.Launch();
#endif
        // 可以被 IDE 选择不生成的代码，但是在完全生成输出时将会跑
        // 这里可以用来存放具体实现的代码，将不影响用户代码的语义，而不是用来做定义的代码
        //context.RegisterImplementationSourceOutput();

        // 在所有逻辑执行之前将会开始跑的代码，参与到用户代码里面，影响用户代码的语义
        // 一般是用来输出一些定义的代码
        context.RegisterPostInitializationOutput(static context =>
        {
            var assembly = typeof(TelescopeExportTypeToMethodIncrementalGenerator).Assembly;
            var telescopeExportAttributeCodeStream = assembly.GetManifestResourceStream("dotnetCampus.Telescope.SourceGeneratorAnalyzers.EmbeddedResourceCode.TelescopeExportAttribute.cs")!;
            var sourceText = SourceText.From(telescopeExportAttributeCodeStream,
                // error : Unhandled exception. System.ArgumentException: SourceText cannot be embedded. Provide encoding or canBeEmbedded = true at construction. (Parameter 'text')
                canBeEmbedded: true);
            context.AddSource("TelescopeExportAttribute", sourceText);
        });

        var incrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((node, token) =>
            {
                if (node is MethodDeclarationSyntax methodDeclarationSyntax)
                {
                    // 标记 TelescopeExportAttribute 特性
                    if (methodDeclarationSyntax.AttributeLists.SelectMany(t => t.Attributes).Any(t => t.ToString().Contains("TelescopeExport")))
                    {
                        // 方法是 Partial 的
                        if (methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                        {
                            return true;
                        }
                    }
                }

                return false;
            },
            (syntaxContext, token) =>
            {

                return syntaxContext;
            });
        context.RegisterImplementationSourceOutput(incrementalValuesProvider, (productionContext, syntaxContext) =>
        {

        });
    }
}