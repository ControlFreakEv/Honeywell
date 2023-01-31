using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbExperionFunctionBlock
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }
        public int ConfigTemplateId { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; }

        [ImportAndExport(true, true)]
        public string NewName { get; set; }

        [ImportAndExport(true, true)]
        public string Type { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        public virtual DbConfigTemplates? ConfigTemplate { get; set; }
        public virtual ICollection<DbExperionParameter> Parameters { get; set; }

        public DbExperionFunctionBlock()
        {

        }
    }
}
