using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

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
        IncrementalValuesProvider<ExportTypeCollectionResult> exportMethodIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, token) =>
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

                 if (attributeData.AttributeClass is null) continue;

                 var attributeName = TypeSymbolHelper.TypeSymbolToFullName(attributeData.AttributeClass);
                 if (attributeName == "global::dotnetCampus.Telescope.TelescopeExportAttribute")
                 {
                     var attributeDataNamedArguments = attributeData.NamedArguments;
                     var includeReference =
                         attributeDataNamedArguments
                             .FirstOrDefault(t => t.Key == nameof(TelescopeExportAttribute.IncludeReference)).Value
                             .Value is true;

                     return new ExportTypeCollectionResult(methodSymbol, generatorSyntaxContext)
                     {
                         IncludeReference = includeReference
                     };
                 }
             }

             return null;
         })
         // 过滤不满足条件的
         .Where(t => t is not null)
         .Select((t, _) => t!);

        // 获取方法返回值导出类型
        var exportMethodReturnTypeCollectionResultIncrementalValuesProvider = exportMethodIncrementalValuesProvider.Select((exportTypeCollectionResult, token) =>
        {
            ITypeSymbol methodSymbolReturnType = exportTypeCollectionResult.MethodSymbol.ReturnType;

            //if (methodSymbolReturnType is IArrayTypeSymbol arrayTypeSymbol)
            //{
            //    // 如果是如 partial Base[] ExportFoo() 这样的，收集起来
            //    ITypeSymbol elementType = arrayTypeSymbol.ElementType;
            //    return new ExportMethodReturnTypeCollectionResult(elementType, null, exportTypeCollectionResult.MethodSymbol) as IExportMethodReturnTypeCollectionResult;
            //}
            //else
            if (methodSymbolReturnType is INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.IsGenericType)
                {
                }

                return new ExportMethodReturnTypeCollectionResult(namedTypeSymbol, null, exportTypeCollectionResult.MethodSymbol) as IExportMethodReturnTypeCollectionResult;
            }

            // 其他不认识的，要告诉开发者不能这样写哦
            return new ExportMethodReturnTypeCollectionDiagnostic();
        });

        // 这是有定义出错的，需要反馈给到开发者的
        var diagnosticIncrementalValuesProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider.Select((t, _) => t as ExportMethodReturnTypeCollectionDiagnostic).Where(t => t is not null);

        //context.RegisterSourceOutput(diagnosticIncrementalValuesProvider , (productionContext, diagnostic) =>
        //{
        //    productionContext.ReportDiagnostic();
        //});

        // 收集所有的带返回类型，用来进行下一步的收集项目里的所有类型
        IncrementalValueProvider<ImmutableArray<ExportMethodReturnTypeCollectionResult>> returnTypeCollectionIncrementalValuesProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider
            .Select((t, _) => t as ExportMethodReturnTypeCollectionResult)
            .Where(t => t is not null)
            .Select((t, _) => t!)
            .Collect();

        // 收集到了期望收集的内容，将开始进行整个项目的类型收集

        // 先收集整个项目里面所有的类型
        var assemblyClassIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, _) =>
            {
                return syntaxNode.IsKind(SyntaxKind.ClassDeclaration);
            }, (generatorSyntaxContext, token) =>
            {
                var classDeclarationSyntax = (ClassDeclarationSyntax) generatorSyntaxContext.Node;
                // 从语法转换为语义，用于后续判断是否标记了特性
                INamedTypeSymbol? assemblyClassTypeSymbol =
                    generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
                if (assemblyClassTypeSymbol is not null && !assemblyClassTypeSymbol.IsAbstract)
                {
                    return new AssemblyClassCollectionResult(assemblyClassTypeSymbol, classDeclarationSyntax);
                }

                return null;
            })
            .Where(t => t != null)
            .Select((t, _) => t!);

        var candidateClassCollectionResultIncrementalValuesProvider = assemblyClassIncrementalValuesProvider
            .Combine(returnTypeCollectionIncrementalValuesProvider)
            .Select((tuple, token) =>
            {
                var assemblyClassCollectionResult = tuple.Left;
                var exportMethodReturnTypeCollectionResultArray = tuple.Right;

                foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResultArray)
                {
                    if (TypeSymbolHelper.IsInherit(assemblyClassCollectionResult.AssemblyClassTypeSymbol, exportMethodReturnTypeCollectionResult.ExpectedClassBaseType))
                    {
                        if (exportMethodReturnTypeCollectionResult.ExpectedClassAttributeType is null)
                        {
                            // 没有 Attribute 的要求
                        }
                    }
                }

                return new CandidateClassCollectionResult();
            })
            .Where(t => t is not null);

        // 可以被 IDE 选择不生成的代码，但是在完全生成输出时将会跑
        // 这里可以用来存放具体实现的代码，将不影响用户代码的语义，而不是用来做定义的代码
        context.RegisterImplementationSourceOutput(candidateClassCollectionResultIncrementalValuesProvider,
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

    class ExportTypeCollectionResult : IEquatable<ExportTypeCollectionResult>
    {
        public ExportTypeCollectionResult(IMethodSymbol methodSymbol, GeneratorSyntaxContext generatorSyntaxContext)
        {
            MethodSymbol = methodSymbol;
            GeneratorSyntaxContext = generatorSyntaxContext;
        }

        /// <summary>
        /// 是否包含引用的程序集和 DLL 里面的类型导出。默认只导出当前程序集
        /// </summary>
        public bool IncludeReference { set; get; }

        public IMethodSymbol MethodSymbol { get; }
        public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

        public bool Equals(ExportTypeCollectionResult? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return GeneratorSyntaxContext.Equals(other.GeneratorSyntaxContext) && SymbolEqualityComparer.Default.Equals(MethodSymbol, other.MethodSymbol);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExportTypeCollectionResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MethodSymbol.GetHashCode() * 397) ^ GeneratorSyntaxContext.GetHashCode();
            }
        }
    }

    /// <summary>
    /// 收集导出方法返回值类型
    /// </summary>
    /// 可以是收集到了，也可以是返回开发者定义错误代码
    interface IExportMethodReturnTypeCollectionResult
    {
    }

    class ExportMethodReturnTypeCollectionResult : IExportMethodReturnTypeCollectionResult
    {
        public ExportMethodReturnTypeCollectionResult(ITypeSymbol expectedClassBaseType, ITypeSymbol? expectedClassAttributeType, IMethodSymbol exportPartialMethodSymbol)
        {
            ExpectedClassBaseType = expectedClassBaseType;
            ExpectedClassAttributeType = expectedClassAttributeType;
            ExportPartialMethodSymbol = exportPartialMethodSymbol;
        }

        /// <summary>
        /// 期望收集的类型所继承的基础类型
        /// </summary>
        public ITypeSymbol ExpectedClassBaseType { get; }

        /// <summary>
        /// 期望类型标记的特性，可选
        /// </summary>
        public ITypeSymbol? ExpectedClassAttributeType { get; }

        /// <summary>
        /// 程序集里面标记了导出的分部方法，将用来生成代码
        /// </summary>
        public IMethodSymbol ExportPartialMethodSymbol { get; }
    }

    class ExportMethodReturnTypeCollectionDiagnostic : IExportMethodReturnTypeCollectionResult
    {

    }

    /// <summary>
    /// 程序集里面的类型收集结果
    /// </summary>
    class AssemblyClassCollectionResult
    {
        public AssemblyClassCollectionResult(INamedTypeSymbol assemblyClassTypeSymbol, ClassDeclarationSyntax classDeclarationSyntax)
        {
            AssemblyClassTypeSymbol = assemblyClassTypeSymbol;
            ClassDeclarationSyntax = classDeclarationSyntax;
        }

        public INamedTypeSymbol AssemblyClassTypeSymbol { get; }
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
    }

    class CandidateClassCollectionResult
    {

    }
}