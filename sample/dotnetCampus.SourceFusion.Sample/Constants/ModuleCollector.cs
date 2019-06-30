﻿using System.Linq;
using dotnetCampus.Sample.Fakes.Modules;
using dotnetCampus.SourceFusion;
using dotnetCampus.SourceFusion.Attributes;

namespace dotnetCampus.Sample.Constants
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
