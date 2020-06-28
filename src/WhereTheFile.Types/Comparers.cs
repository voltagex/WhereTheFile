using System;
using System.Collections.Generic;
using System.Text;

namespace WhereTheFile.Types
{
    public class CompareFileNameOnly : IEqualityComparer<ScannedFileInfo>
    {
        public bool Equals(ScannedFileInfo x, ScannedFileInfo y)
        {
            return x.FullPath == y.FullPath;
        }

        public int GetHashCode(ScannedFileInfo obj)
        {
            //todo: is this terrible?

            return obj.FullPath.GetHashCode();
        }

    }
}
