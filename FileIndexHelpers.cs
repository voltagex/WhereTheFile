using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using SQLitePCL;
using WhereTheFile.Database;
using WhereTheFile.Types;
using WhereTheFile.Windows;

public class FileIndexHelpers
{
    private WTFContext _context = null;
    public FileIndexHelpers()
    {
        _context = new WTFContext();
    }

    public int ScanFiles(string path)
    {
        WindowsInterop.RtlSetProcessPlaceholderCompatibilityMode(2);

        FileSystemEnumerable<ScannedFileInfo> fse =
            new FileSystemEnumerable<ScannedFileInfo>(path,
                (ref FileSystemEntry entry) => new ScannedFileInfo() { FullPath = entry.ToFullPath(), Size = entry.Length, Attributes = entry.Attributes, FileCreated = entry.CreationTimeUtc.UtcDateTime },
                new EnumerationOptions() { RecurseSubdirectories = true })
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
            };

        using (var scanContext = new WTFContext())
        {
            //FileSystemEnumerable seems to return directories, and the file size is set to the total of all files in that directory.
            //Exclude them for now.
            scanContext.FilePaths.AddRange(fse.Where(file => !file.Attributes.HasFlag(FileAttributes.Directory)));
            return scanContext.SaveChanges();
        }
    }


    public IEnumerable<IGrouping<long, ScannedFileInfo>> FindFilesByPath(string search)
    {
        return _context.FilePaths.Where(r => r.FullPath.Contains(search)).OrderByDescending(r => r.Size).AsEnumerable().GroupBy(r => r.Size);
    }

    public IEnumerable<IGrouping<long, ScannedFileInfo>> GetDuplicates(bool orderByNumberOfDuplicates)
    {
        var dupes = _context.FilePaths.Where(d => d.Size > 1024 * 1024 * 5).AsEnumerable()
            .OrderByDescending(r => r.Size).GroupBy(r => r.Size)
            .Where(g => g.Count() > 1);


        if (orderByNumberOfDuplicates)
        {
            dupes = dupes.OrderByDescending(g => g.Count());
        }

        return dupes;
    }

}
