using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbTdcFileRef : IDeleteOnImport
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int TagId { get; set; }

        [ImportAndExport(true, true)]
        public string FileType { get; set; } = null!;

        [ImportAndExport(true, true)]
        public string Value { get; set; } = null!;

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcTag Tag { get; set; } = null!;

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
