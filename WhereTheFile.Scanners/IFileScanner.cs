using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhereTheFile.Types;

namespace WhereTheFile
{
    public interface IFileScanner
    {
        public Task ScanFiles(string startPath);
        public string[] GetStartPaths(string topLevelPath);
    }
}
