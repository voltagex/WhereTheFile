using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WhereTheFile.Types
{
    public class Duplicates
    {
        [Key] public int Id { get; set; }
        public string FullPath { get; set; }

        public long Size { get; set; }

        [ForeignKey("Drive")]
        public string DriveId { get; set; }

        public DriveInfo Drive { get; set; }

        public int DuplicateCount { get; set; }

        public string DuplicateIds { get; set; }
    }
}
