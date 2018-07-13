using System;
using System.Collections.Generic;
using System.Linq;
using Cvte.Compiler.CompileTime;

namespace Cvte.Compiler
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

        internal TemplateTransformer(string workingFolder, string intermediateFolder, CompileAssembly assembly)
        {
            _workingFolder = workingFolder;
            _intermediateFolder = intermediateFolder;
            _assembly = assembly;
        }

        public IEnumerable<string> Transform()
        {
            yield break;
            foreach (var assemblyFile in _assembly.Files)
            {
                var compileType = assemblyFile.Types.FirstOrDefault();
                if (compileType != null)
                {
                    if (compileType.Attributes.Any(x => x.Match<CompileTimeTemplateAttribute>()))
                    {
                    }
                }
            }
        }
    }
}
