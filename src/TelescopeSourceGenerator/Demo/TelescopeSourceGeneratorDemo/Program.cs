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

[Foo(1ul, FooEnum.N2, Number2 = 2L, Type = typeof(Foo))]
class Foo : Base
{
}

class Base
{
}

class FooAttribute : Attribute
{
    public FooAttribute(ulong number1, FooEnum fooEnum)
    {
        Number1 = number1;
        FooEnum = fooEnum;
    }

    public ulong Number1 { get; set; }
    public long Number2 { get; set; }

    public FooEnum FooEnum { get; set; }

    public Type? Type { get; set; }
}

public enum FooEnum
{
    N1,
    N2,
    N3,
}

