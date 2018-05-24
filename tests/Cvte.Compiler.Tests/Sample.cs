using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cvte.Compiler.Tests
{
    // [CodeTransform("zh-cn.yml", "en-us.yml")]
    [CodeTransform(typeof(LocalSample))]
    public class Sample
    {
        public void TestMethod1(CompilingType type)
        {
        }
    }
}
