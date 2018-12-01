using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dotnetCampus.CompilingServices;

namespace dotnetCampus.SourcePerformance.Framework
{
    public sealed class ExtensionManager
    {
        public ExtensionManager()
        {
            _counter = Services.PerformanceCounter;
            _export = new ExtensionExport();
        }

        private readonly ExtensionExport _export;

        private readonly PerformanceCounter _counter;

        public List<IInteresting> Extensions { get; } = new List<IInteresting>();

        public void LoadExtensions()
        {
            _counter.Framework();
#if !使用SourceFusion

            Extensions.AddRange(_export.Interestings);

#else

            var assembly = typeof(ExtensionExport).Assembly;

            foreach (var type in from type in assembly.GetTypes()
                let attribute = type.GetCustomAttribute<InterestingAttribute>()
                where attribute != null
                select type)
            {
                Extensions.Add((IInteresting) Activator.CreateInstance(type));
            }

#endif
            _counter.Extension();
        }
    }
}