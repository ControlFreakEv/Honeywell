using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbProject
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        public virtual ICollection<DbTdcCL> CLs { get; set; }
        public virtual ICollection<DbTdcTag> Tags { get; set; }

        public DbProject()
        {

        }
    }
}
