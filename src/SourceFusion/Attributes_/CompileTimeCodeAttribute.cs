using System;
using System.Diagnostics.Contracts;

namespace dotnetCampus.SourceFusion
{
    /// <summary>
    /// 标记此类型仅在编译期间使用。注意：编译完毕后此类型将从目标程序集中消失。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CompileTimeCodeAttribute : Attribute
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
        /// 获取或设置一个值，该值指示在转换代码之后，被转换的文件是否还保留。默认不保留。
        /// </summary>
        public bool KeepSourceFiles { get; set; } = false;

        /// <summary>
        /// 编译时将转换 <paramref name="sourceType"/> 类型。
        /// </summary>
        public CompileTimeCodeAttribute(Type sourceType) => SourceType = sourceType;

        /// <summary>
        /// 编译时将转换 <paramref name="sourceFileNames"/> 文件到新的文件。
        /// </summary>
        public CompileTimeCodeAttribute(params string[] sourceFileNames) => SourceFiles = sourceFileNames;

        [ContractPublicPropertyName(nameof(RepeatCount))]
        private int _repeatCount = 1;
    }
}
