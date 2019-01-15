using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using dotnetCampus.SourceFusion.CompileTime;
using dotnetCampus.SourceFusion.Core;
using dotnetCampus.SourceFusion.Properties;

namespace dotnetCampus.SourceFusion.Transforming
{
    /// <summary>
    /// 为编译时提供源码转换。
    /// </summary>
    internal class CodeTransformer
    {
        /// <summary>
        /// 创建用于转换源码的 <see cref="CodeTransformer"/>。
        /// </summary>
        /// <param name="context">项目工作区上下文信息。</param>
        internal CodeTransformer(ProjectCompilingContext context)
        {
            _workingFolder = context.WorkingFolder;
            _generatedCodeFolder = context.GeneratedCodeFolder;
            _assembly = context.Assembly;
        }
        
        /// <summary>
        /// 获取编译期的程序集。
        /// </summary>
        private readonly CompileAssembly _assembly;

        /// <summary>
        /// 获取中间文件的生成路径（文件夹，相对路径）。
        /// </summary>
        private readonly string _generatedCodeFolder;

        /// <summary>
        /// 获取转换源码的工作路径。
        /// </summary>
        private readonly string _workingFolder;

        /// <summary>
        /// 执行 <see cref="IPlainCodeTransformer"/> 的转换代码的方法。
        /// </summary>
        /// <param name="codeFile">此代码文件的文件路径。</param>
        /// <param name="transformer">编译好的代码转换类实例。</param>
        private IEnumerable<string> InvokeCodeTransformer(string codeFile, IPlainCodeTransformer transformer)
        {
            var attribute = transformer.GetType().GetCustomAttribute<CompileTimeCodeAttribute>();

            var sourceFiles = attribute.SourceFiles
                .Select(x => Path.GetFullPath(Path.Combine(x.StartsWith("/") || x.StartsWith("\\")
                    ? _workingFolder
                    : Path.GetDirectoryName(codeFile), x)));
            foreach (var sourceFile in sourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                var extension = attribute.TargetType == FileType.Compile ? ".cs" : Path.GetExtension(sourceFile);

                var text = File.ReadAllText(sourceFile);
                for (var i = 0; i < attribute.RepeatCount; i++)
                {
                    var transformedText = transformer.Transform(text, new TransformingContext(i));
                    var targetFile = Path.Combine(_generatedCodeFolder, $"{fileName}.g.{i}{extension}");
                    File.WriteAllText(targetFile, AssemblyInfo.GeneratedCodeComment + transformedText, Encoding.UTF8);
                }

                if (!attribute.KeepSourceFiles)
                {
                    yield return sourceFile;
                }
            }
        }

        /// <summary>
        /// 执行代码转换。这将开始从所有的编译文件中搜索 <see cref="CompileTimeCodeAttribute"/>，并执行其转换方法。
        /// </summary>
        internal IEnumerable<string> Transform()
        {
            foreach (var assemblyFile in _assembly.Files)
            {
                var compileType = assemblyFile.Types.FirstOrDefault();
                if
                (
                    compileType != null
                    && compileType.Attributes
                        .Any(x => x.Match<CompileTimeCodeAttribute>())
                )
                {
                    var type = assemblyFile.Compile().First();
                    var transformer = (IPlainCodeTransformer) Activator.CreateInstance(type);
                    var excludedFiles = InvokeCodeTransformer(assemblyFile.FullName, transformer);
                    yield return assemblyFile.FullName;
                    foreach (var excludedFile in excludedFiles)
                    {
                        yield return excludedFile;
                    }
                }
            }
        }
    }
}
