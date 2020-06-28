using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WhereTheFile.Database;
using WhereTheFile.Types;

namespace WhereTheFile.Tests
{
    internal static class TestHelpers
    {
        internal static ScannedFileInfo CreateTestFile(string path, long size)
        {
            return new ScannedFileInfo()
            {
                Attributes = FileAttributes.Normal,
                FileCreated = DateTime.Now,
                FullPath = path,
                Size = size
            };
        }

        internal static WTFContext CreateContextBackedByFile(string databasePath)
        {
            SqliteConnection fileConnection = new SqliteConnection($"Data Source={databasePath}");
            fileConnection.Open();
            DbContextOptionsBuilder<WTFContext> builder = new DbContextOptionsBuilder<WTFContext>();
            builder.UseSqlite(fileConnection);
            return new WTFContext(builder.Options);
        }

        internal static WTFContext CreateMemoryBackedContextFromSQL(string sqlPath)
        {
            var context = CreateMemoryBackedContext();
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = File.ReadAllText(sqlPath);
            command.ExecuteNonQuery();
            
            return context;
        }

        internal static WTFContext CreateMemoryBackedContext()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            DbContextOptionsBuilder<WTFContext> builder = new DbContextOptionsBuilder<WTFContext>();
            builder.UseSqlite(connection);
            return new WTFContext(builder.Options);
        }
    }
}
