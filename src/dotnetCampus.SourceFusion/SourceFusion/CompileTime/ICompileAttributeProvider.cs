namespace dotnetCampus.SourceFusion.CompileTime
{
    public interface ICompileAttributeProvider
    {
        ICompileAttribute[] Attributes { get; }
    }
}
