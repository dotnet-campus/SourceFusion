namespace dotnetCampus.SourceFusion.CompileTime
{
    public interface ICompileAttribute
    {
        /// <summary>
        /// 获取此特性在代码中书写的名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 判断当前的 <see cref="ICompileAttribute"/> 是否匹配指定的特性名称。
        /// 由于特性的名称可能包含 Attribute 后缀或命名空间前缀，所以使用此方法判断此特性是否是指定名称的特性会更加安全。
        /// </summary>
        /// <param name="attributeName">
        /// 进行比较的特性名称。
        /// </param>
        /// <returns>
        /// 如果此特性符合 <paramref name="attributeName"/> 名称，则返回 true；否则返回 false。
        /// </returns>
        bool Match(string attributeName);

        /// <summary>
        /// Gets or sets the indexed properties of the Attribute.
        /// </summary>
        /// <param name="valueIndex">The property index of the Attribute.</param>
        /// <returns></returns>
        string this[int valueIndex] { get; }

        /// <summary>
        /// Gets or sets the properties of the Attribute.
        /// </summary>
        /// <param name="propertyName">The property name of the Attribute.</param>
        /// <returns></returns>
        string this[string propertyName] { get; }
    }
}
