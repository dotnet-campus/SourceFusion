using CommandLine;

namespace dotnetCampus.SourceFusion.Cli
{
    internal class Options
    {
        [Value(0, HelpText = "转换源码的工作路径。")]
        public string WorkingFolder { get; set; }

        [Option('t', "tool-folder", Required = true, HelpText = "SourceFusion 可以使用的临时文件夹路径。")]
        public string ToolFolder { get; set; }

        [Option('c', "generated-code-folder", Required = true, HelpText = "SourceFusion 生成的新源码文件所在的文件夹。")]
        public string GeneratedCodeFolder { get; set; }

        [Option('p', "project-property-file", Required = true, HelpText = "一个文件，包含项目的各种所需属性和集合。")]
        public string ProjectPropertyFile { get; set; }
        
        [Option('s', "preprocessor-symbols", Required = true, HelpText = "预处理符号，也就是条件编译符。")]
        public string PreprocessorSymbols { get; set; }

        [Option('r', "rebuild", Default = true, HelpText = "如果需要重新生成，则指定为 true。")]
        public bool RebuildRequired { get; set; }

        [Option("debug-mode", Default = false, HelpText = "如果指定，将在启动编译时进入调试模式。")]
        public bool DebugMode { get; set; }
    }
}