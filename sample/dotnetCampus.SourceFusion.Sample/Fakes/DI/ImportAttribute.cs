using System;

namespace dotnetCampus.Sample.Fakes.DI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class ImportAttribute : Attribute
    {
    }
}
