using System;
using System.Collections.Generic;
using System.Text;
using WhereTheFile.Types;

namespace WhereTheFile
{
    public interface IFileScanner
    {
        public List<ScannedFileInfo> ScanFiles(string startPath);
    }
}
