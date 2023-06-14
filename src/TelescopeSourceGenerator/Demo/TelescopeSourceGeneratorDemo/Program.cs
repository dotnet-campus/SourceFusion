namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        var attributedTypesExport = new __AttributedTypesExport__();
        ICompileTimeTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        foreach (var exportedTypeMetadata in exporter.ExportTypes())
        {
            // 输出导出的类型
            Console.WriteLine(exportedTypeMetadata.RealType.FullName);
        }
    }
}

[Foo]
class Foo : Base
{
}

class Base
{
}

class FooAttribute : Attribute
{
}