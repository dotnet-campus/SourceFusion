using System;

namespace dotnetCampus.SourceFusion
{
    internal class CompileCodeSnippetBuilder
    {
        private ICompilingContext _context;

        public CompileCodeSnippetBuilder(ICompilingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public CompileCodeSnippetBuilder CollectAttributedTypes(string attributeName)
        {
            return this;
        }

        public string ToCodeSnippet()
        {
            return "";
        }

        public override string ToString() => ToCodeSnippet();
    }
}
