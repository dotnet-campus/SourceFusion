using System;

namespace dotnetCampus.Telescope
{
    /// <summary>
    /// 编译时，如果一个类型的基类或实现的接口是 <paramref name="baseClassOrInterfaceType"/> 类型，切标记了 <paramref name="attributeType"/> 特性，则将其导出。
    /// <para>标记导出后，在运行时，可通过 <see cref="AttributedTypes.FromAssembly"/> 将导出的这些类型实例化。</para>
    /// <para>此操作用来替代一部分的反射，用以解决掉应用程序启动时因反射导致的耗时。</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class MarkExportAttribute : Attribute
    {
        /// <summary>
        /// 编译时，如果一个类型的基类或实现的接口是 <paramref name="baseClassOrInterfaceType"/> 类型，切标记了 <paramref name="attributeType"/> 特性，则将其导出。
        /// <para>标记导出后，在运行时，可通过 <see cref="AttributedTypes.FromAssembly"/> 将导出的这些类型实例化。</para>
        /// <para>此操作用来替代一部分的反射，用以解决掉应用程序启动时因反射导致的耗时。</para>
        /// </summary>
        /// <param name="baseClassOrInterfaceType">标记导出此程序集中的此类型的子类（含自身）或者此接口的实现类的类型。如果任意类型均可，请使用 typeof(<see cref="object"/>)。</param>
        /// <param name="attributeType">类型上标记的特性（<see cref="Attribute"/>）类型。</param>
        public MarkExportAttribute(Type baseClassOrInterfaceType, Type attributeType)
        {
            RealType = baseClassOrInterfaceType;
            AttributeType = attributeType;
        }

        public Type RealType { get; }

        public Type AttributeType { get; }
    }
}
