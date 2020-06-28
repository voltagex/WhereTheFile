using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhereTheFile.Database;
using WhereTheFile.Types;
using static WhereTheFile.Tests.TestHelpers;
namespace WhereTheFile.Tests
{
    public class UpdateDeleteTests
    {
        private const int Megabyte = 1024 * 1024;
        private WTFContext context = null;

        [SetUp]
        public void Setup()
        {
            context = CreateMemoryBackedContext();
        }

        [Test]
        public void UpdateAndDelete()
        {
            //this isn't a very good test, but it helped me work out that I needed a custom comparer
            var firstList = new[] {
                CreateTestFile("T:\\test1", Megabyte),
                CreateTestFile("T:\\test2", Megabyte),
                CreateTestFile("T:\\test3", Megabyte)
            };

            var secondList = new[] {
                CreateTestFile("T:\\test1", Megabyte),
                CreateTestFile("T:\\test2", Megabyte),
            };

            context.AddRange(firstList);
            context.SaveChanges();

            var delete = firstList.Except(secondList, new CompareFileNameOnly()).Select(f => f.FullPath);
            var entitiesToDelete = context.FilePaths.Where(f => delete.Contains(f.FullPath));
            context.FilePaths.RemoveRange(entitiesToDelete);
            context.SaveChanges();

            var deletedFileDoesNotExist = !context.FilePaths.Any(t => t.FullPath == "T:\\test3");

            Assert.IsTrue(deletedFileDoesNotExist);
        }
    }
}
