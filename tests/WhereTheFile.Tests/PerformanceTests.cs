using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereTheFile.Database;
using static WhereTheFile.Tests.TestHelpers;
namespace WhereTheFile.Tests
{
    public class PerformanceTests
    {
        private WTFContext context;
        private FileIndexHelpers helpers;

        [SetUp]
        public void Setup()
        {
            context = CreateMemoryBackedContextFromSQL("Data\\kernel.sql");
            TestContext.WriteLine($"loaded {context.FilePaths.Count()} records");
            helpers = new FileIndexHelpers(context);
        }

        [Test]
        public void TestGenerateStatistics()
        {
            TestContext.WriteLine(helpers.GenerateStatistics());
        }
    }
}
