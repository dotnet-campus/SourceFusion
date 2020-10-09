using System.IO;

using dotnetCampus.Cli;

namespace dotnetCampus.SourceFusion.Cli
{
    internal class Options
    {
        [Value(0, Description = "转换源码的工作路径。")]
        public string WorkingDirectory { get; set; }

        [Option('t', "ToolFolder", Description = "SourceFusion 可以使用的临时文件夹路径。")]
        public string ToolFolder { get; set; }

        [Option('c', "GeneratedCodeFolder", Description = "SourceFusion 生成的新源码文件所在的文件夹。")]
        public string GeneratedCodeFolder { get; set; }

        [Option('p', "ProjectPropertyFile", Description = "一个文件，包含项目的各种所需属性和集合。")]
        public string ProjectPropertyFile { get; set; }

        [Option('s', "PreprocessorSymbols", Description = "预处理符号，也就是条件编译符。")]
        public string PreprocessorSymbols { get; set; }

        [Option("Debug", Description = "如果指定，将在启动编译时进入调试模式。")]
        public bool DebugMode { get; set; }
    }
}
