using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbTdcSourceCrossComm
    {
        [Key]
        public int Id { get; set; }
        public int NodeId { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        public virtual DbTdcNode? Node { get; set; }
    }
}
