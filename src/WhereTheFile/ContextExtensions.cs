using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereTheFile.Database;
using WhereTheFile.Types;

namespace WhereTheFile
{
    public static class ContextExtensions
    {
        public static IEnumerable<ScannedFileInfo> FindFilesByPath(this WTFContext context, string search)
        {
            return context.FilePaths.Where(r => r.FullPath.Contains(search)).OrderByDescending(r => r.Size);
        }


        public static IEnumerable<IGrouping<long, ScannedFileInfo>> GetDuplicates(this WTFContext context, bool orderByNumberOfDuplicates, int excludeLessThanMegabytes = 5)
        {
            var dupes = context.FilePaths.Where(d => d.Size > 1024 * 1024 * excludeLessThanMegabytes).AsEnumerable()
                .OrderByDescending(r => r.Size).GroupBy(r => r.Size)
                .Where(g => g.Count() > 1);


            if (orderByNumberOfDuplicates)
            {
                dupes = dupes.OrderByDescending(g => g.Count());
            }

            return dupes;
        }

        public static string GenerateStatistics(this WTFContext context)
        {

            //todo: return some kind of type instead of a string

            var totalSize = context.FilePaths.Sum(f => f.Size);
            var totalFiles = context.FilePaths.Count();
            var largestFile = context.FilePaths.OrderByDescending(f => f.Size).FirstOrDefault();

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Statistics:");
            builder.AppendLine(
                $"{totalFiles} files indexed with a combined size of {(totalSize / 1024 / 1024)} megabytes ({totalSize / 1024 / 1024 / 1024} gigabytes or {(totalSize / 1024 / 1024 / 1024 / 1024)} terabytes)");
            if (totalFiles > 0) //don't crash on an empty database
            {
                builder.AppendLine(
                    $"The largest file indexed is {largestFile.FullPath} at {(largestFile.Size / 1024 / 1024)} megabytes");
            }

            return builder.ToString();
        }
    }
}
