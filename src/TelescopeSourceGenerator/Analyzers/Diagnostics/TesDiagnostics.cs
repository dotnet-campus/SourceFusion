using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using dotnetCampus.Telescope.SourceGeneratorAnalyzers.Properties;
using static dotnetCampus.Telescope.SourceGeneratorAnalyzers.Properties.Resources;

using static Microsoft.CodeAnalysis.WellKnownDiagnosticTags;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Diagnostics;

static class TesDiagnostics
{
    // ReSharper disable InconsistentNaming
    public static DiagnosticDescriptor Tes000_UnknownError => new DiagnosticDescriptor
    (
        nameof(Tes000),
        Localize(nameof(Tes000)),
        Localize(nameof(Tes000_Message)),
        Categories.Compiler,
        DiagnosticSeverity.Error,
        true,
        customTags: new[] { AnalyzerException, NotConfigurable }
    );

    public static DiagnosticDescriptor Tes001_MethodReturnTypeError => new DiagnosticDescriptor
    (
        nameof(Tes001),
        Localize(nameof(Tes001)),
        Localize(nameof(Tes001_Message)),
        Categories.Compiler,
        DiagnosticSeverity.Error,
        true,
        customTags: new[] { AnalyzerException, NotConfigurable }
    );


    private static class Categories
    {
        /// <summary>
        /// 可能产生 bug，则报告此诊断。
        /// </summary>
        public const string AvoidBugs = "dotnetCampus.AvoidBugs";

        /// <summary>
        /// 为了提供代码生成能力，则报告此诊断。
        /// </summary>
        public const string CodeFixOnly = "dotnetCampus.CodeFixOnly";

        /// <summary>
        /// 因编译要求而必须满足的条件没有满足，则报告此诊断。
        /// </summary>
        public const string Compiler = "dotnetCampus.Compiler";

        /// <summary>
        /// 因 Telescope 库内的机制限制，必须满足此要求 Telescope 才可正常工作，则报告此诊断。
        /// </summary>
        public const string Mechanism = "dotnetCampus.Mechanism";

        /// <summary>
        /// 为了代码可读性，使之更易于理解、方便调试，则报告此诊断。
        /// </summary>
        public const string Readable = "dotnetCampus.Readable";

        /// <summary>
        /// 能写得出来正常编译，但会引发运行时异常，则报告此诊断。
        /// </summary>
        public const string RuntimeException = "dotnetCampus.RuntimeException";

        /// <summary>
        /// 编写了无法生效的代码，则报告此诊断。
        /// </summary>
        public const string Useless = "dotnetCampus.Useless";
    }

    public static LocalizableString Localize(string key) => new LocalizableResourceString(key, dotnetCampus.Telescope.SourceGeneratorAnalyzers.Properties.Resources.ResourceManager, typeof(Resources));
}
