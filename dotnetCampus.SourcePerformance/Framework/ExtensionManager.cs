using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace dotnetCampus.SourcePerformance.Framework
{
    public sealed class ExtensionManager
    {
        public List<string> Extensions { get; } = new List<string>();

        public void LoadExtensions()
        {
            var assembly = typeof(Foo).Assembly;

            foreach (var type in from type in assembly.GetTypes()
                let attribute = type.GetCustomAttribute<InterestingAttribute>()
                where attribute != null
                select type)
            {
                Extensions.Add(type.FullName);
            }
        }
    }
}