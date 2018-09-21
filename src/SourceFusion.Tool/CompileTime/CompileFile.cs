using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotnetCampus.SourceFusion.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// 包含 Compile 类型文件的编译期信息。
    /// </summary>
    internal class CompileFile
    {
        /// <summary>
        /// 创建 <see cref="CompileFile"/> 的新实例，通过此实例可以获取文件中的相关类型信息。
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

        /// <summary>
        /// 获取文件的名称（不含路径，包含扩展名）。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取文件的完全限定路径。
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// 获取此文件中包含的所有类型。
        /// </summary>
        public IReadOnlyCollection<ICompileType> Types { get; }

        /// <summary>
        /// 编译指定语法树中的源码，以获取其中定义的类型。
        /// </summary>
        /// <returns>文件中已发现的所有类型。</returns>
        public Type[] Compile()
        {
            var assemblyName = $"{Name}.g";
            return _syntaxTree.Compile(assemblyName);
        }

        private readonly SyntaxTree _syntaxTree;
    }
}