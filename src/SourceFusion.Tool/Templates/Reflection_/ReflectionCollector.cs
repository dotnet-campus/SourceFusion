using System.Linq;

namespace dotnetCampus.SourceFusion.Templates
{
    public class ReflectionCollector
    {
        public void Run()
        {
            var type = typeof(CompileTimeCodeAttribute);
            var properties = type.GetProperties().Where(x => x.CanWrite);
        }
    }
}
