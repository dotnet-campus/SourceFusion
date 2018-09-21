using System;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// 编译时的特性
    /// </summary>
    public class CompileAttribute : ICompileAttribute
    {
        /// <summary>
        /// 创建编译时的特性
        /// </summary>
        /// <param name="name"></param>
        public CompileAttribute(string name)
        {
            Name = name;
        }

        public bool Match<TAttribute>() where TAttribute : Attribute
        {
            var attributeName = typeof(TAttribute).Name;
            if (Name == attributeName)
            {
                return true;
            }

            var index = attributeName.LastIndexOf("Attribute", StringComparison.InvariantCulture);
            if (index >= 0)
            {
                attributeName = attributeName.Substring(0, index);
            }

            return Name == attributeName;
        }

        public string Name { get; }

        /// <inheritdoc />
        public bool Match(string attributeName)
        {
            if (Name == attributeName)
            {
                return true;
            }

            var index = attributeName.LastIndexOf("Attribute", StringComparison.InvariantCulture);
            if (index >= 0)
            {
                attributeName = attributeName.Substring(0, index);
            }

            return Name == attributeName;
        }
    }

    /// <summary>
    /// 为 <see cref="ICompileAttribute"/> 提供扩展。
    /// </summary>
    internal static class CompileAttributeExtensions
    {
        /// <summary>
        /// 判断当前的 <see cref="ICompileAttribute"/> 是 <typeparamref name="TAttribute"/> 指定的特性名称。
        /// 此方法仅支持在实际可执行的代码中使用，如果是目标程序集的编译期，则使用此方法会因为 <typeparamref name="TAttribute"/> 无法找到而编译错误。
        /// </summary>
        /// <typeparam name="TAttribute">强类型的特性。</typeparam>
        /// <param name="attribute"><see cref="ICompileAttribute"/> 的实例。</param>
        /// <returns>
        /// 如果此特性符合 <typeparamref name="TAttribute"/> 名称，则返回 true；否则返回 false。
        /// </returns>
        internal static bool Match<TAttribute>(this ICompileAttribute attribute) where TAttribute : Attribute
        {
            return attribute.Match(typeof(TAttribute).Name);
        }
    }
}