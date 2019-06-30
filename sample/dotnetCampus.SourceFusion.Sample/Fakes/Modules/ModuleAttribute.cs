using System;

namespace dotnetCampus.SourceFusion.Sample.Fakes.Modules
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute
    {
    }
}
