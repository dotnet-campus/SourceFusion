using System.Collections.Generic;
using System.Diagnostics;
using CommandLine;
using CommandLine.Text;

namespace Cvte.Compiler.Command
{
    public class Command
    {
        [Option('p', Required = true, HelpText = "转换源码的工作路径")]
        public string WorkFolder { get; set; }

        [Option('i', "IntermediateFolder", HelpText = "中间文件的生成路径")]
        public string IntermediateFolder { get; set; }

        [Option('c', "CompilingFiles", HelpText = "所有参与编译的文件，多个文件使用;分割")]
        public string CompilingFiles { get; set; }

        [Option("debug-mode", HelpText = "测试模式")]
        public bool DebugMode { get; set; }
    }
}