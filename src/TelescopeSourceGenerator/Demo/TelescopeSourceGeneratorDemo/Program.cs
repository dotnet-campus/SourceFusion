using dotnetCampus.Telescope.SourceGeneratorAnalyzers.DemoLib1;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo;

internal partial class Program
{
    static void Main(string[] args)
    {
        foreach (var (_, xx, xxx) in ExportFooEnumerable())
        {
        }

        var program = new Program();
        foreach (var (_, xxx) in program.ExportF1Enumerable())
        {
        }

        var attributedTypesExport = new __AttributedTypesExport__();
        ICompileTimeAttributedTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        foreach (var exportedTypeMetadata in exporter.ExportAttributeTypes())
        {
            // 输出导出的类型
            Console.WriteLine(exportedTypeMetadata.RealType.FullName);
        }
    }

    [dotnetCampus.Telescope.TelescopeExportAttribute()]
    private static partial IEnumerable<(Type type, FooAttribute attribute, Func<Base> creator)> ExportFooEnumerable();

    [dotnetCampus.Telescope.TelescopeExportAttribute(IncludeReferences = true)]
    private partial IEnumerable<(Type, Func<DemoLib1.F1> xxx)> ExportF1Enumerable();
}

[F1]
public class CurrentFoo : DemoLib1.F1
{
}

[FooAttribute]
class F1 : Base
{
}

[FooAttribute]
class F2 : Base
{
}

class Base
{
}

class FooAttribute : Attribute
{
 
}

public enum FooEnum
{
    N1,
    N2,
    N3,
}

