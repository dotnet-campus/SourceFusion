namespace dotnetCampus.SourceFusion
{
    public interface IPlainCodeTransformer
    {
        string Transform(string originalText, TransformingContext context);
    }
}
