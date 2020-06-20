using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhereTheFile.Types;

namespace WhereTheFile.Database
{
    public class WTFContext : DbContext
    { 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            if (!optionsBuilder.IsConfigured)
            {
                var databasePath = Path.Join(Settings.BaseAppDataPath, "WTF_EF.db");
                optionsBuilder.UseSqlite($"Data Source={databasePath}");
            }
        }

        public DbSet<ScannedFileInfo> FilePaths { get; set; }


        public WTFContext(DbContextOptions<WTFContext> options) : base(options)
        {
            var created = base.Database.EnsureCreated();
            if (!base.Database.CanConnect())
            {
                throw new IOException("Can't connect to database with custom options");
            }
        }

        public WTFContext()
        {
            base.Database.EnsureCreated();
        }
    }
}