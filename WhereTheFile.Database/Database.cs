using System;
using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using WhereTheFile.Types;

namespace WhereTheFile.Database
{
    public class WTFContext : DbContext, IWTFContext
    {
#if !DEBUG
        private readonly string BaseAppDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhereTheFile"); //todo: allow override from appsettings.json
#else
        private readonly string BaseAppDataPath = ".";
#endif        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            var loggerFactory = LoggerFactory.Create(l => l.AddConsole()); //todo: use injected logger?
            if (!optionsBuilder.IsConfigured)
            {
                if (!Directory.Exists(BaseAppDataPath))
                {
                    Directory.CreateDirectory(BaseAppDataPath);
                }
                var databasePath = Path.Join(BaseAppDataPath, "WTF_EF.db");
                
                optionsBuilder.UseSqlite($"Data Source={databasePath}").UseLoggerFactory(loggerFactory);
#if DEBUG
                Console.WriteLine(System.IO.Path.GetFullPath(databasePath)); 
#endif
                
            }
        }

        public DbSet<ScannedFileInfo> FilePaths { get; set; }


        public WTFContext(DbContextOptions<WTFContext> options) : base(options)
        {
            base.Database.EnsureCreated();
            if (!base.Database.CanConnect())
            {
                throw new IOException("Can't connect to database with custom options");
            }
        }

        public WTFContext()
        {
            base.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScannedFileInfo>().Property(p => p.FileCreated)
                .HasConversion(v => v.Ticks, v => new DateTime(v));
        }
    }
}