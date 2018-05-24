namespace Cvte.Compiler
{
    public interface IPlainCodeTransformer
    {
        string Transform(string originalText, TransformingContext context);
    }
}
