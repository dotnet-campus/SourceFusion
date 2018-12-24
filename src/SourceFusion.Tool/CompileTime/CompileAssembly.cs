using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// 编译时找到的程序集
    /// </summary>
    internal class CompileAssembly : ICompileAssembly
    {
        /// <summary>
        /// 创建编译找到程序集
        /// </summary>
        /// <param name="compileFiles"></param>
        /// <param name="preprocessorSymbols"></param>
        public CompileAssembly(IEnumerable<string> compileFiles, string preprocessorSymbols)
        {
            var symbols = preprocessorSymbols.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            _compileFilesLazy = new Lazy<List<CompileFile>>
            (
                () => compileFiles.Select(x => new CompileFile(x, symbols)).ToList(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public IReadOnlyCollection<CompileFile> Files => _compileFilesLazy.Value;

        public ICompileType[] GetTypes()
        {
            return Files.SelectMany(x => x.Types).ToArray();
        }

        private readonly Lazy<List<CompileFile>> _compileFilesLazy;
    }
}