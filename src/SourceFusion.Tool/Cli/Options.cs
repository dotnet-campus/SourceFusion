using CommandLine;

namespace dotnetCampus.SourceFusion.Cli
{
    internal class Options
    {
        [Value(0, HelpText = "转换源码的工作路径。")]
        public string WorkingFolder { get; set; }

        [Option('i', "intermediate-folder", Required = true, HelpText = "SourceFusion 可以使用的临时文件夹路径。")]
        public string IntermediateFolder { get; set; }

        [Option('g', "generated-code-folder", Required = true, HelpText = "SourceFusion 生成的新源码文件所在的文件夹。")]
        public string GeneratedCodeFolder { get; set; }

        [Option('c', "compiling-files", Required = true, HelpText = "所有参与编译的文件，多个文件使用分号（;）分割。")]
        public string CompilingFiles { get; set; }

        [Option('f', "filter-files", Required = true, HelpText = "只分析指定（正则表达式）名称的文件。")]
        public string FilterFiles { get; set; }

        [Option('p', "preprocessor-symbols", Required = true, HelpText = "指定条件编译符。")]
        public string PreprocessorSymbols { get; set; }

        [Option('r', "rebuild", Default = true, HelpText = "如果需要重新生成，则指定为 true。")]
        public bool RebuildRequired { get; set; }

        [Option("debug-mode", Default = false, HelpText = "如果指定，将在启动编译时进入调试模式。")]
        public bool DebugMode { get; set; }
    }
}