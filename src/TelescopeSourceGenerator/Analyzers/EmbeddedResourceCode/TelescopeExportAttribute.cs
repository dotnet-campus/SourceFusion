// ReSharper disable once CheckNamespace 这个代码将会被编到用户的代码里面，请不要随意改动
namespace dotnetCampus.Telescope
{
    [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class TelescopeExportAttribute : global::System.Attribute
    {
        /// <summary>
        /// 是否包含引用的程序集和 DLL 里面的类型导出。默认只导出当前程序集
        /// </summary>
        public bool IncludeReferences { set; get; }
    }
}