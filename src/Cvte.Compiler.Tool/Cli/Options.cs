using CommandLine;

namespace Cvte.Compiler.Cli
{
    internal class Options
    {
        [Option('p', Required = true, HelpText = "转换源码的工作路径。")]
        public string WorkingFolder { get; set; }

        [Option('i', "IntermediateFolder", HelpText = "项目临时生成文件夹的相对或绝对路径。")]
        public string IntermediateFolder { get; set; }

        [Option('c', "CompilingFiles", HelpText = "所有参与编译的文件，多个文件使用分号（;）分割。")]
        public string CompilingFiles { get; set; }

        [Option("debug-mode", HelpText = "如果指定，将在启动编译时进入调试模式。")]
        public bool DebugMode { get; set; }
    }
}