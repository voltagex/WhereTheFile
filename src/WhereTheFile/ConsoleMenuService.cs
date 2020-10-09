using EFCore.BulkExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WhereTheFile.Database;

namespace WhereTheFile
{
    public class ConsoleMenuService
    {
        private IWTFContext _context;
        private IFileScanner _scanner;

        public ConsoleMenuService(IFileScanner scanner, IWTFContext databaseContext)
        {
            _context = databaseContext;
            _scanner = scanner;
        }
        public void Menu()
        {
            Console.WriteLine("s) Scan all drives");
            Console.WriteLine("ss) Scan specific drive only");
            Console.WriteLine("sp) Scan single path");
            Console.WriteLine("t) Show database statistics");
            Console.WriteLine("f) Find file");
            Console.WriteLine("d) Show duplicates");
            Console.WriteLine("dd) Delete database!");
            Console.WriteLine("b) Backup scan database");
            Console.WriteLine("q) Exit");
            Console.WriteLine("Choice? ");
            var choice = Console.ReadLine().Trim();
            switch (choice)
            {
                case "s":
                    ScanAllDrives();
                    Menu();
                    break;
                case "ss":
                    DriveMenu();
                    Menu();
                    break;
                case "sp":
                    ScanSpecificPath();
                    Menu();
                    break;
                case "b":
                    BackupDatabase();
                    Menu();
                    break;
                case "d":
                    ShowDuplicates();
                    Menu();
                    break;
                case "dd":
                    DeleteDatabase();
                    Menu();
                    break;
                case "f":
                    FindFiles();
                    Menu();
                    break;
                case "t":
                    ShowStatistics();
                    Menu();
                    break;
                case "q":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine();
                    Console.WriteLine("Invalid choice");
                    Menu();
                    break;
            }
        }

        private void DeleteDatabase()
        {
            Console.WriteLine("If you're really sure, type 'yes' and hit Enter: ");
            string confirm = Console.ReadLine().Trim();
            if (confirm.Equals("yes", StringComparison.InvariantCulture))
            {
                File.Delete(Settings.DatabasePath);
                Console.WriteLine("Deleted.");
            }
        }

        private void ScanSpecificPath()
        {
            Console.WriteLine();
            Console.WriteLine("Enter a path to scan, or Enter to get back to the menu: ");

            var scanPath = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(scanPath))
            {
                Menu();
            }

            ScanAndAdd(scanPath);
        }

        private void FindFiles()
        {

            Console.WriteLine();
            Console.WriteLine("Enter all or part of the path, or Enter to get back to the menu: ");

            var search = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(search))
            {
                Menu();
            }


            var results = _context.FilePaths.FindFilesByPath(search);

            foreach (var result in results)
            {
                float megabytes = result.Size / 1024 / 1024;
                Console.WriteLine($"{result.FullPath} ({megabytes} MB):");
            }

            Console.WriteLine();
            FindFiles();
        }

        private void ShowDuplicates(bool orderByNumberOfDuplicates = false)
        {

            var dupes = _context.FilePaths.GetDuplicates(orderByNumberOfDuplicates);

            foreach (var dupe in dupes)
            {
                //TODO: handle filenames much smarter than this
                string filename = dupe.First().FullPath.Split("\\").Last();
                float megabytes = dupe.First().Size / 1024 / 1024;
                Console.WriteLine($"{filename} ({megabytes} MB):");
                foreach (var entry in dupe)
                {
                    Console.WriteLine(entry.FullPath);
                }

                Console.WriteLine();
            }
        }


        private void ShowStatistics()
        {
            Console.WriteLine();
            Console.WriteLine(_context.FilePaths.GenerateStatistics());
            Console.WriteLine();
            Menu();
        }

        private void BackupDatabase()
        {
            Console.WriteLine();

            if (!File.Exists(Settings.DatabasePath))
            {
                Console.WriteLine("Database doesn't exist yet");
                Console.WriteLine();
                Menu();
            }

            File.Copy(Settings.DatabasePath, $"{Settings.DatabasePath}.bak");
            Console.WriteLine($"Backed up to {Settings.DatabasePath}.bak");
            Menu();
        }

        void DriveMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Scan a specific drive, or hit Enter to exit:");
            var drives = System.IO.Directory.GetLogicalDrives();
            foreach (string drive in drives)
            {
                Console.WriteLine(drive);
            }

            Console.WriteLine("Choice? (enter letter only)");
            var choice = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(choice))
            {
                Menu();
            }

            var choiceDrive =
                drives.FirstOrDefault(d => d.StartsWith(choice, StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrEmpty(choiceDrive))
            {
                ScanAndAdd(choiceDrive);
            }

            else
            {
                Console.WriteLine($"Drive {choice} doesn't exist");
                DriveMenu();
            }

        }

        void ScanAllDrives()
        {
            //todo: fix this for Linux - https://github.com/dotnet/runtime/issues/32054
            var drives = System.IO.Directory.GetLogicalDrives();

            foreach (string drive in drives)
            {
                ScanAndAdd(drive);
            }
        }

        //TODO: move this up to another class, this shouldn't be using WTFContext directly like this
        void ScanAndAdd(string path)
        {
            var dbContext = (WTFContext)_context;
            //todo: remove all the console stuff out and pull this out to another class
            Console.WriteLine($"Scanning {path}");
            var watch = Stopwatch.StartNew();
            var files = _scanner.ScanFiles(path);
            watch.Stop();

            Console.WriteLine($"Scanned {files.Count} files in {watch.Elapsed}");

            Console.WriteLine("Adding entries to database");
            watch.Restart();
            dbContext.BulkInsert(files);
            dbContext.SaveChanges();

            watch.Stop();
            Console.WriteLine($"Finished adding entries to database in {watch.Elapsed}");
        }
    }
}
