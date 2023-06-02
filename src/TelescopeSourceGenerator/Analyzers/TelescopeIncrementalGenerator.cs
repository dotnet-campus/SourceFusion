using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

[Generator(LanguageNames.CSharp)]
public class TelescopeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();

        IncrementalValuesProvider<GeneratorSyntaxContext> generatorSyntaxContextIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((n, c) => true, (g, c) =>
        {
            return g;
        });

        var collect = generatorSyntaxContextIncrementalValuesProvider.Collect();

        context.RegisterSourceOutput(collect, (sourceProductionContext, generatorSyntaxContext) =>
        {
            foreach (var syntaxContext in generatorSyntaxContext)
            {
                var model = syntaxContext.SemanticModel;
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