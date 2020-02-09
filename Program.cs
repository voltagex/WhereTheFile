using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection.Metadata;
using WhereTheFile.Database;
using WhereTheFile.Types;
using WhereTheFile.Windows;
using DriveInfo = WhereTheFile.Types.DriveInfo;

namespace WhereTheFile
{
    class Program
    {
        private static string[] drives;
        private static List<DriveInfo> ScannedDrives;
        static void Main(string[] args)
        { 
            Menu();
        }

        static void Menu()
        {
            Console.WriteLine("i) Generate GUIDs for drives (requires Admin the first time around)");
            Console.WriteLine("s) Scan all drives");
            Console.WriteLine("ss) Scan specific drive only");
            Console.WriteLine("t) Show database statistics");
            Console.WriteLine("f) Find file");
            Console.WriteLine("d) Show duplicates");
            Console.WriteLine("b) Backup scan database");
            Console.WriteLine("q) Exit");
            Console.WriteLine("Choice? ");
            var choice = Console.ReadLine().Trim();
            switch (choice)
            {
                case "i":
                    GetOrGenerateDriveGuids();
                    Menu();
                    break;
                case "s":
                    ScanAllDrives();
                    Menu();
                    break;
                case "ss":
                    DriveMenu();
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

        private static void FindFiles()
        {
            var context = new WTFContext();

            Console.WriteLine();
            Console.WriteLine("Enter all or part of the path, or Enter to get back to the menu: ");

            var search = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(search))
            {
                Menu();
            }

            var results = context.FilePaths.Where(r => r.FullPath.Contains(search)).OrderByDescending(r => r.Size).AsEnumerable().GroupBy(r => r.Size);

            foreach (var result in results)
            {
                string filename = result.First().FullPath.Split("\\").Last();
                float megabytes = result.First().Size / 1024 / 1024;
                Console.WriteLine($"{filename} ({megabytes} MB):");
                foreach (var file in result)
                {
                    Console.WriteLine($"\t{file.FullPath}");
                }
            }
            Console.WriteLine();
            FindFiles();
        }

        private static void ShowDuplicates(bool orderByNumberOfDuplicates = false)
        {
            var context = new WTFContext();
            var dupes = context.FilePaths.Where(d => d.Size > 1024 * 1024 * 5).AsEnumerable()
                .OrderByDescending(r => r.Size).GroupBy(r => r.Size)
                .Where(g => g.Count() > 1);


            if (orderByNumberOfDuplicates)
            {
                dupes = dupes.OrderByDescending(g => g.Count());
            }
            
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
        
        private static void ShowStatistics()
        {
            var context = new WTFContext();
            
            float totalSize = context.FilePaths.Sum(f => f.Size);
            var totalFiles = context.FilePaths.Count();
            var largestFile = context.FilePaths.OrderByDescending(f=>f.Size).First();

            Console.WriteLine();
            Console.WriteLine("Statistics:");
            Console.WriteLine($"{totalFiles} files indexed with a combined size of {(totalSize / 1024 / 1024)} megabytes ({totalSize / 1024 / 1024 / 1024} or {(totalSize / 1024 / 1024 / 1024 / 1024)} terabytes)");
            Console.WriteLine($"The largest file indexed is {largestFile.FullPath} at {(largestFile.Size / 1024 / 1024)} megabytes");
            Console.WriteLine();
            Menu();
        }

        private static void BackupDatabase()
        {
            Console.WriteLine();
            string databaseFile = "WTF_EF.db";
            if (!File.Exists(databaseFile))
            {
                Console.WriteLine("Database doesn't exist yet");
                Console.WriteLine();
                Menu();
            }

            File.Copy(databaseFile, $"{databaseFile}.bak");
            string fullPath = Path.GetFullPath($"{databaseFile}.bak");
            Console.WriteLine($"Backed up to {fullPath}");
            Menu();
        }

        static void DriveMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Scan a specific drive, or hit Enter to exit:");
            drives = System.IO.Directory.GetLogicalDrives();
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
                ScanFiles(choiceDrive);
            }

            else
            {
                Console.WriteLine($"Drive {choice} doesn't exist");
                DriveMenu();
            }

        }

        static void ScanAllDrives()
        {
            drives = System.IO.Directory.GetLogicalDrives();
            ScannedDrives = GetOrGenerateDriveGuids();

            foreach (string drive in drives)
            {
                Console.WriteLine($"Scanning {drive}");
                ScanFiles(drive);
            }
        }

        static void ScanFiles(string path)
        {
            DriveInfo drive = ScannedDrives.First(d => path.StartsWith(d.CurrentDriveLetter));
            WindowsInterop.RtlSetProcessPlaceholderCompatibilityMode(2);

            FileSystemEnumerable<ScannedFileInfo> fse =
                new FileSystemEnumerable<ScannedFileInfo>(path,
                    (ref FileSystemEntry entry) => new ScannedFileInfo() {FullPath = entry.ToFullPath(), Size = entry.Length, Drive = drive},
                    new EnumerationOptions() {RecurseSubdirectories = true});

            var context = new WTFContext();
            context.FilePaths.AddRange(fse);
            context.SaveChanges();
        }

        static List<DriveInfo> GetOrGenerateDriveGuids()
        {
            //This is far easier than trying to get the drive serial number, but I guess I'll have to do that eventually
            List<DriveInfo> scannedDrives = new List<DriveInfo>();
            foreach (string drive in drives)
            {
                string guid = String.Empty;
                string path = Path.Join(drive, ".wtf");
                if (File.Exists(path))
                {
                    guid = File.ReadAllText(path);
                }

                else
                {
                    guid = Guid.NewGuid().ToString();
                    File.WriteAllText(path,guid);
                }
                scannedDrives.Add(new DriveInfo() { CurrentDriveLetter = drive, GeneratedGuid = guid, HasBeenScanned = false});
            }

            return scannedDrives;
        }
    }
}


