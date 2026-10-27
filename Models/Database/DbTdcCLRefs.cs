using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcCLRefs
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int TagId { get; set; }

        [ImportAndExport(true, true)]
        public int CLId { get; set; }

        [ImportAndExport(true, true)]
        public bool CLAttachedToThisPoint { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcTag Tag { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcCL CL { get; set; }

        public DbTdcCLRefs()
        {

        }

        public DbTdcCLRefs(int id, TdcTag tag, TdcCL cl)
        {
            Id = id;
            TagId = tag.Id;
            CLId = cl.Id;
            CLAttachedToThisPoint = cl.TagsCLIsAttachedTo.Contains(tag.Name);
        }
    }
}
