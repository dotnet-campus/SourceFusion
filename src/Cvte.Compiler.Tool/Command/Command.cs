using System.Collections.Generic;
using System.Diagnostics;

namespace Cvte.Compiler.Command
{
    [CommandMetadata("compile", Description = "编译")]
    public class Command : CommandTask
    {
        [CommandArgument("-p", Description = "转换源码的工作路径")]
        public string Words { get; set; }

        [CommandArgument("-i|--IntermediateFolder", Description = "中间文件的生成路径")]
        public string IntermediateFolder { get; set; }

        [CommandArgument("-c|--CompilingFiles", Description = "所有参与编译的文件，多个文件使用;分割")]
        public string CompilingFiles { get; set; }

        [CommandArgument("--debug-mode", Description = "测试模式")]
        public bool DebugMode { get; set; }

        /// <inheritdoc />
        public override int Run()
        {
            if (DebugMode)
            {
                Debugger.Launch();
            }


            return base.Run();
        }
    }

    [CommandMetadata("echo", Description = "Output users command at specified format.")]
    public sealed class SampleTask : CommandTask
    {
        private int _repeatCount;

        [CommandArgument("[words]", Description = "The words the user wants to output.")]
        public string Words { get; set; }

        [CommandOption("-r|--repeat-count", Description = "Indicates how many times to output the users words.")]
        public string RepeatCountRaw
        {
            get => _repeatCount.ToString();
            set => _repeatCount = value == null ? 1 : int.Parse(value);
        }

        [CommandOption("--upper", Description = "Indicates that whether all words should be in upper case.")]
        public bool UpperCase { get; set; }

        [CommandOption("-s|--separator", Description = "Specify a string to split each repeat.")]
        public List<string> Separators { get; set; }

        public override int Run()
        {
            // 当用户敲入的命令已准备好，上面的参数准备好，那么这个函数就会在这里执行啦。
            return 0;
        }
    }
}