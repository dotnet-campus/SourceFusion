using System;

namespace Cvte.Compiler
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CodeTransformAttribute : Attribute
    {
        public CodeTransformAttribute(Type sourceType)
        {
        }

        public CodeTransformAttribute(string sourceFileName)
        {
        }

        public CodeTransformAttribute(Type sourceFileName, params string[] sourceFileNames)
        {
        }
    }
}
