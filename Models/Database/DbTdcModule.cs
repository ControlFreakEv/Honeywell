using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcModule
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int? ConfigTemplateId { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; }

        [ImportAndExport(true, true)]
        public string? Desc { get; set; }

        [ImportAndExport(true, true)]
        public string? CEE { get; set; }

        [ImportAndExport(true, true)]
        public string? Notes { get; set; }

        [ImportAndExport(true, true)]
        public string? Interlocks { get; set; }

        [ImportAndExport(true, true)]
        public string? Permissives { get; set; }

        [ImportAndExport(true, true)]
        public string? MiscLogic { get; set; }

        [ImportAndExport(true, true)]
        public string? Alarms { get; set; }

        [ImportAndExport(true, true)]
        public string? AlarmSuppression { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbConfigTemplates? ConfigTemplate { get; set; }
        public virtual ICollection<DbTdcNode> Nodes { get; set; }
        public virtual ICollection<DbExperionParameter> ExperionParameters { get; set; }

        public DbTdcModule()
        {

        }

        public DbTdcModule(TdcModule tdcModule)
        {
            Id = tdcModule.Id;
            Name = tdcModule.Name;
            Desc = tdcModule.Desc;
        }
    }
}
