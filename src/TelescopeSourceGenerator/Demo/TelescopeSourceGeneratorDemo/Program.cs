namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo;

internal class Program
{
    static void Main(string[] args)
    {
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