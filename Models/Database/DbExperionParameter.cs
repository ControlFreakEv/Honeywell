using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbExperionParameter
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int FunctionBlockId { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; }

        [ImportAndExport(true, true)]
        public string? Value { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbExperionFunctionBlock? FunctionBlock { get; set; }

        public DbExperionParameter()
        {

        }
    }
}
