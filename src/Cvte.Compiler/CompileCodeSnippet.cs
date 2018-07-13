using System.Collections.Generic;
using System.Text;

namespace Cvte.Compiler
{
    public class CompileCodeSnippet
    {
        private readonly string _snippet;

        public CompileCodeSnippet(string template, IEnumerable<string> placeholders)
        {
            var builder = new StringBuilder();

            foreach (var placeholder in placeholders)
            {
                builder.Append(string.Format(template, placeholder));
            }

            _snippet = builder.ToString();
        }

        public override string ToString() => _snippet;

        public static implicit operator string(CompileCodeSnippet snippet)
        {
            return snippet.ToString();
        }
    }
}
