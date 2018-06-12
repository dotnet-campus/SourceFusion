using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cvte.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cvte.Compiler.CompileTime
{
    public class CompileFile
    {
        public CompileFile(string fileName)
        {
            //var originalText = File.ReadAllText(file);
            //var syntaxTree = CSharpSyntaxTree.ParseText(originalText);

            //var compileTypeVisitor = new CompileTypeVisitor();
            //var classDeclarationVisitor = new ClassDeclarationVisitor();
            //classDeclarationVisitor.Visit(tree.GetRoot());
            //return classDeclarationVisitor.Classes;

            //var declarations = ClassDeclarationVisitor.VisiteSyntaxTree(syntaxTree);
            //foreach (var declaration in declarations)
            //{
            //    if (declaration.Attributes.Any(x => x == "CodeTransform"))
            //    {
            //        var excludedFiles = InvokeCodeTransformer(file, declaration, syntaxTree);
            //        yield return file;
            //        foreach (var excludedFile in excludedFiles)
            //        {
            //            yield return excludedFile;
            //        }
            //    }
            //}
        }

        private FileInfo _file { get; }

        public ICompileType[] Types { get; }

        /// <summary>
        /// 编译指定语法树中的源码，以获取其中定义的类型（类型名称由参数 <paramref name="originalClassName"/> 指定）。
        /// </summary>
        /// <param name="syntaxTree">整个文件的语法树。</param>
        /// <returns>文件中的转换器的类型。</returns>
        private Type[] Compile(SyntaxTree syntaxTree)
        {
            var assemblyName = $"{_file.Name}.g";
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
                    return assembly.GetTypes();
                }

                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                throw new CompilingException(failures.Select(x => $"{x.Id}: {x.GetMessage()}"));
            }
        }
    }
}
