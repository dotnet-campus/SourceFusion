using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

using static dotnetCampus.Telescope.InternalContracts;

namespace dotnetCampus.Telescope
{
    /// <summary>
    /// 当某程序集通过 dotnetCampus.TelescopeSource 编译后，可通过此类型高性能地查找类型并创建类型实例。
    /// </summary>
    public static class AttributedTypes
    {
        /// <summary>
        /// 从指定的程序集 <paramref name="assemblies"/> 中查找标记了 <typeparamref name="TAttribute"/> 的 <typeparamref name="TBaseClassOrInterface"/> 的子类（含自身）或接口的实现类。
        /// </summary>
        /// <typeparam name="TBaseClassOrInterface">要查找的类型必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
        /// <typeparam name="TAttribute">要查找的类型必须已标记的特性（<see cref="Attribute"/>）类型。</typeparam>
        /// <param name="assemblies">要查找的程序集（或程序集集合）。</param>
        /// <returns>对这些程序集集合中所有已导出的指定类型的元数据的集合。</returns>
        public static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> FromAssembly<TBaseClassOrInterface, TAttribute>(params Assembly[] assemblies)
            where TAttribute : Attribute
            => FromAssembly<TBaseClassOrInterface, TAttribute>((IEnumerable<Assembly>)assemblies);

        /// <summary>
        /// 从指定的程序集 <paramref name="assemblies"/> 中查找标记了 <typeparamref name="TAttribute"/> 的 <typeparamref name="TBaseClassOrInterface"/> 的子类（含自身）或接口的实现类。
        /// </summary>
        /// <typeparam name="TBaseClassOrInterface">要查找的类型必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
        /// <typeparam name="TAttribute">要查找的类型必须已标记的特性（<see cref="Attribute"/>）类型。</typeparam>
        /// <param name="assemblies">要查找的程序集（或程序集集合）。</param>
        /// <returns>对这些程序集集合中所有已导出的指定类型的元数据的集合。</returns>
        public static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> FromAssembly<TBaseClassOrInterface, TAttribute>(IEnumerable<Assembly> assemblies)
            where TAttribute : Attribute
        {
            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            List<Exception>? exceptions = null;
            foreach (var assembly in assemblies)
            {
                IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>>? metadataList = null;
                try
                {
                    metadataList = LoadFromAssembly<TBaseClassOrInterface, TAttribute>(assembly);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
                if (metadataList is { })
                {
                    foreach (var metadata in metadataList)
                    {
                        yield return metadata;
                    }
                }
            }
            if (exceptions is { } && exceptions.Count > 0)
            {
                ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
            }
        }

        /// <summary>
        /// 从指定的程序集 <paramref name="assemblies"/> 中查找标记了 <typeparamref name="TAttribute"/> 的 <typeparamref name="TBaseClassOrInterface"/> 的子类（含自身）或接口的实现类。
        /// </summary>
        /// <typeparam name="TBaseClassOrInterface">要查找的类型必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
        /// <typeparam name="TAttribute">要查找的类型必须已标记的特性（<see cref="Attribute"/>）类型。</typeparam>
        /// <param name="assembly">要查找的程序集。</param>
        /// <returns>对这些程序集集合中所有已导出的指定类型的元数据的集合。</returns>
        private static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> LoadFromAssembly<TBaseClassOrInterface, TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var exportType = assembly.GetType(AttributedTypesExportFullClassName);
            return exportType is { } && Activator.CreateInstance(exportType) is ICompileTimeAttributedTypesExporter<TBaseClassOrInterface, TAttribute> exporter
                ? LoadFromExporter(exporter)
                : Enumerable.Empty<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>>();
        }

        /// <summary>
        /// 从指定的程序集已导出元数据中查找标记了 <typeparamref name="TAttribute"/> 的 <typeparamref name="TBaseClassOrInterface"/> 的子类（含自身）或接口的实现类。
        /// </summary>
        /// <typeparam name="TBaseClassOrInterface">要查找的类型必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
        /// <typeparam name="TAttribute">要查找的类型必须已标记的特性（<see cref="Attribute"/>）类型。</typeparam>
        /// <param name="exporter">已导出的元数据。</param>
        /// <returns>对这些程序集集合中所有已导出的指定类型的元数据的集合。</returns>
        private static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> LoadFromExporter<TBaseClassOrInterface, TAttribute>(
            ICompileTimeAttributedTypesExporter<TBaseClassOrInterface, TAttribute> exporter)
            where TAttribute : Attribute
            => exporter.ExportAttributeTypes();
    }
}
