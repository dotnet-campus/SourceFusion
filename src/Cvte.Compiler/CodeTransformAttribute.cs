using System;
using System.Diagnostics.Contracts;

namespace Cvte.Compiler
{
    /// <summary>
    /// 标记此类型将用于编译时进行源码转换。注意：编译完毕后此类型将从目标程序集中消失。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CodeTransformAttribute : Attribute
    {
        /// <summary>
        /// 获取要转换的所有文件。
        /// </summary>
        public string[] SourceFiles { get; }

        /// <summary>
        /// 获取要转换的类型（这是用于辅助寻找 <see cref="SourceFiles"/> 的临时属性）。
        /// </summary>
        internal Type SourceType { get; }

        /// <summary>
        /// 获取或设置转换的目标文件的编译类型。 默认值是自动，即与原类型一致。
        /// </summary>
        public FileType TargetType { get; set; } = FileType.Auto;

        /// <summary>
        /// 获取或设置代码转换的重复次数。指定多次可以让同一份代码进行多次转换，生成多份不同的目标文件。
        /// </summary>
        public int RepeatCount
        {
            get => _repeatCount;
            set => _repeatCount = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value),
                    $"{nameof(RepeatCount)} should be more than zero.");
        }

        /// <summary>
        /// 编译时将转换 <paramref name="sourceType"/> 类型。
        /// </summary>
        public CodeTransformAttribute(Type sourceType) => SourceType = sourceType;

        /// <summary>
        /// 编译时将转换 <paramref name="sourceFileNames"/> 文件到新的文件。
        /// </summary>
        public CodeTransformAttribute(params string[] sourceFileNames) => SourceFiles = sourceFileNames;

        [ContractPublicPropertyName(nameof(RepeatCount))]
        private int _repeatCount = 1;
    }
}
