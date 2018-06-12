using System;

namespace Cvte.Compiler.Tests
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class ExportAttribute : Attribute
    {
    }
}
