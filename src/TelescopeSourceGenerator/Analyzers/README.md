# Telescope.SourceGeneratorAnalyzers

使用 SourceGenerator 源代码生成器的 Telescope 版本

可以用来导出指定类型

## 用法

支持多个不同的导出写法

### 分部方法式

这是推荐的方法

在分部类里定义分部方法，分部方法标记 `dotnetCampus.Telescope.TelescopeExportAttribute` 特性，且返回值包括导出条件，如以下写法

```csharp
internal partial class Program
{
    [dotnetCampus.Telescope.TelescopeExportAttribute()]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

以上代码将导出当前项目标记了 `F1Attribute` 且继承 `Base` 的所有类型。经过 Telescope 源代码生成器即可生成大概如下的代码

```csharp
    [global::System.CodeDom.Compiler.GeneratedCode("dotnetCampus.Telescope.SourceGeneratorAnalyzers", "1.0.0")]
    internal partial class Program
    {
        private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable()
        {
            yield return (typeof(F2), new F1Attribute()
            {
                       
            }, () => new F2());
            yield return (typeof(F3), new F1Attribute()
            {
                       
            }, () => new F3());
        }
    }
```

高级用法：

可以在 TelescopeExportAttribute 加上 IncludeReference 属性用来导出所有引用程序集的满足条件的类型，如以下代码

```csharp
internal partial class Program
{
    [dotnetCampus.Telescope.TelescopeExportAttribute(IncludeReference = true)]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

仅推荐在入口程序集加上 `IncludeReference = true` 属性，因为一旦加入此属性，任何引用程序集的变更都可能导致源代码生成器重复执行，降低 VisualStudio 性能

### 程序集标记

这是传统的 Telescope 实现方法，在需要导出类型的项目里标记 `dotnetCampus.Telescope.MarkExportAttribute` 特性，如以下代码

```csharp
[assembly: dotnetCampus.Telescope.MarkExportAttribute(typeof(Base), typeof(FooAttribute))]
```

标记之后将会自动生成 `dotnetCampus.Telescope.__AttributedTypesExport__` 类型，即可在代码里面直接使用，如以下代码

```csharp
        var attributedTypesExport = new __AttributedTypesExport__();
        ICompileTimeAttributedTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        foreach (var exportedTypeMetadata in exporter.ExportAttributeTypes())
        {
            // 输出导出的类型
            Console.WriteLine(exportedTypeMetadata.RealType.FullName);
        }
```

也可以使用 `dotnetCampus.Telescope.AttributedTypes` 辅助类获取所有导出类型