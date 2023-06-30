namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        var attributedTypesExport = new __AttributedTypesExport__();
        ICompileTimeAttributedTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        foreach (var exportedTypeMetadata in exporter.ExportAttributeTypes())
        {
            // 输出导出的类型
            Console.WriteLine(exportedTypeMetadata.RealType.FullName);
        }
    }
}

[Foo(0, FooEnum.N1, typeof(Foo), null)]
abstract class F1 : Base
{
}

[Foo(1ul, FooEnum.N2, typeof(Base), null, Number2 = 2L, Type2 = typeof(Foo), FooEnum2 = FooEnum.N1, Type3 = null)]
class Foo : Base
{
}

class Base
{
}

class FooAttribute : Attribute
{
    public FooAttribute(ulong number1, FooEnum fooEnum, Type? type1, Type? type3)
    {
        Number1 = number1;
        FooEnum1 = fooEnum;
        Type1 = type1;
    }

    public ulong Number1 { get; set; }
    public long Number2 { get; set; }

    public FooEnum FooEnum1 { get; set; }
    public FooEnum FooEnum2 { get; set; }
    public FooEnum FooEnum3 { get; set; }

    public Type? Type1 { get; set; }
    public Type? Type2 { get; set; }
    public Type? Type3 { get; set; }
}

public enum FooEnum
{
    N1,
    N2,
    N3,
}

