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
        // 先找到定义
        IncrementalValuesProvider<CollectionExportTypeResult> collectionExportMethodIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, token) =>
         {
             // 先要求是分部的方法，分部的方法必定在分部类里面，这部分判断分部类里面还可以省略
             if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.AttributeLists.Any())
             {
                 // 同时满足以下条件：
                 // 1. 是方法
                 // 2. 方法带特性
                 // 3. 是分部方法
                 if (methodDeclarationSyntax.Modifiers.Any(t => t.IsKind(SyntaxKind.PartialKeyword)))
                 {
                     return true;
                 }
             }

             return false;
         }, (generatorSyntaxContext, token) =>
         {
             // 语义分析，判断方法是否标记了 TelescopeExportAttribute 特性

             var methodDeclarationSyntax = (MethodDeclarationSyntax) generatorSyntaxContext.Node;

             // 从语法转换为语义，用于后续判断是否标记了特性
             IMethodSymbol? methodSymbol =
                 generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax, token);

             if (methodSymbol is null)
             {
                 return default;
             }

             var attributeDataArray = methodSymbol.GetAttributes();
             foreach (var attributeData in attributeDataArray)
             {
                 token.ThrowIfCancellationRequested();

                 var attributeName = attributeData.AttributeClass?.ToDisplayString(new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included));
                 if (attributeName == "global::dotnetCampus.Telescope.TelescopeExportAttribute")
                 {

                     return new CollectionExportTypeResult(methodSymbol, generatorSyntaxContext);
                 }
             }

             return null;
         })
         // 过滤不满足条件的
         .Where(t => t is not null)
         .Select((t, _) => t!);

        // 可以被 IDE 选择不生成的代码，但是在完全生成输出时将会跑
        // 这里可以用来存放具体实现的代码，将不影响用户代码的语义，而不是用来做定义的代码
        context.RegisterImplementationSourceOutput(collectionExportMethodIncrementalValuesProvider,
            (productionContext, result) =>
            {

            });

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


    }

    class CollectionExportTypeResult
    {
        public CollectionExportTypeResult(IMethodSymbol methodSymbol, GeneratorSyntaxContext generatorSyntaxContext)
        {
            MethodSymbol = methodSymbol;
            GeneratorSyntaxContext = generatorSyntaxContext;
        }

        public IMethodSymbol MethodSymbol { get; }
        public GeneratorSyntaxContext GeneratorSyntaxContext { get; }
    }
}