using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbTdcGraph
    {
        [Key]
        public int Id { get; set; }
        public string Xml { get; set; } = null!;

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }
        public virtual ICollection<DbTdcNode> Nodes { get; set; } = null!;
    }
}
