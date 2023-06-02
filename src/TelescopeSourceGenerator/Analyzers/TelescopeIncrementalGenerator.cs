using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

[Generator(LanguageNames.CSharp)]
public class TelescopeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();

        var assemblyAttributeSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, cancellationToken) =>
        {
            // [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
            return syntaxNode.IsKind(SyntaxKind.AttributeList) && syntaxNode.ChildNodes().Any(subNode=> subNode.IsKind(SyntaxKind.AttributeTargetSpecifier));
        }, (generatorSyntaxContext, cancellationToken) =>
        {
            var descendantNodes = generatorSyntaxContext.Node.DescendantNodes(node => node.IsKind(SyntaxKind.Attribute));

            var symbol = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(generatorSyntaxContext.Node);

            

            if (generatorSyntaxContext.Node is AttributeListSyntax attributeListSyntax)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    // g
                    var preprocessingSymbolInfo = generatorSyntaxContext.SemanticModel.GetPreprocessingSymbolInfo(attributeSyntax);
                    var enclosingSymbol = generatorSyntaxContext.SemanticModel.GetEnclosingSymbol(attributeSyntax.Span.Start);

                    // 太多项了
                    //generatorSyntaxContext.SemanticModel.LookupNamespacesAndTypes()

                    var declaredSymbol = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(attributeSyntax.Name);
                    var declaredSymbol2 = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(attributeSyntax);

                    var symbolInfo1 = generatorSyntaxContext.SemanticModel.GetSymbolInfo(attributeSyntax);
                    var symbolInfo2 = generatorSyntaxContext.SemanticModel.GetSymbolInfo(attributeSyntax.Name);
                    var symbolInfo1Symbol = symbolInfo1.Symbol as IMethodSymbol;
                    var displayString = symbolInfo1Symbol.ReceiverType.ToDisplayString(new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Included,SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
                }
            }

            return generatorSyntaxContext;
        }).Collect();

        IncrementalValuesProvider<GeneratorSyntaxContext> generatorSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((n, c) => true, (g, c) =>
        {
            return g;
        });

        var collect = generatorSyntaxContextIncrementalValuesProvider.Combine(assemblyAttributeSyntaxContextIncrementalValuesProvider).Collect();

        context.RegisterSourceOutput(collect, (sourceProductionContext, generatorSyntaxContext) =>
        {
            foreach (var syntaxContext in generatorSyntaxContext)
            {
                //var model = syntaxContext.SemanticModel;
            }
        });
    }
}

class DelegateE<T> : IEqualityComparer<T>
{
    public bool Equals(T x, T y)
    {
        throw new System.NotImplementedException();
    }

    public int GetHashCode(T obj)
    {
        throw new System.NotImplementedException();
    }
}