using System;

namespace dotnetCampus
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class InterestingAttribute : Attribute
    {
    }
}
