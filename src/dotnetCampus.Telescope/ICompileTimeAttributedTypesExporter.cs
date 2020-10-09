using System;

namespace dotnetCampus.Telescope
{
    /// <summary>
    /// 此接口的实现由编译器生成，用于以非反射的方式导出程序集中标记了特定特性（<see cref="Attribute"/>）的类型和其元数据集合。
    /// </summary>
    /// <typeparam name="TBaseClassOrInterface">要求此部分类型元数据必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
    /// <typeparam name="TAttribute">标记的特性（<see cref="Attribute"/>）类型。</typeparam>
    public interface ICompileTimeAttributedTypesExporter<TBaseClassOrInterface, TAttribute>
        where TAttribute : Attribute
    {
        /// <summary>
        /// 编译器必须显式实现此接口方法，将此程序集中的类型元数据导出。
        /// </summary>
        /// <returns>此程序集中的类型元数据。</returns>
        AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>[] ExportAttributeTypes();
    }
}
