using System;
using System.Collections.Generic;
using System.Linq;

namespace Cvte.Compiler
{
    public class CompilingException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public CompilingException(IEnumerable<string> errors) : base("编译过程中出现了异常。")
        {
            Errors = errors.ToList();
        }
    }
}
