using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using SQLitePCL;

namespace WhereTheFile.Types
{
    public class ScannedFileInfo
    {
        [Key] public int Id { get; set; }
        public string FullPath { get; set; }

        public long Size { get; set; }

        public FileAttributes Attributes { get; set; }

        public DateTime FileCreated { get; set; }
    }
}
