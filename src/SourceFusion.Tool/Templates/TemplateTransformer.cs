using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using dotnetCampus.SourceFusion.CompileTime;
using dotnetCampus.SourceFusion.Core;
using dotnetCampus.SourceFusion.Properties;
using dotnetCampus.SourceFusion.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.SourceFusion.Templates
{
    internal class TemplateTransformer
    {
        private readonly Regex _usingRegex = new Regex(@"using\sdotnetCampus\.SourceFusion[\.\w]*;\r?\n");
        private readonly Regex _attributeRegex = new Regex(@"\[CompileTimeTemplate\]");

        internal TemplateTransformer(string workingFolder, string generatedCodeFolder, CompileAssembly assembly)
        {
            _workingFolder = workingFolder;
            _generatedCodeFolder = generatedCodeFolder;
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

        /// <summary>
        /// 获取编译期的程序集。
        /// </summary>
        private readonly CompileAssembly _assembly;

        /// <summary>
        /// 获取编译期执行上下文。
        /// </summary>
        private readonly CompilingContext _compilingContext;

        /// <summary>
        /// 获取中间文件的生成路径（文件夹，相对路径）。
        /// </summary>
        private readonly string _generatedCodeFolder;

        /// <summary>
        /// 获取转换源码的工作路径。
        /// </summary>
        private readonly string _workingFolder;

        private string TransformTemplate(CompileFile assemblyFile)
        {
            // 读取文件，去掉非期望字符。
            var originalText = File.ReadAllText(assemblyFile.FullName);
            originalText = _usingRegex.Replace(originalText, "");
            originalText = originalText.Replace("[CompileTimeTemplate]", AssemblyInfo.GeneratedCodeComment);

            // 解析其语法树。
            var syntaxTree = CSharpSyntaxTree.ParseText(originalText);

            var visitor = new PlaceholderVisitor();
            visitor.Visit(syntaxTree.GetRoot());

            var builder = new StringBuilder();
            var currentTextPosition = 0;

            var placeholders = visitor.Placeholders;
            foreach (var placeholder in placeholders)
            {
                var actualText = placeholder.Fill(_compilingContext);

                builder.Append(originalText.Substring(currentTextPosition, placeholder.Span.Start));
                builder.Append(actualText);
                currentTextPosition = placeholder.Span.End;
            }

            builder.Append(originalText.Substring(currentTextPosition, originalText.Length - currentTextPosition));
            builder.Replace("[CompileTimeTemplate]", "");
            builder.Replace("using dotnetCampus.SourceFusion;", "");

            var targetText = builder.ToString();
            var fileName = Path.GetFileNameWithoutExtension(assemblyFile.FullName);
            var targetFile = Path.Combine(_generatedCodeFolder, $"{fileName}.g.cs");
            File.WriteAllText(targetFile, targetText);

            return assemblyFile.FullName;
        }
    }
}
