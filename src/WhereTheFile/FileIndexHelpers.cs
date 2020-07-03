using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Runtime.InteropServices;
using WhereTheFile.Types;
using WhereTheFile.Windows;

namespace WhereTheFile
{
    //todo: where should ScanFiles actually live?
    public static class FileIndexHelpers
    {
        public static List<ScannedFileInfo> ScanFiles(string path)
        {

            //todo: don't run this every time?
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsInterop.RtlSetProcessPlaceholderCompatibilityMode(2); //unhides OneDrive symlinks so we don't cause a download
            }

            List<ScannedFileInfo> list;

            list = new FileSystemEnumerable<ScannedFileInfo>(path,
                (ref FileSystemEntry entry) => new ScannedFileInfo()
                {
                    FullPath = entry.ToFullPath(),
                    Size = entry.Length,
                    Attributes = entry.Attributes,
                    FileCreated = entry.CreationTimeUtc.UtcDateTime
                },
                new EnumerationOptions() { RecurseSubdirectories = true })
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
            }.ToList();

            return list;
        }



    }
}