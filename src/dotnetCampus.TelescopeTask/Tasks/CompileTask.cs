﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using dotnetCampus.Cli;
using dotnetCampus.Cli.Standard;
using dotnetCampus.MSBuildUtils;
using dotnetCampus.SourceFusion.Cli;
using dotnetCampus.SourceFusion.CompileTime;
using dotnetCampus.SourceFusion.Core;
using dotnetCampus.Telescope;

using Walterlv.IO.PackageManagement;

using static dotnetCampus.TelescopeTask.CodeGenerating.AttributedTypesExportFileGenerator;

namespace dotnetCampus.TelescopeTask.Tasks
{
    internal class CompileTask
    {
        internal void Run(Options options)
        {
            // 准备编译环境。
            var context = new ProjectCompilingContext(options);

            // 清除前一次生成的文件。
            PackageDirectory.Delete(context.GeneratedCodeFolder);

            // 收集并生成类。
            var code = GenerateAttributedTypesExportCode(context.Assembly);
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            // 将生成的类（编译单元）写入到文件中。
            PackageDirectory.Create(context.GeneratedCodeFolder);
            var targetFile = Path.Combine(context.GeneratedCodeFolder, "AttributedTypesExport.g.cs");
            File.WriteAllText(targetFile, code);
        }

        private static string GenerateAttributedTypesExportCode(CompileAssembly assembly)
        {
            // 从 AssemblyInfo.cs 中找出设置的导出类信息。
            var markedExports = assembly.Files
                .SelectMany(x => x.AssemblyAttributes)
                .Where(x => x.Match("MarkExport"))
                .OfType<CompileAttribute>()
                .Select(x => new
                {
                    BaseClassOrInterfaceName = GuessTypeNameByTypeOfSyntax(x[0]),
                    AttributeName = GuessTypeNameByTypeOfSyntax(x[1])
                })
                .ToList();

            // 未标记任何导出，于是不生成类型。
            if (markedExports.Count == 0)
            {
                return "";
            }

            // 寻找并导出所有类，加入到接口列表/方法列表中，并生成类。
            var exportedInterfaces = markedExports.Select(x => $@"ICompileTimeAttributedTypesExporter<{x.BaseClassOrInterfaceName}, {x.AttributeName}>");
            var exportedMethodCodes = markedExports.Select(x => BuildExplicitMethodImplementation(
                assembly.Files.SelectMany(x => x.Types).OfType<CompileType>(),
                x.BaseClassOrInterfaceName,
                x.AttributeName));
            var exportedFileUsings = assembly.Files
                .SelectMany(x => x.Types)
                .Where(x => markedExports.FindIndex(m => x.Attributes.Any(x => x.Match(m.AttributeName))) >= 0)
                .SelectMany(x => x.UsingNamespaceList)
                .Concat(assembly.Files
                    .Where(x => x.AssemblyAttributes.Any(x => x.Match("MarkExport")))
                    .SelectMany(x => x.UsingNamespaceList))
                .Distinct(StringComparer.Ordinal)
                .Where(x => !x.Contains('='))
                .OrderBy(x => x);
            var exportedClass = BuildClassImplementation(exportedInterfaces, exportedMethodCodes, exportedFileUsings);
            return exportedClass.Trim();
        }

        /// <summary>
        /// 从 typeof(Namespace.Type) 字符串中取出 Type 部分。
        /// </summary>
        /// <param name="typeofSyntaxString">typeof 字符串。</param>
        /// <returns>typeof 字符串的 Type 部分。</returns>
        private static string GuessTypeNameByTypeOfSyntax(string typeofSyntaxString)
        {
            var match = Regex.Match(typeofSyntaxString, @"typeof\((?<name>([\w_]+\.)*[\w_]+)\)",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1));
            return match.Success ? match.Groups["name"].Value : typeofSyntaxString;
        }
    }
}
