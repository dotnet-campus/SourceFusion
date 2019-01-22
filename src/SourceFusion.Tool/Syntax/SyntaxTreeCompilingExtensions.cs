using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dotnetCampus.SourceFusion.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.SourceFusion.Syntax
{
    /// <summary>
    /// 包含语法树编译相关的扩展方法。
    /// </summary>
    public static class SyntaxTreeCompilingExtensions
    {
        /// <summary>
        /// 编译指定语法树中的源码，以获取其中定义的类型。
        /// </summary>
        /// <returns>文件中已发现的所有类型。</returns>
        public static Type[] Compile(this SyntaxTree syntaxTree, IEnumerable<string> references, string assemblyName)
        {
            var compilation = CSharpCompilation.Create(assemblyName, new[] {syntaxTree},
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !string.IsNullOrEmpty(x.Location))
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