using NUnit.Framework;
using System.Collections.Generic;
using WhereTheFile.Database;
using WhereTheFile.Types;
using static WhereTheFile.Tests.TestHelpers;

namespace WhereTheFile.Tests
{
    public class DuplicateTests
    {

        private const int Megabyte = 1024 * 1024;
 

        [Test]
        public void FindDuplicateWithSizeAndName()

        {
            IEnumerable<ScannedFileInfo> testFiles = new List<ScannedFileInfo>()
            {
                CreateTestFile(@"T:\same_size_different_name\a", 4*Megabyte),
                CreateTestFile(@"T:\same_size_different_name\b", 4*Megabyte),
                CreateTestFile(@"T:\same_name_different_size\c", 3*Megabyte),
                CreateTestFile(@"T:\same_name_different_size\2\c", 2*Megabyte),
            };
            var duplicates = testFiles.GetDuplicates(false);
            
            

            Assert.Fail();
        }
    }
}
