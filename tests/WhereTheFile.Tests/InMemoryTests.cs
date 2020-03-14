using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WhereTheFile.Database;
using WhereTheFile.Types;

namespace WhereTheFile.Tests
{
    public class Tests
    {
        private const int Megabyte = 1024 * 1024;


        private WTFContext context = null;
        private ScannedFileInfo CreateTestFile(string path, long size)
        {
            return new ScannedFileInfo()
            {
                Attributes = FileAttributes.Normal,
                FileCreated = DateTime.Now,
                FullPath = path,
                Size = size
            };
        }
        [SetUp]
        public void Setup()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            DbContextOptionsBuilder<WTFContext> builder = new DbContextOptionsBuilder<WTFContext>();
            builder.UseSqlite(connection);
            context = new WTFContext(builder.Options);

            List<ScannedFileInfo>  testFiles = new List<ScannedFileInfo>()
            {
                CreateTestFile(@"T:\folder1\a", 4*Megabyte),
                CreateTestFile(@"T:\folder1\b", 2*Megabyte),
                CreateTestFile(@"T:\folder2\c", 3*Megabyte),
                CreateTestFile(@"T:\folder2\d", 2*Megabyte),
            };

            context.FilePaths.AddRange(testFiles);
            context.SaveChanges();
        }

        [Test]
        public void TestDuplicatesFound()
        {
            var dupes = new FileIndexHelpers(context).GetDuplicates(false,0);
            var files = dupes.SelectMany(d => d).ToList();
            var fileNames = files.Select(f => f.FullPath);

            Assert.True(fileNames.Contains(@"T:\folder1\b"));
            Assert.True(fileNames.Contains(@"T:\folder2\d"));
        }


    }
}