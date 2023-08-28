using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static dotnetCampus.Telescope.SourceGeneratorAnalyzers.TelescopeExportTypeToMethodIncrementalGenerator;

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
                    if (namedTypeSymbol.TypeArguments.Length == 1 && ValueTupleInfoParser.TryParse(namedTypeSymbol.TypeArguments[0], out ValueTupleInfo valueTupleInfo) && valueTupleInfo.ItemList.Count == 3)
                    {
                        if (TypeSymbolHelper.TypeSymbolToFullName(valueTupleInfo.ItemList[0].ItemType) != "global::System.Type")
                        {
                            // 这就是错误的
                        }

                        var funcTypeSymbol = (INamedTypeSymbol) valueTupleInfo.ItemList[2].ItemType;
                        // 准备导出的类型的基类型
                        var expectedClassBaseType = funcTypeSymbol.TypeArguments[0];

                        // 表示的特性
                        var expectedClassAttributeType = valueTupleInfo.ItemList[1].ItemType;

                        return new ExportMethodReturnTypeCollectionResult(expectedClassBaseType, expectedClassAttributeType,
                            exportTypeCollectionResult, new ValueTupleExportMethodReturnTypeInfo(valueTupleInfo)
                            {
                                IsIEnumerable = true
                            });
                    }
                }
            }

            // 其他不认识的，要告诉开发者不能这样写哦
            return new ExportMethodReturnTypeCollectionDiagnostic() as IExportMethodReturnTypeCollectionResult;
        });

        // 这是有定义出错的，需要反馈给到开发者的
        var diagnosticIncrementalValuesProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider.Select((t, _) => t as ExportMethodReturnTypeCollectionDiagnostic).Where(t => t is not null);

        //context.RegisterSourceOutput(diagnosticIncrementalValuesProvider , (productionContext, diagnostic) =>
        //{
        //    productionContext.ReportDiagnostic();
        //});

        // 收集到了期望收集的内容，将开始进行整个项目的类型收集

        // 将这些需要包含引用程序集的加进来返回值类型。因为标记导出支持带引用程序集的
        var assemblyReferenceExportReturnTypeProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider
            .Select((t, _) => t as ExportMethodReturnTypeCollectionResult)
            // 只有非空且包含引用程序集的，才加入
            .Where(t => t is not null && t.ExportTypeCollectionResult.IncludeReference)
            .Select((t, _) => t!)
            .Collect();

        // 收集引用的程序集的类型
        var referenceAssemblyTypeIncrementalValueProvider = context.CompilationProvider.Combine(assemblyReferenceExportReturnTypeProvider).Select((tuple, token) =>
        {
            var compilation = tuple.Left;

            // 所有导出类型的定义逻辑
            var exportMethodReturnTypeCollectionResults = tuple.Right;

            // 获取到所有引用程序集
            var referencedAssemblySymbols = compilation.SourceModule.ReferencedAssemblySymbols;

            var candidateClassList = new List<CandidateClassTypeResult>();

            foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResults)
            {
                var assemblyClassTypeSymbolList = new List<INamedTypeSymbol>();
                var candidateClassTypeResult = new CandidateClassTypeResult(exportMethodReturnTypeCollectionResult, assemblyClassTypeSymbolList);
                candidateClassList.Add(candidateClassTypeResult);

                // 期望继承的基础类型
                var expectedClassBaseType = exportMethodReturnTypeCollectionResult.ExpectedClassBaseType;
                // 过滤程序集，只有引用了期望继承的基础类型所在程序集的，才可以被收集到。如果没有引用，那自然写不出继承基础类型的代码

                var visited = new Dictionary<IAssemblySymbol, bool /*是否引用*/>(SymbolEqualityComparer.Default);

                foreach (var referencedAssemblySymbol in referencedAssemblySymbols)
                {
                    if (!AssemblySymbolHelper.IsReference(referencedAssemblySymbol, expectedClassBaseType.ContainingAssembly, visited))
                    {
                        // 如果当前程序集没有直接或间接继承期望继承的基础类型所在程序集，那就证明当前程序集一定不存在任何可能被收集的类型
                        continue;
                    }

                    var isInternalsVisibleTo = referencedAssemblySymbol.GivesAccessTo(compilation.Assembly);

                    foreach (var assemblyClassTypeSymbol in AssemblySymbolHelper.GetAllTypeSymbol(referencedAssemblySymbol))
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
            .Select((t, _) => t as ExportMethodReturnTypeCollectionResult)
            .Where(t => t is not null)
            .Select((t, _) => t!)
            .Collect();

        // 先收集整个项目里面所有的类型
        var candidateClassCollectionResultIncrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, _) =>
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
                        return assemblyClassTypeSymbol;
                    }

                    return null;
                })
            .Where(t => t != null)
            .Select((t, _) => t!)
            .Combine(returnTypeCollectionIncrementalValuesProvider)
            .Select((tuple, _) =>
            {
                var assemblyClassTypeSymbol = tuple.Left;
                var exportMethodReturnTypeCollectionResultArray = tuple.Right;

                var result = new List<CandidateClassTypeResult>();

                foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResultArray)
                {
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
            .Where(t => t is not null)
            .Select((t, _) => t!);

        var collectionResultIncrementalValueProvider = referenceAssemblyTypeIncrementalValueProvider.Combine(candidateClassCollectionResultIncrementalValuesProvider.Collect())
            .SelectMany((tuple, _) => { return tuple.Right.Add(tuple.Left); })
            .Collect()
            .Select((array, _) =>
            {
                // 去掉重复的定义
                var dictionary = new Dictionary<ExportMethodReturnTypeCollectionResult, List<INamedTypeSymbol>>();

                foreach (var candidateClassCollectionResult in array)
                {
                    foreach (var candidateClassTypeResult in candidateClassCollectionResult.CandidateClassTypeResultList)
                    {
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

        // 可以被 IDE 选择不生成的代码，但是在完全生成输出时将会跑
        // 这里可以用来存放具体实现的代码，将不影响用户代码的语义，而不是用来做定义的代码
        context.RegisterImplementationSourceOutput(collectionResultIncrementalValueProvider,
            (productionContext, result) =>
            {
                foreach (var item in result)
                {
                    /*
                        private static partial IEnumerable<(Type, F1Attribute xx, Func<DemoLib1.F1> xxx)> ExportFooEnumerable()
                        {
                            yield return (typeof(CurrentFoo), new F1Attribute(), () => new CurrentFoo());
                        }
                     */
                    var exportMethodReturnTypeCollectionResult = item.Key;
                    var list = item.Value;

                    var methodSource = new StringBuilder();

                    if (exportMethodReturnTypeCollectionResult.ExportMethodReturnTypeInfo is ValueTupleExportMethodReturnTypeInfo valueTupleExportMethodReturnTypeInfo)
                    {
                        var exportPartialMethodSymbol = exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol;

                        var accessibilityCode =
                            exportPartialMethodSymbol.DeclaredAccessibility.ToCSharpCode();
                        methodSource.Append(accessibilityCode).Append(' ');

                        if (exportPartialMethodSymbol.IsStatic)
                        {
                            methodSource.Append("static ");
                        }

                        methodSource.Append("partial ");

                        if (!valueTupleExportMethodReturnTypeInfo.IsIEnumerable)
                        {
                            // 还没支持其他返回值的情况
                            throw new NotSupportedException();
                        }

                        methodSource.Append("global::System.Collections.Generic.IEnumerable<");
                        methodSource.Append('(');
                        var valueTupleInfo = valueTupleExportMethodReturnTypeInfo.ValueTupleInfo;
                        for (var i = 0; i < valueTupleInfo.ItemList.Count; i++)
                        {
                            var info = valueTupleInfo.ItemList[i];

                            if (i != valueTupleInfo.ItemList.Count - 1)
                            {
                                var type = TypeSymbolHelper.TypeSymbolToFullName(info.ItemType);
                                methodSource.Append(type).Append(' ');
                                methodSource.Append(info.ItemName);

                                methodSource.Append(',').Append(' ');
                            }
                            else
                            {
                                var type = TypeSymbolHelper.TypeSymbolToFullName(exportMethodReturnTypeCollectionResult
                                    .ExpectedClassBaseType);
                                methodSource.Append($"global::System.Func<{type}> {info.ItemName}");
                            }
                        }

                        methodSource.Append(')');
                        methodSource.Append('>');
                        methodSource.Append(' ');
                        methodSource.Append(exportPartialMethodSymbol.Name);
                        methodSource.AppendLine("()");
                        methodSource.AppendLine("{");

                        foreach (var namedTypeSymbol in list)
                        {
                            // yield return (typeof(CurrentFoo), new F1Attribute(), () => new CurrentFoo());

                            var attribute = namedTypeSymbol.GetAttributes().First(t =>
                                SymbolEqualityComparer.Default.Equals(t.AttributeClass,
                                    exportMethodReturnTypeCollectionResult
                                        .ExpectedClassAttributeType));
                            var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(attribute);

                            var typeName = TypeSymbolHelper.TypeSymbolToFullName(namedTypeSymbol);
                            methodSource.AppendLine(IndentSource($"    yield return (typeof({typeName}), {attributeCreatedCode}, () => new {typeName}());",
                                numIndentations: 1));
                        }
                        methodSource.AppendLine("}");
                    }

                    var source = methodSource.ToString();

                    var partialClassType = (INamedTypeSymbol) exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.ReceiverType!;

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
    {IndentSource(source, Math.Max(1, declarationCount - 1))}
}}";
                        stringBuilder.AppendLine(IndentSource(partialContextImplementation, numIndentations: declarationCount));

                        if (isIncludeNamespace)
                        {
                            stringBuilder.AppendLine("}");
                        }

                        var fileName = $"{partialClassType.Name}-{exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.Name}";
                        productionContext.AddSource(fileName,stringBuilder.ToString());

//#if DEBUG
//                        var debugFolder = @"F:\temp";
//                        if (Directory.Exists(debugFolder))
//                        {
//                            var debugFile = Path.Combine(debugFolder, fileName);
//                            File.WriteAllText(debugFile, stringBuilder.ToString());
//                        }
//#endif
                    }
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



    private static string IndentSource(string source, int numIndentations)
    {
        Debug.Assert(numIndentations >= 1);
        return source.Replace("\r", "").Replace("\n", $"\r\n{new string(' ', 4 * numIndentations)}");
        //return source.Replace(Environment.NewLine, $"{Environment.NewLine}{new string(' ', 4 * numIndentations)}"); // 4 spaces per indentation.
    }

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

    class ExportTypeCollectionResult : IEquatable<ExportTypeCollectionResult>
    {
        public ExportTypeCollectionResult(IMethodSymbol methodSymbol, GeneratorSyntaxContext generatorSyntaxContext)
        {
            ExportPartialMethodSymbol = methodSymbol;
            GeneratorSyntaxContext = generatorSyntaxContext;
        }

        /// <summary>
        /// 是否包含引用的程序集和 DLL 里面的类型导出。默认只导出当前程序集
        /// </summary>
        public bool IncludeReference { set; get; }
        /// <summary>
        /// 程序集里面标记了导出的分部方法，将用来生成代码
        /// </summary>
        public IMethodSymbol ExportPartialMethodSymbol { get; }
        public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

        public bool Equals(ExportTypeCollectionResult? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return GeneratorSyntaxContext.Equals(other.GeneratorSyntaxContext) && SymbolEqualityComparer.Default.Equals(ExportPartialMethodSymbol, other.ExportPartialMethodSymbol);
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
                return (ExportPartialMethodSymbol.GetHashCode() * 397) ^ GeneratorSyntaxContext.GetHashCode();
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

    /// <summary>
    /// 导出的方法的导出类型返回值结果
    /// </summary>
    class ExportMethodReturnTypeCollectionResult : IExportMethodReturnTypeCollectionResult
    {
        public ExportMethodReturnTypeCollectionResult(ITypeSymbol expectedClassBaseType, ITypeSymbol? expectedClassAttributeType, ExportTypeCollectionResult exportTypeCollectionResult, IExportMethodReturnTypeInfo exportMethodReturnTypeInfo)
        {
            ExpectedClassBaseType = expectedClassBaseType;
            ExpectedClassAttributeType = expectedClassAttributeType;
            ExportTypeCollectionResult = exportTypeCollectionResult;
            ExportMethodReturnTypeInfo = exportMethodReturnTypeInfo;
        }

        /// <summary>
        /// 期望收集的类型所继承的基础类型
        /// </summary>
        public ITypeSymbol ExpectedClassBaseType { get; }

        /// <summary>
        /// 期望类型标记的特性，可选
        /// </summary>
        public ITypeSymbol? ExpectedClassAttributeType { get; }

        public ExportTypeCollectionResult ExportTypeCollectionResult { get; }

        /// <summary>
        /// 程序集里面标记了导出的分部方法，将用来生成代码
        /// </summary>
        public IMethodSymbol ExportPartialMethodSymbol => ExportTypeCollectionResult.ExportPartialMethodSymbol;

        /// <summary>
        /// 导出类型的返回类型信息
        /// </summary>
        public IExportMethodReturnTypeInfo ExportMethodReturnTypeInfo { get; }

        /// <summary>
        /// 判断传入的程序集类型满足当前的要求条件
        /// </summary>
        /// <param name="assemblyClassTypeSymbol"></param>
        /// <returns></returns>
        public bool IsMatch(INamedTypeSymbol assemblyClassTypeSymbol)
        {
            if (assemblyClassTypeSymbol.IsAbstract)
            {
                // 抽象类不能提供
                return false;
            }

            // 先判断是否继承，再判断是否标记特性
            if (!TypeSymbolHelper.IsInherit(assemblyClassTypeSymbol, ExpectedClassBaseType))
            {
                // 没有继承基类，那就是不符合了
                return false;
            }

            if (ExpectedClassAttributeType is null)
            {
                // 如果没有特性要求，那就返回符合
                return true;
            }

            foreach (var attributeData in assemblyClassTypeSymbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, ExpectedClassAttributeType))
                {
                    return true;
                }
            }

            return false;
        }
    }

    class ExportMethodReturnTypeCollectionDiagnostic : IExportMethodReturnTypeCollectionResult
    {
    }

    /// <summary>
    /// 候选收集的结果
    /// </summary>
    /// 包含所有的导出结果。比如项目里面有多个导出方法，每一个导出方法就是一个 CandidateClassTypeResult 对象
    class CandidateClassCollectionResult
    {
        public CandidateClassCollectionResult(IReadOnlyList<CandidateClassTypeResult> candidateClassTypeResultList)
        {
            CandidateClassTypeResultList = candidateClassTypeResultList;
        }

        public IReadOnlyList<CandidateClassTypeResult> CandidateClassTypeResultList { get; }
    }

    /// <summary>
    /// 候选的导出类型结果
    /// </summary>
    /// 包含标记导出的方法信息，以及程序集里面导出的类型
    class CandidateClassTypeResult
    {
        public CandidateClassTypeResult(ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult, IReadOnlyList<INamedTypeSymbol> assemblyClassTypeSymbolList)
        {
            ExportMethodReturnTypeCollectionResult = exportMethodReturnTypeCollectionResult;
            AssemblyClassTypeSymbolList = assemblyClassTypeSymbolList;
        }

        public ExportMethodReturnTypeCollectionResult ExportMethodReturnTypeCollectionResult { get; }

        public IReadOnlyList<INamedTypeSymbol> AssemblyClassTypeSymbolList { get; }
    }
}