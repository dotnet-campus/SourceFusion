using System.Linq;
using Cvte.Compiler.Tests.Fakes.Modules;

namespace Cvte.Compiler.Tests
{
    [CompileTimeTemplate]
    internal static class ModuleCollector
    {
        public static ModuleInfo[] Modules { get; } = Placeholder.Array<ModuleInfo>(context =>
        {
            var moduleTypes = context.Assembly.GetTypes().Where(type => type.Attributes.Any(x => x.Name == "Module"));
            return new CompileCodeSnippet(@"new ModuleInfo<{0}>()),
", moduleTypes.Select(x => x.FullName));
        });
    }
}
