namespace Cvte.Compiler.Tests
{
    [CompileTimeCode("HelloWorld.cs")]
    public class HelloWorldTransformer : IPlainCodeTransformer
    {
        public string Transform(string originalText, TransformingContext context)
        {
            return originalText.Replace("Hello World", "Hello ");
        }
    }
}
