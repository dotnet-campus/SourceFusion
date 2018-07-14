using System;
using Cvte.Compiler.Syntax;

namespace Cvte.Compiler.Templates
{
    internal static class PlaceholderExtensions
    {
        internal static Func<ICompilingContext, string> Compile(this PlaceholderVisitor.PlaceholderInfo placeholderInfo)
        {
            return context => "";
        }
    }
}
