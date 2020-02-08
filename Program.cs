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

        private static void ShowDuplicates()
        {
            var context = new WTFContext();

            /*  This was fun.
                The inner query selects COUNT(Size) to get 'unique' filesizes.
                PARTITION BY gives us the corresponding rows back so that I can actually identify duplicates
                https://www.sqlshack.com/sql-partition-by-clause-overview/
                https://www.sqlite.org/windowfunctions.html#the_partition_by_clause

                group_concat probably means I can never move away from SQLite - it allows me to grab the duplicate IDs and put them into a single column,
                separated by commas (sue me, this is easier than working out a one-to-many table for this)

                It's possible the extra WINDOW isn't needed here and you can have two groups without it, but I couldn't get this working.
             */

            context.Duplicates.FromSqlRaw("DELETE FROM Duplicates");

            string couldYouDoThisInLinq = @"
            INSERT INTO Duplicates (FullPath, Size, DriveId, DuplicateCount, DuplicateIds)
            SELECT FullPath, Size, DriveId, DuplicateCount, DuplicateIds FROM 
            (SELECT Id, FullPath, Size, DriveId, Count(Size) 
            OVER (PARTITION BY Size) AS DuplicateCount, 
            group_concat(Id,',') OVER win AS DuplicateIds 
            FROM (SELECT Id, FullPath, Size, DriveId FROM FilePaths Where SIZE >= (1024*1024*5)) 
            WINDOW win AS (PARTITION by Size)) 
            WHERE DuplicateCount > 1
";
           context.Duplicates.FromSqlRaw(couldYouDoThisInLinq);

           var dupes = context.Duplicates.OrderByDescending(d=> d.Size).Take(10);

           foreach (var dupe in dupes)
           {
               Console.WriteLine($"{dupe.Size / 1024 / 1024} megabytes: {dupe.FullPath}");
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


