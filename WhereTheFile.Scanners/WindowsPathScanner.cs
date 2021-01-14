
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WhereTheFile.Database;
using WhereTheFile.Types;
using WhereTheFile.Windows;
using EFCore.BulkExtensions;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;

namespace WhereTheFile.Scanners
{
    public class WindowsPathScanner : IFileScanner
    {
        private readonly WTFContext _context;
        private readonly ILogger<WindowsPathScanner> _logger;
        public WindowsPathScanner(IWTFContext context, ILogger<WindowsPathScanner> logger)
        {
            //todo: how do I not do this cast? It seems very bad.
            _context = (WTFContext)context;
            _logger = logger;
        }

        public async Task ScanFiles(string path)
        {
            _logger.LogDebug("Scanning {path}", path);
            //todo: don't run this every time?
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsInterop.RtlSetProcessPlaceholderCompatibilityMode(2); //unhides OneDrive symlinks so we don't cause a download
            }

            IEnumerable<ScannedFileInfo> info = new FileSystemEnumerable<ScannedFileInfo>(path,
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
            };
            Console.WriteLine($"outside of lock for {path}");
            lock (this)
            {
                
                Console.WriteLine($"in lock for {path}");
                _context.BulkInsert<ScannedFileInfo>(info.ToList());
                _context.SaveChanges();
                Console.WriteLine($"saved {path}");
                
            }

            _logger.LogDebug("Finishing insert and saving changes");
        }


        //todo: no real way to do this async
        public string[] GetStartPaths(string topLevelPath)
        {
            if (string.IsNullOrEmpty(topLevelPath))
            {
                return System.IO.Directory.GetLogicalDrives();
            }
            // passing in C:\ from Javascript was exploding as it was coming in as "C:\"
            topLevelPath = topLevelPath.Trim('"');

            if (topLevelPath.EndsWith(':'))
            // weird case that passing 'C:' gets you different results to 'C:\',
            {
                topLevelPath += '\\';
            }

            return System.IO.Directory.EnumerateDirectories(topLevelPath).ToArray();

        }

    }
}