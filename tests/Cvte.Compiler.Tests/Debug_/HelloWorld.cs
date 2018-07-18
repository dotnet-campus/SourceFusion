using System;

namespace Cvte.Compiler.Tests
{
    [CompileTimeTemplate]
    public class HelloWorld
    {
        public void SayHello()
        {
            var outputs = Placeholder.Array<string>(context =>
            {
                return @"""Hello "", ""World"", ""!""";
            });
            foreach (var output in outputs)
            {
                Console.Write(output);
            }
        }
    }
}
