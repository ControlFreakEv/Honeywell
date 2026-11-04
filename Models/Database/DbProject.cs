using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbProject : IDeleteOnImport
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; } = null!;

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        public virtual ICollection<DbTdcCL> CLs { get; set; } = null!;
        public virtual ICollection<DbTdcTag> Tags { get; set; } = null!;

        public DbProject()
        {

        }
    }
}
