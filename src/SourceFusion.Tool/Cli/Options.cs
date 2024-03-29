﻿using System;
using dotnetCampus.Cli;

namespace dotnetCampus.SourceFusion.Cli
{
    internal class Options
    {
        [Option('w', "working-directory", Description = "转换源码的工作路径。")]
        public string WorkingDirectory { get; set; }

        [Option('t', "tool-folder", Description = "SourceFusion 可以使用的临时文件夹路径。")]
        public string ToolFolder { get; set; }

        [Option('c', "generated-code-folder", Description = "SourceFusion 生成的新源码文件所在的文件夹。")]
        public string GeneratedCodeFolder { get; set; }

        [Option('p', "project-property-file", Description = "一个文件，包含项目的各种所需属性和集合。")]
        public string ProjectPropertyFile { get; set; }
        
        [Option('s', "preprocessor-symbols", Description = "预处理符号，也就是条件编译符。")]
        public string PreprocessorSymbols { get; set; }

        [Option('r', "rebuild", Description = "如果需要重新生成，则指定为 true。")]
        public bool RebuildRequired { get; set; }

        [Option("debug-mode", Description = "如果指定，将在启动编译时进入调试模式。")]
        public bool DebugMode { get; set; }
    }
}