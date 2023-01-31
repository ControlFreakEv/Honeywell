using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbConfigTemplates
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public string TypicalName { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }
        public virtual ICollection<DbTdcModule> Modules { get; set; }
        public virtual ICollection<DbExperionFunctionBlock> FunctionBlocks { get; set; }

        public DbConfigTemplates()
        {

        }
    }
}
