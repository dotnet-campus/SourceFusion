using System;
using System.Collections.Generic;

namespace dotnetCampus.SourceFusion.CompileTime
{
    /// <summary>
    /// The <see cref="Attribute"/> that is in the compile time context.
    /// </summary>
    public class CompileAttribute : ICompileAttribute
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="CompileAttribute"/>.
        /// </summary>
        /// <param name="name">The identifier name of the Attribute.</param>
        public CompileAttribute(string name, IEnumerable<KeyValuePair<string, string>> propertyValues = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (propertyValues != null)
            {
                foreach (var pair in propertyValues)
                {
                    _propertyDictionary.Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Gets the identifier name of the Attribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the properties of the Attribute.
        /// </summary>
        /// <param name="propertyName">The property name of the Attribute.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get => _propertyDictionary.TryGetValue(
                propertyName ?? throw new ArgumentNullException(nameof(propertyName)), out var value)
                ? value
                : "";
            internal set => _propertyDictionary
                [propertyName ?? throw new ArgumentNullException(nameof(propertyName))] = value;
        }

        /// <summary>
        /// Check whether this <see cref="CompileAttribute"/> indicate the runtime version of <typeparamref name="TAttribute"/>.
        /// </summary>
        /// <typeparam name="TAttribute">
        /// The runtime version of <see cref="Attribute"/>.
        /// </typeparam>
        /// <returns>
        /// If the <see cref="CompileAttribute"/> indicate the runtime version of <typeparamref name="TAttribute"/>, returns true.
        /// Else returns false.
        /// </returns>
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

        private readonly Dictionary<string, string> _propertyDictionary = new Dictionary<string, string>();
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