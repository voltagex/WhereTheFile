using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereTheFile.Types;

namespace WhereTheFile
{
    public static class ScannedFileInfoExtensions
    {
        public static IEnumerable<ScannedFileInfo> FindFilesByPath(this IEnumerable<ScannedFileInfo> scannedFiles, string search)
        {
            return scannedFiles.Where(r => r.FullPath.Contains(search)).OrderByDescending(r => r.Size);
        }


        public static IEnumerable<IGrouping<long, ScannedFileInfo>> GetDuplicates(this IEnumerable<ScannedFileInfo> scannedFiles, bool orderByNumberOfDuplicates, int excludeLessThanMegabytes = 5)
        {
            var dupes = scannedFiles.Where(d => d.Size > 1024 * 1024 * excludeLessThanMegabytes).AsEnumerable()
                .OrderByDescending(r => r.Size).GroupBy(r => r.Size)
                .Where(g => g.Count() > 1);


            if (orderByNumberOfDuplicates)
            {
                dupes = dupes.OrderByDescending(g => g.Count());
            }

            return dupes;
        }

        public static string GenerateStatistics(this IEnumerable<ScannedFileInfo> scannedFiles)
        {
            //todo: return some kind of type instead of a string

            var totalSize = scannedFiles.Sum(f => f.Size);
            var totalFiles = scannedFiles.Count();
            var largestFile = scannedFiles.OrderByDescending(f => f.Size).FirstOrDefault();

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Statistics:");
            builder.AppendLine(
                $"{totalFiles} files indexed with a combined size of {(totalSize / 1024 / 1024)} megabytes ({totalSize / 1024 / 1024 / 1024} gigabytes or {(totalSize / 1024 / 1024 / 1024 / 1024)} terabytes)");
            if (totalFiles > 0) //don't crash on an empty database
            {
                builder.AppendLine(
                    $"The largest file indexed is {largestFile.FullPath} at {(largestFile.Size / 1024 / 1024)} megabytes");
            }

            var topTen = scannedFiles.OrderByDescending(f => f.Size).Take(10);

            builder.AppendLine();
            builder.AppendLine("Top ten largest files:");
            foreach (var topFile in topTen)
            {
                builder.AppendLine($"{topFile.FullPath}: {(topFile.Size / 1024 / 1024)} megabytes)");
            }

            return builder.ToString();
        }
    }
}
