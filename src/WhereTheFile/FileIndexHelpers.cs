using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using SQLitePCL;
using WhereTheFile.Database;
using WhereTheFile.Types;
using WhereTheFile.Windows;
using System.Runtime.InteropServices;
using System.Text;

namespace WhereTheFile
{
    public class FileIndexHelpers
    {
        private WTFContext _context = null;
        public FileIndexHelpers(WTFContext context = null)
        {
            _context = context ?? new WTFContext();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsInterop.RtlSetProcessPlaceholderCompatibilityMode(2); //unhides OneDrive symlinks so we don't cause a download
            }
        }

        public int ScanFiles(string path)
        {
            //todo: Should this return a List<ScannedFileInfo> instead and move the DB handling out?
            FileSystemEnumerable<ScannedFileInfo> fse =
                new FileSystemEnumerable<ScannedFileInfo>(path,
                    (ref FileSystemEntry entry) => new ScannedFileInfo() { FullPath = entry.ToFullPath(), Size = entry.Length, Attributes = entry.Attributes, FileCreated = entry.CreationTimeUtc.UtcDateTime },
                    new EnumerationOptions() { RecurseSubdirectories = true })
                {
                    ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
                };

            //FileSystemEnumerable seems to return directories, and the file size is set to the total of all files in that directory.
            //Exclude them for now.
            _context.FilePaths.AddRange(fse.Where(file => !file.Attributes.HasFlag(FileAttributes.Directory)));
            return _context.SaveChanges();
        }

        public IEnumerable<ScannedFileInfo> FindFilesByPath(string search)
        {
            return _context.FilePaths.Where(r => r.FullPath.Contains(search)).OrderByDescending(r => r.Size);
        }

        public IEnumerable<IGrouping<long, ScannedFileInfo>> GetDuplicates(bool orderByNumberOfDuplicates, int excludeLessThanMegabytes = 5)
        {
            var dupes = _context.FilePaths.Where(d => d.Size > 1024 * 1024 * excludeLessThanMegabytes).AsEnumerable()
                .OrderByDescending(r => r.Size).GroupBy(r => r.Size)
                .Where(g => g.Count() > 1);


            if (orderByNumberOfDuplicates)
            {
                dupes = dupes.OrderByDescending(g => g.Count());
            }

            return dupes;
        }

        public string GenerateStatistics()
        {
            float totalSize = _context.FilePaths.Sum(f => f.Size);
            var totalFiles = _context.FilePaths.Count();
            var largestFile = _context.FilePaths.OrderByDescending(f => f.Size).FirstOrDefault();
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Statistics:");
            builder.AppendLine($"{totalFiles} files indexed with a combined size of {(totalSize / 1024 / 1024)} megabytes ({totalSize / 1024 / 1024 / 1024} gigabytes or {(totalSize / 1024 / 1024 / 1024 / 1024)} terabytes)");
            if (totalFiles > 0) //don't crash on an empty database
            {
                builder.AppendLine(
                    $"The largest file indexed is {largestFile.FullPath} at {(largestFile.Size / 1024 / 1024)} megabytes");
            }

            return builder.ToString();
        }
    }
}