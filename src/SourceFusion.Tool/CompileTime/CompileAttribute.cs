using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="propertyValues"></param>
        public CompileAttribute(string name, IEnumerable<KeyValuePair<string, string>> propertyValues = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Attribute 不能使用空白字符串创建。", nameof(name));

            if (propertyValues != null)
            {
                foreach (var pair in propertyValues)
                {
                    if (pair.Key == null)
                    {
                        _values.Add(pair.Value);
                    }
                    else
                    {
                        _propertyValues.Add(pair.Key, pair.Value);
                    }
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
        /// <param name="valueIndex">The value index name of the Attribute.</param>
        /// <returns></returns>
        public string this[int valueIndex] => _values.Count > valueIndex ? _values[valueIndex] : "";

        /// <summary>
        /// Gets or sets the properties of the Attribute.
        /// </summary>
        /// <param name="propertyName">The property name of the Attribute.</param>
        /// <returns></returns>
        public string this[string propertyName] =>
            _propertyValues.TryGetValue(
                propertyName ?? throw new ArgumentNullException(nameof(propertyName)), out var value)
                ? value
                : "";

        public IEnumerable<string> GetValues() => _values;

        public IEnumerable<(string property, string value)> GetProperties() => _propertyValues.Select(x=>(x.Key,x.Value));

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
            var left = Name;
            var right = attributeName;

            if (string.Equals(left, right, StringComparison.Ordinal))
            {
                return true;
            }

            var leftIndex = left.LastIndexOf('.');
            var rightIndex = right.LastIndexOf('.');
            left = leftIndex >= 0 ? left.Substring(leftIndex + 1) : left;
            right = rightIndex >= 0 ? right.Substring(rightIndex + 1) : right;

            return left.EndsWith("Attribute", StringComparison.Ordinal)
                ? string.Equals(left, right + "Attribute", StringComparison.Ordinal)
                : string.Equals(left + "Attribute", right, StringComparison.Ordinal);
        }

        private readonly List<string> _values = new List<string>();
        private readonly Dictionary<string, string> _propertyValues = new Dictionary<string, string>();
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
