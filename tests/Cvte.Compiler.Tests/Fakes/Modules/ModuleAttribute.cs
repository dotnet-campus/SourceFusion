using System;

namespace Cvte.Compiler.Tests.Fakes.Modules
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute
    {
    }
}
