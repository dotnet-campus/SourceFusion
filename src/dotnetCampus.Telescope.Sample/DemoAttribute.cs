using System;

namespace dotnetCampus.Telescope.Sample
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DemoAttribute : Attribute
    {
        public DemoAttribute()
        {
        }

        public DemoAttribute(string value)
        {
            Value = value;
        }

        public string? Value { get; }
    }
}
