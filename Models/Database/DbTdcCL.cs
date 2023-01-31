using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcCL
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int? ProjectId { get; set; }

        [ImportAndExport(true, true)]
        public string FileName { get; set; }
        [ImportAndExport(true, true)]
        public string? CLName { get; set; }
        public string OriginalContent { get; set; }
        public string Content { get; set; }
        public string? Indicators { get; set; }

        [ImportAndExport(true, true)]
        public string? Notes { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbProject? Project { get; set; }
        public virtual ICollection<DbTdcCLRefs> CLTagReferences { get; set; }

        public DbTdcCL()
        {

        }

        public DbTdcCL(TdcCL tdcCL)
        {
            Id = tdcCL.Id;
            CLName = tdcCL.CLName;
            FileName = tdcCL.FileName;
            Content = tdcCL.Content;
            OriginalContent = tdcCL.OriginalContent;
        }
    }
}
