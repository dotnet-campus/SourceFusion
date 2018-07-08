using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cvte.Compiler.CompileTime
{
    /// <summary>
    /// 编译时找到的程序集
    /// </summary>
    internal class CompileAssembly : ICompileAssembly
    {
        private readonly Lazy<List<CompileFile>> _compileFilesLazy;

        /// <summary>
        /// 创建编译找到程序集
        /// </summary>
        /// <param name="compileFiles"></param>
        public CompileAssembly(IEnumerable<string> compileFiles)
        {
            _compileFilesLazy = new Lazy<List<CompileFile>>(
                () => compileFiles.Select(x => new CompileFile(x)).ToList(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IReadOnlyCollection<CompileFile> Files => _compileFilesLazy.Value;

        public ICompileType[] GetTypes()
        {
            return Files.SelectMany(x => x.Types).ToArray();
        }
    }
}
