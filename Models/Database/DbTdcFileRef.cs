using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcFileRef
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int TagId { get; set; }

        [ImportAndExport(true, true)]
        public string FileType { get; set; }

        [ImportAndExport(true, true)]
        public string Value { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcTag Tag { get; set; }

        public DbTdcFileRef()
        {

        }

        public DbTdcFileRef(int id, int tagId, Helper.FileType fileType, string value)
        {
            Id = id;
            TagId = tagId;
            FileType = fileType.ToString();
            Value = value;
        }


    }
}
