namespace dotnetCampus.SourceFusion.CompileTime
{
    internal interface ICompileAttributeProvider
    {
        ICompileAttribute[] Attributes { get; }
    }
}
