using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cvte.Compiler.CompileTime;
using Cvte.Compiler.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Cvte.Compiler.Templates
{
    internal class TemplateTransformer
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
        /// 获取编译期的程序集。
        /// </summary>
        private readonly CompileAssembly _assembly;

        /// <summary>
        /// 获取编译期执行上下文。
        /// </summary>
        private readonly CompilingContext _compilingContext;

        internal TemplateTransformer(string workingFolder, string intermediateFolder, CompileAssembly assembly)
        {
            _workingFolder = workingFolder;
            _intermediateFolder = intermediateFolder;
            _assembly = assembly;
            _compilingContext = new CompilingContext(assembly);
        }

        public IEnumerable<string> Transform()
        {
            return from assemblyFile in _assembly.Files
                let compileType = assemblyFile.Types.FirstOrDefault()
                where compileType?.Attributes.Any(x => x.Match<CompileTimeTemplateAttribute>()) is true
                select TransformTemplate(assemblyFile);
        }

        private string TransformTemplate(CompileFile assemblyFile)
        {
            // 读取文件，解析其语法树。
            var originalText = File.ReadAllText(assemblyFile.FullName);
            var syntaxTree = CSharpSyntaxTree.ParseText(originalText);

            var visitor = new PlaceholderVisitor();
            visitor.Visit(syntaxTree.GetRoot());

            var builder = new StringBuilder();
            var currentTextPosition = 0;

            var placeholders = visitor.Placeholders;
            foreach (var placeholder in placeholders)
            {
                var actualText = placeholder.Execute(_compilingContext);

                builder.Append(originalText.Substring(currentTextPosition, placeholder.Span.Start));
                builder.Append(actualText);
                currentTextPosition = placeholder.Span.End;
            }

            builder.Append(originalText.Substring(currentTextPosition, originalText.Length - currentTextPosition));

            var targetText = builder.ToString();
            var fileName = Path.GetFileNameWithoutExtension(assemblyFile.FullName);
            var targetFile = Path.Combine(_intermediateFolder, $"{fileName}.g.cs");
            File.WriteAllText(targetFile, targetText);

            return assemblyFile.FullName;
        }
    }
}
