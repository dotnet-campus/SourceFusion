using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var resultFile = new FileInfo(Path.Combine(ToolDirectory.FullName, "Result.txt"));
            var lastBuildingTime = resultFile.LastWriteTimeUtc;

            var projectDirectory = ProjectDirectory.FullName;
            var objDirectory = Path.Combine(projectDirectory, "obj");
            var files = CompileFiles;
            var newFiles = files
                .Where(x =>
                    // 是项目中的文件夹
                    x.FullName.StartsWith(projectDirectory, StringComparison.OrdinalIgnoreCase) &&
                    // 不是项目临时文件夹
                    !x.FullName.StartsWith(objDirectory, StringComparison.OrdinalIgnoreCase) &&
                    !x.FullName.StartsWith("obj", StringComparison.OrdinalIgnoreCase)
                    )
                .Where(x => x.LastWriteTimeUtc > lastBuildingTime);

            var shouldRebuild = newFiles.Any();
            return shouldRebuild;
        }
    }
}
