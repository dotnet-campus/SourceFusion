using System;

namespace dotnetCampus.SourceFusion
{
    /// <summary>
    /// 在编写期提供编译后才能确认的代码的占位符。
    /// </summary>
    public static class Placeholder
    {
        /// <summary>
        /// 返回类型为 <typeparamref name="T"/>[] 的占位符。
        /// </summary>
        /// <typeparam name="T">任意的类型。</typeparam>
        /// <param name="compileTimeCodeGenerator">
        /// 此代码将在编译期执行。
        /// 如果需要调用其它方法，则只允许调用 .NET 基础库中的方法和此类型中标记为 <see cref="CompileTimeMethodAttribute"/> 的方法。
        /// </param>
        /// <returns>
        /// 类型为 <typeparamref name="T"/>[] 的占位符。
        /// 在编译完成后，将替换成参数 <paramref name="compileTimeCodeGenerator"/> 生成的代码。
        /// </returns>
        /// <remarks>
        /// 此方法由编译期间 NuGet 包的 targets 提供实现。
        /// </remarks>
        [CompileTimeMethod]
        public static extern T[] Array<T>(Func<ICompilingContext, string> compileTimeCodeGenerator);

        [CompileTimeMethod]
        public static extern AttributedTypeMetadataCollection<T, TAttribute> AttributedTypes<T, TAttribute>()
            where TAttribute : Attribute;
    }
}
