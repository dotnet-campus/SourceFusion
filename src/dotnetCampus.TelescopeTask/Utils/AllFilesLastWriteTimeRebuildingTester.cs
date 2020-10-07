using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetCampus.TelescopeTask.Utils
{
    public class AllFilesLastWriteTimeRebuildingTester : RebuildingTester
    {
        protected override bool CheckRebuildCore()
        {
            var files = CompileFiles;
            var lastWriteTime = files.Max(x => x.LastWriteTimeUtc);

            var resultFile = new FileInfo(Path.Combine(ToolDirectory.FullName, "Result.txt"));
            var lastBuildingTime = resultFile.LastWriteTimeUtc;

            return lastWriteTime > lastBuildingTime;
        }
    }
}
