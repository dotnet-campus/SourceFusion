using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cvte.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cvte.Compiler
{
    /// <summary>
    /// 为编译时提供源码转换。
    /// </summary>
    internal class CodeTransformer
    {
        /// <summary>
        /// 获取转换源码的工作路径。
        /// </summary>
        private readonly string _workingFolder;

        /// <summary>
        /// 获取中间文件的生成路径（文件夹，相对路径）。
        /// </summary>
        private readonly string _intermediateFolder;

        /// <summary>
        /// 获取所有参与编译的文件。
        /// </summary>
        private readonly string[] _compilingFiles;

        /// <summary>
        /// 创建用于转换源码的 <see cref="CodeTransformer"/>。
        /// </summary>
        /// <param name="workingFolder">转换源码的工作路径。</param>
        /// <param name="intermediateFolder">中间文件的生成路径（文件夹，相对路径）。</param>
        /// <param name="compilingFiles">所有参与编译的文件。</param>
        internal CodeTransformer(string workingFolder, string intermediateFolder, IEnumerable<string> compilingFiles)
        {
            _workingFolder = Path.GetFullPath(workingFolder);
            _intermediateFolder = Path.GetFullPath(Path.Combine(workingFolder, intermediateFolder));
            if (!Directory.Exists(_intermediateFolder))
            {
                Directory.CreateDirectory(_intermediateFolder);
            }
            _compilingFiles = compilingFiles.Select(x => Path.GetFullPath(Path.Combine(workingFolder, x))).ToArray();
        }

        /// <summary>
        /// 执行代码转换。这将开始从所有的编译文件中搜索 <see cref="CodeTransformAttribute"/>，并执行其转换方法。
        /// </summary>
        internal IEnumerable<string> Transform()
        {
            foreach (var file in _compilingFiles)
            {
                var originalText = File.ReadAllText(file);
                var syntaxTree = CSharpSyntaxTree.ParseText(originalText);
                var declarations = ClassDeclarationVisitor.VisiteSyntaxTree(syntaxTree);
                foreach (var declaration in declarations)
                {
                    if (declaration.Attributes.Any(x => x == "CodeTransform"))
                    {
                        var excludedFiles = InvokeCodeTransformer(file, declaration, syntaxTree);
                        yield return file;
                        foreach (var excludedFile in excludedFiles)
                        {
                            yield return excludedFile;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行 <see cref="IPlainCodeTransformer"/> 的转换代码的方法。
        /// </summary>
        /// <param name="codeFile">此代码文件的文件路径。</param>
        /// <param name="declaration">类声明信息。</param>
        /// <param name="syntaxTree">整个文件的语法树。</param>
        private IEnumerable<string> InvokeCodeTransformer(string codeFile, ClassDeclaration declaration, SyntaxTree syntaxTree)
        {
            var type = CompileType(declaration.Name, syntaxTree);
            var transformer = (IPlainCodeTransformer) Activator.CreateInstance(type);
            var attribute = type.GetCustomAttribute<CodeTransformAttribute>();

            var sourceFiles = attribute.SourceFiles
                .Select(x => Path.GetFullPath(Path.Combine(
                    x.StartsWith("/") || x.StartsWith("\\") ? _workingFolder : Path.GetDirectoryName(codeFile), x)));
            foreach (var sourceFile in sourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                var extension = attribute.TargetType == FileType.Compile ? ".cs" : Path.GetExtension(sourceFile);
                foreach (var (i, transformedText) in Enumerable.Range(0, attribute.RepeatCount)
                    .Select(i => (i, transformer.Transform(File.ReadAllText(sourceFile), new TransformingContext(i)))))
                {
                    var targetFile = Path.Combine(_intermediateFolder, $"{fileName}.g.{i}{extension}");
                    File.WriteAllText(targetFile, transformedText, Encoding.UTF8);
                }
                if (!attribute.KeepSourceFiles)
                {
                    yield return sourceFile;
                }
            }
        }

        /// <summary>
        /// 编译指定语法树中的源码，以获取其中定义的类型（类型名称由参数 <paramref name="originalClassName"/> 指定）。
        /// </summary>
        /// <param name="originalClassName">通过语法树估算的类型名称。</param>
        /// <param name="syntaxTree">整个文件的语法树。</param>
        /// <returns>文件中的转换器的类型。</returns>
        private static Type CompileType(string originalClassName, SyntaxTree syntaxTree)
        {
            var assemblyName = $"{originalClassName}.g";
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree },
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(
                    AppDomain.CurrentDomain.GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location)));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());
                    return assembly.GetTypes().First(x => x.Name == originalClassName);
                }

                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                throw new CompilingException(failures.Select(x => $"{x.Id}: {x.GetMessage()}"));
            }
        }
    }
}
