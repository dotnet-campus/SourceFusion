using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using dotnetCampus.Compiling.CompileTime;
using dotnetCampus.Compiling.Core;
using dotnetCampus.Compiling.Syntax;
using dotnetCampus.SourceFusion.Attributes;
using dotnetCampus.SourceFusion.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.Compiling.Templates
{
    internal class TemplateTransformer
    {
        private readonly ProjectCompilingContext _context;

        private readonly Regex _usingRegex = new Regex(@"using\sdotnetCampus\.SourceFusion[\.\w]*;\r?\n",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly Regex _projectPropertyRegex = new Regex(@"\$\((?<propertyName>[_\w]+)\)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        internal TemplateTransformer(ProjectCompilingContext context)
        {
            _context = context;
            _workingFolder = context.WorkingFolder;
            _generatedCodeFolder = context.GeneratedCodeFolder;
            _assembly = context.Assembly;
            _compilingContext = new CompilingContext(context.Assembly);
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
            originalText = originalText.Replace("[CompileTimeTemplate]", AssemblyInfo.GeneratedCodeAttribute);
            originalText = _usingRegex.Replace(originalText, "");
            foreach (Match match in _projectPropertyRegex.Matches(originalText))
            {
                var propertyName = match.Groups["propertyName"].Value;
                var propertyValue = _context.GetProperty(propertyName);
                originalText = originalText.Replace(match.Value, propertyValue);
            }

            // 解析其语法树。
            var symbols = _context.PreprocessorSymbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var syntaxTree = CSharpSyntaxTree.ParseText(originalText, new CSharpParseOptions(
                LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Regular, symbols));

            // 访问语法节点。
            var visitor = new PlaceholderVisitor();
            visitor.Visit(syntaxTree.GetRoot());

            // 初始化字符串。
            var builder = new StringBuilder(AssemblyInfo.GeneratedCodeComment);
            var namespaceIndex = builder.Length;
            var currentTextPosition = 0;

            // 替换占位符。
            var placeholders = visitor.Placeholders;
            foreach (var placeholder in placeholders)
            {
                var actualText = placeholder.Fill(_compilingContext);

                builder.Append(
                    originalText.Substring(currentTextPosition, placeholder.Span.Start - currentTextPosition));
                builder.Append(actualText);
                currentTextPosition = placeholder.Span.End;
            }

            // 把文件剩余的部分拼接起来。
            builder.Append(originalText.Substring(currentTextPosition));

            // 去掉原来的命名空间，添加上新补充的命名空间。
            var requiredNamespaces = string.Join(Environment.NewLine, placeholders
                .SelectMany(x => x.RequiredNamespaces)
                .Union(visitor.Namespaces.Select(x => x.@namespace))
                .Distinct()
                .OrderBy(s => s, new SystemFirstNamespaceComparer())
                .Select(x => $"using {x};"));
            var namespaceStart = visitor.Namespaces.Min(x => x.span.Start);
            var namespaceEnd = visitor.Namespaces.Max(x => x.span.End);
            builder.Remove(namespaceIndex, namespaceEnd - namespaceStart);
            builder.Insert(namespaceIndex, requiredNamespaces);

            // 将新的代码写入到文件。
            var targetText = builder.ToString();
            var fileName = Path.GetFileNameWithoutExtension(assemblyFile.FullName);
            var targetFile = Path.Combine(_generatedCodeFolder, $"{fileName}.g.cs");
            File.WriteAllText(targetFile, targetText);

            return assemblyFile.FullName;
        }

        internal class SystemFirstNamespaceComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x is null && y is null)
                {
                    return 0;
                }

                if (x is null)
                {
                    return -1;
                }

                if (y is null)
                {
                    return 1;
                }

                if (x.StartsWith("System") && y.StartsWith("System"))
                {
                    return string.Compare(x, y, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase);
                }

                if (x.StartsWith("System"))
                {
                    return -1;
                }

                if (y.StartsWith("System"))
                {
                    return 1;
                }

                return string.Compare(x, y, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase);
            }
        }
    }
}