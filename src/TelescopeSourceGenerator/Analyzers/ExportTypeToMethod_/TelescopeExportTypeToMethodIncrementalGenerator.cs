using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;
using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static dotnetCampus.Telescope.SourceGeneratorAnalyzers.Properties.Resources;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 从标记的方法导出类型
/// </summary>
// 形如 private static partial IEnumerable<(Type type, FooAttribute attribute, Func<FooBase> creator)> ExportFooEnumerable();
[Generator(LanguageNames.CSharp)]
public class TelescopeExportTypeToMethodIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
        // 先找到定义
        IncrementalValuesProvider<ExportTypeCollectionResult> exportMethodIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider(static (syntaxNode, _) =>
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
         }, static (generatorSyntaxContext, token) =>
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
                             .FirstOrDefault(t => t.Key == nameof(TelescopeExportAttribute.IncludeReferences)).Value
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
         .ExcludeNulls();

        // 获取方法返回值导出类型
        var exportMethodReturnTypeCollectionResultIncrementalValuesProvider = exportMethodIncrementalValuesProvider.Select(static (exportTypeCollectionResult, token) =>
        {
            ITypeSymbol methodSymbolReturnType = exportTypeCollectionResult.ExportPartialMethodSymbol.ReturnType;

            //if (methodSymbolReturnType is IArrayTypeSymbol arrayTypeSymbol)
            //{
            //    // 如果是如 partial Base[] ExportFoo() 这样的，收集起来
            //    ITypeSymbol elementType = arrayTypeSymbol.ElementType;
            //    return new ExportMethodReturnTypeCollectionResult(elementType, null, exportTypeCollectionResult.MethodSymbol) as IExportMethodReturnTypeCollectionResult;
            //}
            //else
            if (methodSymbolReturnType is INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.IsGenericType && TypeSymbolHelper.TypeSymbolToFullName(namedTypeSymbol) ==
                    "global::System.Collections.Generic.IEnumerable")
                {
                    // 尝试判断是 ValueTuple 的情况
                    // 要求符合以下定义
                    // static partial IEnumerable<(Type, FooAttribute xx, Func<Base> xxx)> ExportFooEnumerable()
                    if (namedTypeSymbol.TypeArguments.Length == 1 &&
                        namedTypeSymbol.TypeArguments[0] is INamedTypeSymbol tupleType && tupleType.IsTupleType &&
                        tupleType.TupleElements.Length > 0)
                    {
                        if (tupleType.TupleElements.Length == 3)
                        {
                            // static partial IEnumerable<(Type, FooAttribute xx, Func<Base> xxx)> ExportFooEnumerable()

                            if (TypeSymbolHelper.TypeSymbolToFullName(tupleType.TupleElements[0].Type) != "global::System.Type")
                            {
                                // 这就是错误的
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            // 表示的特性
                            var expectedClassAttributeType = tupleType.TupleElements[1].Type;

                            // Func<Base>
                            var funcTypeSymbol = (INamedTypeSymbol) tupleType.TupleElements[2].Type;
                            if (!funcTypeSymbol.IsGenericType || TypeSymbolHelper.TypeSymbolToFullName(funcTypeSymbol) != "global::System.Func")
                            {
                                // 不是 Func 的
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            // 准备导出的类型的基类型
                            var expectedClassBaseType = funcTypeSymbol.TypeArguments[0];

                            return new ExportMethodReturnTypeCollectionResult(expectedClassBaseType, expectedClassAttributeType,
                                exportTypeCollectionResult, ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator);
                        }
                        else if (tupleType.TupleElements.Length == 2)
                        {
                            // 判断是否 `partial IEnumerable<(Type type, Func<FooBase> creator)> ExportFooEnumerable();` 的情况，没有中间的 Attribute 约束，也就是只需要导出所有继承了 FooBase 的类型即可
                            if (TypeSymbolHelper.TypeSymbolToFullName(tupleType.TupleElements[0].Type) != "global::System.Type")
                            {
                                // 如果首个不是 Type 类型，这就是错误的
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            // Func<Base>
                            var funcTypeSymbol = (INamedTypeSymbol) tupleType.TupleElements[1].Type;
                            if (!funcTypeSymbol.IsGenericType || TypeSymbolHelper.TypeSymbolToFullName(funcTypeSymbol) != "global::System.Func")
                            {
                                // 不是 Func 的
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            // 准备导出的类型的基类型
                            var expectedClassBaseType = funcTypeSymbol.TypeArguments[0];
                            return new ExportMethodReturnTypeCollectionResult(expectedClassBaseType,
                                // 没有预期的特性类型
                                expectedClassAttributeType: null,
                                exportTypeCollectionResult, ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator);
                        }
                    }
                }
            }

            // 其他不认识的，要告诉开发者不能这样写哦
            return new ExportMethodReturnTypeCollectionDiagnostic(TesDiagnostics.Tes000_UnknownError) as IExportMethodReturnTypeCollectionResult;

            Location GetLocation()
            {
                var syntaxNode = exportTypeCollectionResult.GeneratorSyntaxContext.Node;
                var location = Location.Create(syntaxNode.SyntaxTree, syntaxNode.Span);
                return location;
            }

            ExportMethodReturnTypeCollectionDiagnostic ReturnTypeError(string messageKey)
            {
                return new ExportMethodReturnTypeCollectionDiagnostic
                (
                    TesDiagnostics.Tes001_MethodReturnTypeError,
                    GetLocation(),
                    TesDiagnostics.Localize(messageKey)
                );
            }
        });

        // 这是有定义出错的，需要反馈给到开发者的
        var diagnosticIncrementalValuesProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider.Select(static (t, _) => t as ExportMethodReturnTypeCollectionDiagnostic)
            .ExcludeNulls();

        context.RegisterSourceOutput(diagnosticIncrementalValuesProvider, static (productionContext, diagnostic) =>
        {
            productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
        });

        // 收集到了期望收集的内容，将开始进行整个项目的类型收集

        // 将这些需要包含引用程序集的加进来返回值类型。因为标记导出支持带引用程序集的
        var assemblyReferenceExportReturnTypeProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider
            .Select(static (t, _) => t as ExportMethodReturnTypeCollectionResult)
            // 只有非空且包含引用程序集的，才加入
            .Where(static t => t is not null && t.ExportTypeCollectionResult.IncludeReference)
            .Select(static (t, _) => t!)
            .Collect();

        // 收集引用的程序集的类型
        var referenceAssemblyTypeIncrementalValueProvider = context.CompilationProvider
            .Select(static (compilation, _) => compilation.SourceModule.ReferencedAssemblySymbols)
            .Combine(assemblyReferenceExportReturnTypeProvider).Select(static (tuple, token) =>
        {
            // 获取到所有引用程序集
            var referencedAssemblySymbols = tuple.Left;

            // 所有导出类型的定义逻辑
            var exportMethodReturnTypeCollectionResults = tuple.Right;

            var candidateClassList = new List<CandidateClassTypeResult>();

            foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResults)
            {
                token.ThrowIfCancellationRequested();

                var assemblyClassTypeSymbolList = new List<INamedTypeSymbol>();
                var candidateClassTypeResult = new CandidateClassTypeResult(exportMethodReturnTypeCollectionResult, assemblyClassTypeSymbolList);
                candidateClassList.Add(candidateClassTypeResult);

                // 期望继承的基础类型
                var expectedClassBaseType = exportMethodReturnTypeCollectionResult.ExpectedClassBaseType;

                // 当前项目的程序集，用来判断 internal 可见性
                var currentAssembly = exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.ContainingAssembly;

                var visited = new Dictionary<IAssemblySymbol, bool /*是否引用*/>(SymbolEqualityComparer.Default);

                foreach (var referencedAssemblySymbol in referencedAssemblySymbols)
                {
                    // 过滤程序集，只有引用了期望继承的基础类型所在程序集的，才可以被收集到。如果没有引用，那自然写不出继承基础类型的代码
                    token.ThrowIfCancellationRequested();
                    if (!AssemblySymbolHelper.IsReference(referencedAssemblySymbol, expectedClassBaseType.ContainingAssembly, visited))
                    {
                        // 如果当前程序集没有直接或间接继承期望继承的基础类型所在程序集，那就证明当前程序集一定不存在任何可能被收集的类型
                        continue;
                    }

                    // 判断 referencedAssemblySymbol 是否设置 internal 可见
                    var isInternalsVisibleTo = referencedAssemblySymbol.GivesAccessTo(currentAssembly);

                    foreach (var assemblyClassTypeSymbol in AssemblySymbolHelper.GetAllTypeSymbols(referencedAssemblySymbol))
                    {
                        if (!isInternalsVisibleTo &&
                            assemblyClassTypeSymbol.DeclaredAccessibility != Accessibility.Public)
                        {
                            // 如果设置不可见的，那就不要加入了
                            continue;
                        }

                        if (exportMethodReturnTypeCollectionResult.IsMatch(assemblyClassTypeSymbol))
                        {
                            assemblyClassTypeSymbolList.Add(assemblyClassTypeSymbol);
                        }
                    }
                }
            }

            return new CandidateClassCollectionResult(candidateClassList);
        });

        // 收集当前分析器所分析项目的类型
        // 收集所有的带返回类型，用来进行下一步的收集项目里的所有类型
        IncrementalValueProvider<ImmutableArray<ExportMethodReturnTypeCollectionResult>> returnTypeCollectionIncrementalValuesProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider
            .Select(static (t, _) => t as ExportMethodReturnTypeCollectionResult)
            .ExcludeNulls()
            .Collect();

        // 收集整个项目里面所有的类型
        var candidateClassCollectionResultIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, _) =>
                {
                    return syntaxNode.IsKind(SyntaxKind.ClassDeclaration);
                }, static (generatorSyntaxContext, token) =>
                {
                    var classDeclarationSyntax = (ClassDeclarationSyntax)generatorSyntaxContext.Node;
                    // 从语法转换为语义，用于后续判断是否标记了特性
                    INamedTypeSymbol? assemblyClassTypeSymbol =
                        generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
                    if (assemblyClassTypeSymbol is not null && !assemblyClassTypeSymbol.IsAbstract)
                    {
                        return assemblyClassTypeSymbol;
                    }

                    return null;
                })
            .ExcludeNulls()
            .Combine(returnTypeCollectionIncrementalValuesProvider)
            .Select(static (tuple, token) =>
            {
                var assemblyClassTypeSymbol = tuple.Left;
                var exportMethodReturnTypeCollectionResultArray = tuple.Right;

                var result = new List<CandidateClassTypeResult>();

                foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResultArray)
                {
                    token.ThrowIfCancellationRequested();
                    if (exportMethodReturnTypeCollectionResult.IsMatch(assemblyClassTypeSymbol))
                    {
                        result.Add(new CandidateClassTypeResult(exportMethodReturnTypeCollectionResult,
                            new[] { assemblyClassTypeSymbol }));
                    }
                }

                if (result.Count > 0)
                {
                    return new CandidateClassCollectionResult(result);
                }
                else
                {
                    return null;
                }
            })
            .ExcludeNulls();

        var collectionResultIncrementalValueProvider = referenceAssemblyTypeIncrementalValueProvider.Combine(candidateClassCollectionResultIncrementalValuesProvider.Collect())
            .SelectMany(static (tuple, _) => { return tuple.Right.Add(tuple.Left); })
            .Collect()
            .Select(static (array, token) =>
            {
                // 去掉重复的定义
                var dictionary = new Dictionary<ExportMethodReturnTypeCollectionResult, List<INamedTypeSymbol>>();

                foreach (var candidateClassCollectionResult in array)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (var candidateClassTypeResult in candidateClassCollectionResult.CandidateClassTypeResultList)
                    {
                        token.ThrowIfCancellationRequested();
                        var result = candidateClassTypeResult.ExportMethodReturnTypeCollectionResult;

                        if (!dictionary.TryGetValue(result, out var list))
                        {
                            list = new List<INamedTypeSymbol>();
                            dictionary.Add(result, list);
                        }

                        list.AddRange(candidateClassTypeResult.AssemblyClassTypeSymbolList);
                    }
                }

                return dictionary;
            });

        // 转换为源代码输出
        // 源代码输出放在 Select 可以随时打断，实际 VisualStudio 性能会比放在 RegisterImplementationSourceOutput 高很多
        var sourceCodeProvider = collectionResultIncrementalValueProvider.Select(static (result, token) =>
        {
           var codeList = new List<(string /*FileName*/, string /*SourceCode*/)>(result.Count);
            foreach (var item in result)
            {
                token.ThrowIfCancellationRequested();

                var exportMethodReturnTypeCollectionResult = item.Key;
                var list = item.Value;

                var methodCode =
                    ExportMethodCodeGenerator.GenerateSourceCode(exportMethodReturnTypeCollectionResult, list, token);

                var partialClassType = (INamedTypeSymbol) exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.ReceiverType!;

                var code = SourceCodeGeneratorHelper.GeneratePartialClassCode(partialClassType, methodCode);

                var fileName =
                    $"{partialClassType.Name}-{exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.Name}";

                codeList.Add((fileName, code));
            }

            return codeList;
        });

        // 可以被 IDE 选择不生成的代码，但是在完全生成输出时将会跑
        // 这里可以用来存放具体实现的代码，将不影响用户代码的语义，而不是用来做定义的代码
        context.RegisterImplementationSourceOutput(sourceCodeProvider,
            static (productionContext, result) =>
            {
                foreach (var (fileName, code) in result)
                {
                    productionContext.AddSource(fileName, code);
                }
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

    /// <summary>
    /// 提供缩进的方法
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numIndentations"></param>
    /// <returns></returns>
    private static string IndentSource(string source, int numIndentations)
    {
        return SourceCodeGeneratorHelper.IndentSource(source, numIndentations);
    }
}