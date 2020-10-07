using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetCampus.TelescopeTask.Utils
{
    public abstract class RebuildingTester
    {
        public DirectoryInfo ProjectDirectory { get; private set; }

        public DirectoryInfo ToolDirectory { get; private set; }

        public IReadOnlyList<FileInfo> CompileFiles { get; private set; }

        public bool CheckRebuild(string projectDirectory, string toolsFolder, IEnumerable<string> compileFiles)
        {
            ProjectDirectory = new DirectoryInfo(projectDirectory);
            ToolDirectory = new DirectoryInfo(Path.Combine(projectDirectory, toolsFolder));
            CompileFiles = compileFiles.Select(x => new FileInfo(x)).ToList();
            return CheckRebuildCore();
        }

        protected abstract bool CheckRebuildCore();
    }
}
