using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhereTheFile.Types;

namespace WhereTheFile.Database
{
    public class WTFContext : DbContext
    {

        //public static readonly Microsoft.Extensions.Logging.LoggerFactory _myLoggerFactory =
        //    new LoggerFactory(new[] {new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()},
        //        new LoggerFilterOptions() {MinLevel = LogLevel.Information});


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           //optionsBuilder.UseLoggerFactory(_myLoggerFactory);

            if (!optionsBuilder.IsConfigured)
            {
                Trace.WriteLine("Using default database file");
                optionsBuilder.UseSqlite("Data Source=WTF_EF.db");
            }
        }

 
        public DbSet<ScannedFileInfo> FilePaths { get; set; }
        public DbSet<DriveInfo> Drives { get; set; }

        public DbSet<Duplicates> Duplicates { get; set; }
        public WTFContext()
        {
            base.Database.EnsureCreated();
        }
    }
}