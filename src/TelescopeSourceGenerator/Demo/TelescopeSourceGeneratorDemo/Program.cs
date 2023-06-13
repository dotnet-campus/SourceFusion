namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        var attributedTypesExport = new __AttributedTypesExport__();
        ICompileTimeTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        foreach (var exportedTypeMetadata in exporter.ExportTypes())
        {
            
        }
        Console.WriteLine("Hello, World!");
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