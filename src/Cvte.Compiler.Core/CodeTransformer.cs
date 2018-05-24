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
    public class CodeTransformer
    {
        private readonly string _workingFolder;
        private readonly string _intermediateFolder;
        private readonly string[] _compilingFiles;

        public CodeTransformer(string workingFolder, string intermediateFolder, IEnumerable<string> compilingFiles)
        {
            _workingFolder = Path.GetFullPath(workingFolder);
            _intermediateFolder = Path.GetFullPath(Path.Combine(workingFolder, intermediateFolder));
            _compilingFiles = compilingFiles.Select(x => Path.GetFullPath(Path.Combine(workingFolder, x))).ToArray();
        }

        public void Transform()
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
                        var type = CompileType(declaration.Name, syntaxTree);
                        var attribute = type.GetCustomAttribute<CodeTransformAttribute>();
                        var transformer = (IPlainCodeTransformer) Activator.CreateInstance(type);

                        foreach (var sourceFile in attribute.SourceFiles
                            .Select(x => Path.GetFullPath(Path.Combine(_workingFolder, x))))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                            var extension = Path.GetExtension(sourceFile);
                            foreach (var (i, transformedText) in Enumerable.Range(0, attribute.RepeatCount)
                                .Select(i => (i, transformer.Transform(File.ReadAllText(sourceFile), new TransformingContext(i)))))
                            {
                                var targetFile = Path.Combine(_intermediateFolder, $"{fileName}.g.{i}{extension}");
                                File.WriteAllText(targetFile, transformedText, Encoding.UTF8);
                            }
                        }

                    }
                }
            }
        }


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
