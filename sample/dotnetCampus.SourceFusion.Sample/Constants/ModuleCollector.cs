using System.Linq;
using dotnetCampus.SourceFusion.Attributes;
using dotnetCampus.SourceFusion.Sample.Fakes.Modules;

namespace dotnetCampus.SourceFusion.Sample.Constants
{
    [CompileTimeTemplate]
    internal static class ModuleCollector
    {
        public static ModuleInfo[] Modules { get; } = Placeholder.Array<ModuleInfo>(context =>
        {
            var moduleTypes = context.Assembly.GetTypes().Where(type => type.Attributes.Any(x => x.Match("Module")));
            return new CompileCodeSnippet(@"new ModuleInfo<{0}>(),
", moduleTypes.Select(x => x.FullName));
        });
    }
}
