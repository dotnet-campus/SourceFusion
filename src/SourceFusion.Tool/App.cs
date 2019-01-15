using System.IO;
using System.Linq;
using System.Text;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.SourceFusion.Core;
using dotnetCampus.SourceFusion.Templates;
using dotnetCampus.SourceFusion.Transforming;
using dotnetCampus.SourceFusion.Utils;

namespace dotnetCampus.SourceFusion
{
    internal class App
    {
        private readonly Options _options;
        private readonly ILogger _logger;

        public App(Options options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public int Run()
        {
            try
            {
                RunCore();
            }
            catch (CompilingException ex)
            {
                foreach (var error in ex.Errors)
                {
                    _logger.Error($"{error}");
                }
            }

            return 0;
        }

        private void RunCore()
        {
            var context = new ProjectCompilingContext(_options);
            var rebuildRequired = _options.RebuildRequired;
            var cachedExcludesListFile = Path.Combine(context.ToolsFolder, "Excludes.txt");

            // 如果可以差量编译，那么检测之前已经生成的文件，然后将其直接输出。
            if (!rebuildRequired && File.Exists(cachedExcludesListFile))
            {
                var cachedExcludeLines = File.ReadAllLines(cachedExcludesListFile, Encoding.UTF8);
                foreach (var exclude in cachedExcludeLines)
                {
                    _logger.Message(exclude);
                }

                return;
            }

            // 分析 IPlainCodeTransformer。
            var transformer = new CodeTransformer(context);
            var excludes = transformer.Transform();

            // 分析 CompileTimeTemplate。
            var templateTransformer = new TemplateTransformer(context);
            var templateExcludes = templateTransformer.Transform();

            var toExcludes = excludes.Union(templateExcludes)
                .Select(x => PathEx.MakeRelativePath(context.WorkingFolder, x)).ToArray();

            foreach (var exclude in toExcludes)
            {
                _logger.Message(exclude);
            }

            File.WriteAllLines(cachedExcludesListFile, toExcludes, Encoding.UTF8);
        }
    }
}
