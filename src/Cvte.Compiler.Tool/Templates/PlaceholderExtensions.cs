using System;
using Cvte.Compiler.Syntax;

namespace Cvte.Compiler.Templates
{
    internal static class PlaceholderExtensions
    {
        internal static Func<ICompilingContext, string> Compile(this PlaceholderVisitor.PlaceholderInfo placeholderInfo)
        {
            return context => @"new ModuleInfo<Cvte.Compiler.Tests.Fakes.Modules.FooModule>(),
new ModuleInfo<Cvte.Compiler.Tests.Fakes.Modules.BarModule>(),
";
        }

        internal static string Wrap(this PlaceholderVisitor.PlaceholderInfo placeholderInfo, string text)
        {
            switch (placeholderInfo.MethodName)
            {
                case nameof(Placeholder.Array):
                    return $@"
{{
{text}}}";
                default:
                    throw new MissingMethodException($"{nameof(Placeholder)} 中不包含名为 {placeholderInfo.MethodName} 的方法。");
            }
        }
    }
}
