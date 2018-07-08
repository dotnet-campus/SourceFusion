using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cvte.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cvte.Compiler.CompileTime
{
    /// <summary>
    /// 编译文件
    /// </summary>
    internal class CompileFile
    {
        /// <summary>
        /// 创建编译文件
        /// </summary>
        /// <param name="fullName"></param>
        public CompileFile(string fullName)
        {
            FullName = fullName;
            Name = Path.GetFileName(fullName);
            var originalText = File.ReadAllText(fullName);
            _syntaxTree = CSharpSyntaxTree.ParseText(originalText);

            var compileTypeVisitor = new CompileTypeVisitor();
            compileTypeVisitor.Visit(_syntaxTree.GetRoot());
            Types = compileTypeVisitor.Types.ToList();
        }

        private readonly SyntaxTree _syntaxTree;

        /// <summary>
        /// 获取文件名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取文件的完整目录
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// 获取找到的类
        /// </summary>
        public IReadOnlyCollection<ICompileType> Types { get; }

        /// <summary>
        /// 编译指定语法树中的源码，以获取其中定义的类型。
        /// </summary>
        /// <returns>文件中已发现的所有类型。</returns>
        public Type[] Compile()
        {
            var assemblyName = $"{Name}.g";
            var compilation = CSharpCompilation.Create(assemblyName, new[] {_syntaxTree},
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Select(x => MetadataReference.CreateFromFile(x.Location)));

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
