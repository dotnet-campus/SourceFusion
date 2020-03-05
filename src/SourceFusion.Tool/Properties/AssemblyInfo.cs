﻿using System;
using System.Reflection;

namespace dotnetCampus.SourceFusion.Properties
{
    internal static class AssemblyInfo
    {
        public static string ToolName =
            Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

        public static string ToolVersion =
            Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

        /// <summary>
        /// 如果需要为类加上 <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/>，则使用此字符串。
        /// </summary>
        public static string GeneratedCodeAttribute =
            $@"[System.CodeDom.Compiler.GeneratedCode(""{ToolName}"", ""{ToolVersion}"")]";

        /// <summary>
        /// 获取可以为每一个生成的文件都增加的文件头。
        /// </summary>
        public static string GeneratedCodeComment =
            $@"//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:{Environment.Version.ToString(4)}
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

#define GENERATED_CODE

";
    }
}