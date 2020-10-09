using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using WhereTheFile.Database;
using WhereTheFile.Types;
using static WhereTheFile.Tests.TestHelpers;

namespace WhereTheFile.Tests
{
    public class InMemoryTests
    {
        private const int Megabyte = 1024 * 1024;

        private WTFContext context = null;

        [Test]
        public void TestDuplicatesFound()
        {
            List<ScannedFileInfo> testFiles = new List<ScannedFileInfo>()
            {
                CreateTestFile(@"T:\folder1\a", 4*Megabyte),
                CreateTestFile(@"T:\folder1\b", 2*Megabyte),
                CreateTestFile(@"T:\folder2\c", 3*Megabyte),
                CreateTestFile(@"T:\folder2\d", 2*Megabyte),
            };

            var dupes = testFiles.GetDuplicates(false, 0);
            var files = dupes.SelectMany(d => d).ToList();
            var fileNames = files.Select(f => f.FullPath);

            Assert.True(fileNames.Contains(@"T:\folder1\b"));
            Assert.True(fileNames.Contains(@"T:\folder2\d"));
        }
    }
}