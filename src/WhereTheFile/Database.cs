using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhereTheFile.Types;

namespace WhereTheFile.Database
{
    public class WTFContext : DbContext
    { 
        //public static readonly Microsoft.Extensions.Logging.LoggerFactory _myLoggerFactory =
        //    new LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() },
        //        new LoggerFilterOptions() { MinLevel = LogLevel.Information });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLoggerFactory(_myLoggerFactory);

            if (!optionsBuilder.IsConfigured)
            {

                var databasePath = Path.Join(Settings.BaseAppDataPath, "WTF_EF.db");
                optionsBuilder.UseSqlite($"Data Source={databasePath}");
            }
        }

        public DbSet<ScannedFileInfo> FilePaths { get; set; }


        public WTFContext()
        {
            base.Database.EnsureCreated();
        }
    }
}