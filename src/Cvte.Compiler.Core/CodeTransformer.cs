using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cvte.Compiler.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Cvte.Compiler
{
    public class CodeTransformer
    {
        private readonly string[] _compilingFiles;

        public CodeTransformer(string workingFolder, IEnumerable<string> compilingFiles)
        {
            workingFolder = Path.GetFullPath(workingFolder);
            _compilingFiles = compilingFiles.Select(x => Path.GetFullPath(Path.Combine(workingFolder, x))).ToArray();
        }

        public async Task TransformAsync()
        {
            foreach (var file in _compilingFiles)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                var declarations = ClassDeclarationVisitor.VisiteSyntaxTree(syntaxTree);
            }
        }
    }
}
