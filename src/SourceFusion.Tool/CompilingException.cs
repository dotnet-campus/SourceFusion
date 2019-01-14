using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnetCampus.SourceFusion
{
    public class CompilingException : Exception
    {
        /// <summary>
        /// 创建编译时异常
        /// </summary>
        /// <param name="errors"></param>
        public CompilingException(IEnumerable<string> errors) : base("编译过程中出现了异常。")
        {
            Errors = errors.ToList();
        }

        /// <summary>
        /// 创建编译时异常
        /// </summary>
        /// <param name="errors"></param>
        public CompilingException(params string[] errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }
}