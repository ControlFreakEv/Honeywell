using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcNode
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public string NodeId { get; set; }

        [ImportAndExport(true, true)]
        public int? TagId { get; set; }

        [ImportAndExport(true, true)]
        public int GraphId { get; set; }

        [ImportAndExport(true, true)]
        public int? ModuleId { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcTag? Tag { get; set; }
        public virtual DbTdcGraph Graph { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcModule? Module { get; set; }
        public virtual ICollection<DbTdcSourceCrossComm> SourceCrossComms { get; set; }
        public virtual ICollection<DbTdcTargetCrossComm> TargetCrossComms { get; set; }
        public virtual ICollection<DbTdcConnections> ParameterConnections { get; set; }
    }
}
