using Microsoft.EntityFrameworkCore;
using WhereTheFile.Types;

namespace WhereTheFile.Database
{
    public interface IWTFContext
    {
        DbSet<ScannedFileInfo> FilePaths { get; set; }
    }
}