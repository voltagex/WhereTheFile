using System;
using System.IO;
using System.Linq;
using WhereTheFile.Database;

namespace WhereTheFile
{
    class Program
    {
        private static string[] drives;
        private static Settings Settings = new Settings();
        private static FileIndexHelpers helpers;
        static void Main(string[] args)
        {

            Menu();
        }

        static void Menu()
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
                case "a":
                    Test();
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


        private static void Test()
        {
            TextWriter original = Console.Out;
            TextWriter writer = new StreamWriter(new FileStream("C:\\temp\\dupes.txt",FileMode.OpenOrCreate));
            Console.SetOut(writer);
            ShowDuplicates(true);
            Console.SetOut(original);
        }

        private static void DeleteDatabase()
        {
            Console.WriteLine("If you're really sure, type 'yes' and hit Enter: ");
            string confirm = Console.ReadLine().Trim();
            if (confirm.Equals("yes", StringComparison.InvariantCulture))
            {
                File.Delete(Settings.DatabasePath);
                Console.WriteLine("Deleted.");
            }


        }

        private static void ScanSpecificPath() 
        { 
        Console.WriteLine();
            Console.WriteLine("Enter a path to scan, or Enter to get back to the menu: ");

            var scanPath = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(scanPath))
            {
                Menu();
            }


            var count = helpers.ScanFiles(scanPath);
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

            var results = helpers.FindFilesByPath(search);

            foreach (var result in results)
            {
                float megabytes = result.Size / 1024 / 1024;
                Console.WriteLine($"{result.FullPath} ({megabytes} MB):");
            }
            Console.WriteLine();
            FindFiles();
        }



        private static void ShowDuplicates(bool orderByNumberOfDuplicates = false)
        {
            var dupes = helpers.GetDuplicates(orderByNumberOfDuplicates);

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
            helpers = new FileIndexHelpers(context);

            Console.WriteLine();
            Console.WriteLine(helpers.GenerateStatistics());
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

            var choiceDrive =
                drives.FirstOrDefault(d => d.StartsWith(choice, StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrEmpty(choiceDrive))
            {
                helpers.ScanFiles(choiceDrive);
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

            foreach (string drive in drives)
            {
                Console.WriteLine($"Scanning {drive}");
                helpers.ScanFiles(drive);
            }
        }




    }
}


