using System;
using System.Collections.Generic;
using System.Text;

namespace WhereTheFile.Types
{
    public class CompareFileNameOnly : IEqualityComparer<ScannedFileInfo>
    {
        public bool Equals(ScannedFileInfo x, ScannedFileInfo y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            //todo: do we want to track whether the scan came from a case-sensitive filesystem?
            return x.FullPath == y.FullPath;
        }

        public int GetHashCode(ScannedFileInfo obj)
        {
            //todo: is this terrible?

            return obj.FullPath.GetHashCode();
        }

    }
}
