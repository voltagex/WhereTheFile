using NUnit.Framework;
using System.Linq;
using static WhereTheFile.Tests.TestHelpers;
namespace WhereTheFile.Tests
{
    public class PerformanceTests
    {
        [Test]
        public void TestGenerateStatistics()
        {
            var context = CreateMemoryBackedContextFromSQL("Data\\kernel.sql");
            TestContext.WriteLine($"loaded {context.FilePaths.Count()} records");
            TestContext.WriteLine(context.GenerateStatistics());
        }

        [Test]
        public void ScanC()
        {
            var context = CreateMemoryBackedContext();
            var files  = FileIndexHelpers.ScanFiles("C:\\");
            context.FilePaths.AddRange(files);
            TestContext.WriteLine($"loaded {context.FilePaths.Count()} records");
        }
    }
}
