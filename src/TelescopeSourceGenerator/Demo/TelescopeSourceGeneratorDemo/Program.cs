using dotnetCampus.Telescope.SourceGeneratorAnalyzers.DemoLib1;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo;

internal partial class Program
{
    static void Main(string[] args)
    {
        foreach (var (_, xx, xxx) in ExportFooEnumerable())
        {
        }

        //var attributedTypesExport = new __AttributedTypesExport__();
        //ICompileTimeAttributedTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        //foreach (var exportedTypeMetadata in exporter.ExportAttributeTypes())
        //{
        //    // 输出导出的类型
        //    Console.WriteLine(exportedTypeMetadata.RealType.FullName);
        //}
    }

    [dotnetCampus.Telescope.TelescopeExportAttribute(IncludeReferences = true)]
    private static partial IEnumerable<(Type, F1Attribute xx, Func<DemoLib1.F1> xxx)> ExportFooEnumerable();

    [dotnetCampus.Telescope.TelescopeExportAttribute(IncludeReferences = true)]
    private partial IEnumerable<(Type, Func<DemoLib1.F1> xxx)> ExportF1Enumerable();
}

[F1]
public class CurrentFoo : DemoLib1.F1
{
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

