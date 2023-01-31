using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcConnections
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int NodeId { get; set; }

        [ImportAndExport(true, true)]
        public string Parameter { get; set; }

        [ImportAndExport(true, true)]
        public string ConnectedNodeName { get; set; }

        [ImportAndExport(true, true)]
        public string? ConnectedNodeParameter { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcNode? Node { get; set; }
    }
}
