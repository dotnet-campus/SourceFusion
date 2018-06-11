//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Diagnostics.Contracts;
//using System.Linq;
//using Microsoft.Build.Framework;
//using Microsoft.Build.Utilities;

//namespace Cvte.Compiler
//{
//    public class TransformCode : Task
//    {
//        public string WorkingFolder { get; set; }

//        public string IntermediateFolder { get; set; }

//        public string CompilingFiles { get; set; }

//        [Output]
//        public List<string> ExcludedCompiles { get; set; }

//        public override bool Execute()
//        {
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }

//            var workingFolder = WorkingFolder ?? throw new ArgumentException("", nameof(WorkingFolder));
//            var intermediateFolder = IntermediateFolder ?? throw new ArgumentException("", nameof(IntermediateFolder));
//            var compilingFiles = CompilingFiles?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ??
//                                 throw new ArgumentException("", nameof(CompilingFiles));
//            Contract.EndContractBlock();

//            var transformer = new CodeTransformer(workingFolder, intermediateFolder, compilingFiles);
//            var excludes = transformer.Transform();
//            ExcludedCompiles = excludes.ToList();
//            return true;
//        }
//    }
//}
