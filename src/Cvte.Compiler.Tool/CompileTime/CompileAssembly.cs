using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cvte.Compiler.CompileTime
{
    internal class CompileAssembly : ICompileAssembly
    {
        private readonly Lazy<List<CompileFile>> _compileFilesLazy;

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
