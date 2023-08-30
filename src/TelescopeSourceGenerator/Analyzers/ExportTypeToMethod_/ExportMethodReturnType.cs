namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

/// <summary>
/// 导出方法的返回类型
/// </summary>
public enum ExportMethodReturnType
{
    /// <summary>
    /// 采用 IEnumerable 导出 ValueTuple 包含 Type Attribute 和 Creator 三个参数
    /// </summary>
    // static partial IEnumerable<(Type, FooAttribute xx, Func<Base> xxx)> ExportFooEnumerable()
    EnumerableValueTupleWithTypeAttributeCreator,
}