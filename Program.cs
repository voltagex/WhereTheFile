using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Extensions.Configuration;
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
        private static Settings Settings = new Settings();
        private static WTFContext Context = new WTFContext();
        static void Main(string[] args)
        {
            LoadConfig();
            Menu();
        }

        private static void LoadConfig()
        {
            

        }

        static void Menu()
        {
            Console.WriteLine("i) Generate GUIDs for drives (requires Admin the first time around)");
            Console.WriteLine("s) Scan all drives");
            Console.WriteLine("ss) Scan specific drive only");
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
            
            var dupes = Context.FilePaths.Where(d => d.Size > 1024 * 1024 * 5).AsEnumerable()
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
            
            float totalSize = Context.FilePaths.Sum(f => f.Size);
            var totalFiles = Context.FilePaths.Count();
            var largestFile = Context.FilePaths.OrderByDescending(f=>f.Size).First();

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

            //TODO: this isn't correct, should be looking up the GUID from the file if it exists
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
            WindowsInterop.RtlSetProcessPlaceholderCompatibilityMode(2);
            ScannedDrives = GetOrGenerateDriveGuids();
            
            //TODO: this won't work with scanning a path rather than a drive
            string selectedDriveId = ScannedDrives.Single(d => d.CurrentDriveLetter.Contains(path,StringComparison.InvariantCultureIgnoreCase)).GeneratedGuid; //Can't seem to do case insensitive compare against SQLite itself.
            DriveInfo drive = Context.Drives.Single(d => d.GeneratedGuid == selectedDriveId);

            Console.WriteLine("Adding drives to database if needed");
            Context.Drives.AddRange(ScannedDrives.Where(d => !Context.Drives.Contains(d)));
            int changes = Context.SaveChanges();
            Console.WriteLine($"{changes} changes made");

            FileSystemEnumerable<ScannedFileInfo> fse =
                new FileSystemEnumerable<ScannedFileInfo>(path,
                    (ref FileSystemEntry entry) => new ScannedFileInfo() {FullPath = entry.ToFullPath(), Size = entry.Length, Drive = drive, Attributes = entry.Attributes},
                    new EnumerationOptions() {RecurseSubdirectories = true});

            using (var scanContext = new WTFContext())
            {
                //FileSystemEnumerable seems to return directories, and the file size is set to the total of all files in that directory.
                //Exclude them for now.
                scanContext.Attach(drive);
                scanContext.FilePaths.AddRange(fse.Where(file => !file.Attributes.HasFlag(FileAttributes.Directory)));
                int fileChanges = scanContext.SaveChanges();
                Console.WriteLine($"{fileChanges} files added to the database");
            }

           
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


