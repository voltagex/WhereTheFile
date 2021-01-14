using NUnit.Framework;
using System.Linq;
using WhereTheFile.Scanners;
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
            TestContext.WriteLine(context.FilePaths.GenerateStatistics());
        }

        [Test]
        public void ScanC()
        {
            var context = CreateMemoryBackedContext();
            var scanner = new WindowsPathScanner(context, null);
            var files  = scanner.ScanFiles("C:\\");
            var count = context.SaveChanges();
            TestContext.WriteLine($"loaded {count} records");
        }
    }
}
