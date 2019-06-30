namespace dotnetCampus.SourceFusion
{
    internal interface IPlainCodeTransformer
    {
        string Transform(string originalText, TransformingContext context);
    }
}
