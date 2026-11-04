using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbTdcConnections : IDeleteOnImport
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int NodeId { get; set; }

        [ImportAndExport(true, true)]
        public string Parameter { get; set; } = null!;

        [ImportAndExport(true, true)]
        public string ConnectedNodeName { get; set; } = null!;

        [ImportAndExport(true, true)]
        public string? ConnectedNodeParameter { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcNode? Node { get; set; }
    }
}
